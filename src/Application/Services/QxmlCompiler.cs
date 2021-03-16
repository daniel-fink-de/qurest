using NCalc;
using QuRest.Application.Abstractions;
using QuRest.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace QuRest.Application.Services
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class QxmlCompiler : IQxmlCompiler
    {
        private QuantumCircuit compilation = new QuantumCircuit { Name = "Empty" };
        private QuantumCircuit circuitToCompile = new QuantumCircuit { Name = "Empty" };
        private readonly IList<string> loopVariableNames = new List<string>();

        // current variable value, max value, index of starting the loop
        private readonly IDictionary<string, (double, double, int)> loopVariableValues = new Dictionary<string, (double, double, int)>();
        private readonly IDictionary<string, QuantumCircuit> placeholderMapping = new Dictionary<string, QuantumCircuit>();
        private readonly IDictionary<string, double> parameterMapping = new Dictionary<string, double>();

        public IQxmlCompiler WithPlaceholderMapping(IDictionary<string, QuantumCircuit> mapping)
        {
            foreach (var (name, alg) in mapping)
            {
                this.placeholderMapping[name] = alg;
            }

            return this;
        }

        public IQxmlCompiler WithParameterMapping(IDictionary<string, double> mapping)
        {
            foreach (var (name, value) in mapping)
            {
                this.parameterMapping[name] = value;
            }

            return this;
        }

        public IQxmlCompiler AddPlaceholderMapping(string placeholderName, QuantumCircuit quantumCircuit)
        {
            this.placeholderMapping[placeholderName] = quantumCircuit;

            return this;
        }

        public IQxmlCompiler AddParameterMapping(string parameterName, double value)
        {
            this.parameterMapping[parameterName] = value;
            return this;

        }

        public async Task<QuantumCircuit> CompileAsync(QuantumCircuit quantumCircuit)
        {
            // clear everything from previous
            this.loopVariableNames.Clear();
            this.loopVariableValues.Clear();
            this.circuitToCompile = quantumCircuit;
            this.compilation = new QuantumCircuit().WithName(quantumCircuit.Name + "_compilation");

            // add all the parameters from old circuitToCompile
            foreach (var parameter in this.circuitToCompile.ParameterList)
            {
                this.compilation.WithParameter(parameter);
            }

            // if the size is int already, use it
            var sizeIsInt = this.TryParseInt(this.circuitToCompile.Size, out string intString);
            if (sizeIsInt)
            {
                this.circuitToCompile.Size = intString;
                this.compilation.Size = intString;
            }
            // if not, create a parameter for it
            else
            {
                this.circuitToCompile.WithParameter(this.circuitToCompile.Size);
            }

            if (!string.IsNullOrEmpty(quantumCircuit.Description))
            {
                this.compilation.WithDescription(this.ReplaceParameterNameAndLoopVariableWithDoubleValue(quantumCircuit.Description));
            }

            // run the compilation recursively
            await Task.Run(() => this.CompileRecursive(0, this.circuitToCompile));

            // if size was not already int, replace now with parameter mapping 
            if (!sizeIsInt)
            {
                this.compilation.Size = this.ReplaceParameterNameAndLoopVariableWithDoubleValue(intString);
            }

            // delete all parameters
            this.compilation.Parameters = null;

            return this.compilation;
        }

        private bool TryParseInt(string valueString, out string value)
        {
            if (string.IsNullOrEmpty(valueString))
            {
                value = string.Empty;
                return false;
            }

            if (double.TryParse(valueString, out var doubleValue))
            {
                value = ((int)doubleValue).ToString();
                return true;
            }

            value = valueString;
            return false;
        }

        private bool TryParseDouble(string valueString, out string value)
        {
            if (string.IsNullOrEmpty(valueString))
            {
                value = string.Empty;
                return false;
            }

            if (double.TryParse(valueString, out double doubleValue))
            {
                value = doubleValue.ToString(new CultureInfo("en-US"));
                return true;
            }

            value = valueString;
            return false;
        }

        private string ReplaceLoopVariableWithDoubleValue(string source)
        {
            foreach (var loopVariable in this.loopVariableNames)
            {
                if (source.Contains(loopVariable))
                {
                    source = source.Replace(
                        loopVariable, 
                        Convert.ToDouble(this.loopVariableValues[loopVariable].Item1)
                            .ToString(new CultureInfo("en-US")));
                }
            }
            return source;
        }

        private string? ReplaceParameterNameAndLoopVariableWithDoubleValue(string? source)
        {
            if (string.IsNullOrEmpty(source)) return null;

            source = this.ReplaceLoopVariableWithDoubleValue(source);

            var hasParameter = false;
            foreach (var parameter in this.circuitToCompile.ParameterList)
            {
                var formattedParameter = this.ReplaceLoopVariableWithDoubleValue(parameter);
                if (!source.Contains(formattedParameter)) continue;

                if (!this.parameterMapping.ContainsKey(formattedParameter))
                {
                    throw new InvalidOperationException($"No mapping for parameter \"{formattedParameter}\" provided.");
                }

                source = source.Replace(formattedParameter, Convert.ToDouble(this.parameterMapping[formattedParameter]).ToString(new CultureInfo("en-US")));
                hasParameter = true;
            }

            return !hasParameter ? source : this.ReplaceParameterNameAndLoopVariableWithDoubleValue(source);
        }

        private string? ReplaceParameterNameAndLoopVariableWithIntValue(string? source)
        {
            if (string.IsNullOrEmpty(source)) return null;

            source = this.ReplaceLoopVariableWithDoubleValue(source);

            var hasParameter = false;
            foreach (var parameter in this.circuitToCompile.ParameterList)
            {
                var formattedParameter = this.ReplaceLoopVariableWithDoubleValue(parameter);
                if (!source.Contains(formattedParameter)) continue;

                if (!this.parameterMapping.ContainsKey(formattedParameter))
                {
                    throw new InvalidOperationException($"No mapping for parameter \"{formattedParameter}\" provided.");
                }

                source = source.Replace(formattedParameter, Convert.ToInt32(this.parameterMapping[formattedParameter]).ToString(new CultureInfo("en-US")));
                hasParameter = true;
            }

            return !hasParameter ? source : this.ReplaceParameterNameAndLoopVariableWithDoubleValue(source);
        }

        private string? ConvertStringExpressionsToString<T>(string? expression)
        {
            if (string.IsNullOrEmpty(expression)) return null;

            var expressions = new List<string?>();

            foreach (var singleExpression in expression.Split(','))
            {
                var expr = new Expression(singleExpression);
                Func<T> f = expr.ToLambda<T>();
                var result = f();

                if (result == null)
                {
                    continue;
                }
                if (result is double)
                {
                    var resultDouble = (double)(object)result;
                    expressions.Add(resultDouble.ToString(new CultureInfo("en-US")));
                }
                else
                {
                    expressions.Add(result.ToString());
                }
            }

            return expression.Length switch
            {
                0 => null,
                1 => expression[0].ToString(),
                _ => expressions.Aggregate((t1, t2) => $"{t1},{t2}")
            };
        }

        private T ConvertStringExpression<T>(string? expression)
        {
            var expr = new Expression(expression);
            Func<T> f = expr.ToLambda<T>();

            return f();
        }

        private int CompileRecursive(int index, QuantumCircuit circuit)
        {
            while (true)
            {
                // recursive break condition
                // if we finished all steps
                if (index >= circuit.Steps.Count) return 1000000;
                var step = GetStepFromCircuit(index, circuit);

                switch (step.Type)
                {
                    case StepType.Unitarian:
                        var unitarian = this.GetUnitarianFromCircuit(index, circuit);
                        this.AddUnitarian(unitarian);
                        break;
                    case StepType.Hermitian:
                        var hermitian = this.GetHermitianFromCircuit(index, circuit);
                        this.AddHermitian(hermitian);
                        break;
                    case StepType.Placeholder:
                        var placeholder = this.GetPlaceholderFromCircuit(index, circuit);
                        this.ResolvePlaceholderAndAddCircuit(placeholder);
                        break;
                    case StepType.Loop:
                        var loop = this.GetLoopFromCircuit(index, circuit);
                        index = this.AddLoop(loop, circuit);
                        break;
                    case StepType.Condition:
                        var branch = this.GetConditionFromCircuit(index, circuit);
                        index = this.AddCondition(branch, circuit);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                index += 1;
            }
        }

        private Step GetStepFromCircuit(int index, QuantumCircuit circuit)
        {
            var step = circuit.Steps.FirstOrDefault(s => s.Index == index);
            if (step is null)
            {
                throw new InvalidOperationException($"The step with the index \"{index}\" could not be found.");
            }

            return step;
        }

        private Unitarian GetUnitarianFromCircuit(int index, QuantumCircuit circuit)
        {
            var unitarian = circuit.Unitarians.FirstOrDefault(u => u.Index == index);
            if (unitarian is null)
            {
                throw new InvalidOperationException($"The unitarian with the index \"{index}\" could not be found.");
            }

            return unitarian;
        }

        private Hermitian GetHermitianFromCircuit(int index, QuantumCircuit circuit)
        {
            var hermitian = circuit.Hermitians.FirstOrDefault(h => h.Index == index);
            if (hermitian is null)
            {
                throw new InvalidOperationException($"The hermitian with the index \"{index}\" could not be found.");
            }

            return hermitian;
        }

        private Condition GetConditionFromCircuit(int index, QuantumCircuit circuit)
        {
            var branch = circuit.Conditions.FirstOrDefault(b => b.Index == index);
            if (branch is null)
            {
                throw new InvalidOperationException($"The condition with the index \"{index}\" could not be found.");
            }

            return branch;
        }

        private Loop GetLoopFromCircuit(int index, QuantumCircuit circuit)
        {
            var loop = circuit.Loops.FirstOrDefault(l => l.Index == index);
            if (loop is null)
            {
                throw new InvalidOperationException($"The loop with the index \"{index}\" could not be found.");
            }

            return loop;
        }

        private Placeholder GetPlaceholderFromCircuit(int index, QuantumCircuit circuit)
        {
            var placeholder = circuit.Placeholders.FirstOrDefault(p => p.Index == index);
            if (placeholder is null)
            {
                throw new InvalidOperationException($"The placeholder with the index \"{index}\" could not be found.");
            }

            return placeholder;
        }

        private int AddLoop(Loop loop, QuantumCircuit circuit)
        {
            int index;

            switch (loop.Type)
            {
                case LoopControlType.ForLoopStart:
                {
                    var startString = this.ReplaceParameterNameAndLoopVariableWithDoubleValue(loop.Start);
                    var endString = this.ReplaceParameterNameAndLoopVariableWithDoubleValue(loop.End);
                    var incrementString = this.ReplaceParameterNameAndLoopVariableWithDoubleValue(loop.Increment);

                    var start = this.ConvertStringExpression<double>(startString);
                    var end = this.ConvertStringExpression<double>(endString);
                    var increment = this.ConvertStringExpression<double>(incrementString);

                    var variable = loop.Variable;
                    this.loopVariableNames.Add(variable);

                    var tempIndex = loop.Index;

                    var indexAfterLoop = 0;
                    var amountOfLoops = 1;
                    foreach (var step in circuit.Steps)
                    {
                        if (step.Index <= tempIndex || step.Type != StepType.Loop) continue;

                        var loopEnd = this.GetLoopFromCircuit(step.Index, circuit);
                        if (loopEnd.Type == LoopControlType.ForLoopStart)
                        {
                            amountOfLoops++;
                        }
                        else
                        {
                            if (amountOfLoops == 1)
                            {
                                indexAfterLoop = step.Index;
                                break;
                            }
                            amountOfLoops--;
                        }
                    }

                    for (var i = start; i < end; i += increment)
                    {
                        this.loopVariableValues[variable] = (i, end, tempIndex);
                        this.CompileRecursive(tempIndex + 1, circuit);
                    }
                    index = indexAfterLoop;
                    break;
                }
                case LoopControlType.ForLoopEnd:
                {
                    var variable = this.loopVariableNames.Last();
                    index = 100000;

                    if (!(this.loopVariableValues[variable].Item1 >= this.loopVariableValues[variable].Item2))
                    {
                        return index;
                    }

                    this.loopVariableNames.Remove(variable);
                    this.loopVariableValues.Remove(variable);
                    break;
                }
                default:
                    throw new InvalidOperationException("Unknown type of loop.");
            }

            return index;
        }

        private void AddUnitarian(Unitarian unitarian)
        {
            var qubitString = this.ReplaceParameterNameAndLoopVariableWithIntValue(unitarian.Qubits);
            qubitString = ConvertStringExpressionsToString<int>(qubitString);

            if (string.IsNullOrEmpty(qubitString))
            {
                throw new InvalidOperationException($"The expression \"{qubitString}\" is not valid for qubits.");
            }

            var parameterString = this.ReplaceParameterNameAndLoopVariableWithDoubleValue(unitarian.Parameters);
            parameterString = ConvertStringExpressionsToString<double>(parameterString);

            this.compilation.Unitarian(unitarian.Type, qubitString, parameterString);
        }

        private void AddHermitian(Hermitian hermitian)
        {
            var qubitString = this.ReplaceParameterNameAndLoopVariableWithIntValue(hermitian.Qubits);
            qubitString = ConvertStringExpressionsToString<int>(qubitString);

            if (string.IsNullOrEmpty(qubitString))
            {
                throw new InvalidOperationException($"The expression \"{qubitString}\" is not valid for qubits.");
            }

            var parameterString = this.ReplaceParameterNameAndLoopVariableWithDoubleValue(hermitian.Parameters);
            parameterString = ConvertStringExpressionsToString<double>(parameterString);

            this.compilation.Hermitian(hermitian.Type, qubitString, parameterString);
        }

        private void ResolvePlaceholderAndAddCircuit(Placeholder placeholder)
        {
            if (!this.placeholderMapping.ContainsKey(placeholder.Name))
            {
                throw new InvalidOperationException($"The placeholder with the name \"{placeholder.Name}\" has no mapping.");
            }
            this.AddCircuit(this.placeholderMapping[placeholder.Name]);
        }

        private void AddCircuit(QuantumCircuit circuit)
        {
            foreach (var parameter in circuit.ParameterList)
            {
                this.compilation.WithParameter(parameter);
            }

            this.CompileRecursive(0, circuit);
        }

        private int AddCondition(Condition condition, QuantumCircuit circuit)
        {
            if (condition.Type != ConditionControlType.If)
            {
                return 100000;
            }

            var currentConditions = this.GetCurrentConditions(condition.Index, circuit);
            var indexAfterCondition = currentConditions.First(cc => cc.Value == ConditionControlType.EndIf).Key + 1;
            var indexToGoTo = indexAfterCondition;

            var ifExpression = condition.Expression;
            ifExpression = this.ReplaceParameterNameAndLoopVariableWithDoubleValue(ifExpression);
            if (this.ConvertStringExpression<bool>(ifExpression))
            {
                indexToGoTo = condition.Index + 1;
            }
            else
            {
                var elseIfMatched = false;
                foreach (var (index, type) in currentConditions)
                {
                    if (type != ConditionControlType.ElseIf) continue;

                    var elseIfExpression = circuit.Conditions.First(cond => cond.Index == index).Expression;
                    elseIfExpression = this.ReplaceParameterNameAndLoopVariableWithDoubleValue(elseIfExpression);
                    if (!this.ConvertStringExpression<bool>(elseIfExpression)) continue;

                    indexToGoTo = index + 1;
                    elseIfMatched = true;
                    break;
                }

                if (!elseIfMatched && currentConditions.Any(cc => cc.Value == ConditionControlType.Else))
                {
                    indexToGoTo = currentConditions.First(cc => cc.Value == ConditionControlType.Else).Key + 1;
                }
            }

            this.CompileRecursive(indexToGoTo, circuit);
            return indexAfterCondition;
        }

        private IDictionary<int, ConditionControlType> GetCurrentConditions(int ifIndex, QuantumCircuit circuit)
        {
            var amountNestedIfs = 0;
            var result = new Dictionary<int, ConditionControlType>();
            for (var index = ifIndex + 1; index < circuit.Steps.Count; index++)
            {
                if (circuit.Steps.First(step => step.Index == index).Type == StepType.Condition)
                {
                    switch (circuit.Conditions.First(cond => cond.Index == index).Type)
                    {
                        case ConditionControlType.If:
                            amountNestedIfs++;
                            break;
                        case ConditionControlType.ElseIf:
                            if (amountNestedIfs == 0)
                                result[index] = ConditionControlType.ElseIf;
                            break;
                        case ConditionControlType.Else:
                            if (amountNestedIfs == 0)
                                result[index] = ConditionControlType.Else;
                            break;
                        case ConditionControlType.EndIf:
                            if (amountNestedIfs == 0)
                                result[index] = ConditionControlType.EndIf;
                            else
                                amountNestedIfs--;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return result;
        }
    }
}
