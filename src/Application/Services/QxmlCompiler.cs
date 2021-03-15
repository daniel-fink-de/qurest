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
        private QuantumCircuit compilation = new() { Name = "Empty" };
        private QuantumCircuit algorithmToCompile = new() { Name = "Empty" };
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

        public IQxmlCompiler AddPlaceholderMapping(string placeholderName, QuantumCircuit algorithm)
        {
            this.placeholderMapping[placeholderName] = algorithm;

            return this;
        }

        public IQxmlCompiler AddParameterMapping(string parameterName, double value)
        {
            this.parameterMapping[parameterName] = value;
            return this;

        }

        public async Task<QuantumCircuit> CompileAsync(QuantumCircuit algorithm)
        {
            // clear everything from previous
            this.loopVariableNames.Clear();
            this.loopVariableValues.Clear();
            this.algorithmToCompile = algorithm;
            this.compilation = new QuantumCircuit().WithName(algorithm.Name + "_compilation");

            // add all the parameters from old algorithmToCompile
            foreach (var parameter in this.algorithmToCompile.ParameterList)
            {
                this.compilation.WithParameter(parameter);
            }

            // if the size is int already, use it
            var sizeIsInt = this.TryParseInt(this.algorithmToCompile.Size, out string intString);
            if (sizeIsInt)
            {
                this.algorithmToCompile.Size = intString;
                this.compilation.Size = intString;
            }
            // if not, create a parameter for it
            else
            {
                this.algorithmToCompile.WithParameter(this.algorithmToCompile.Size);
            }

            if (!string.IsNullOrEmpty(algorithm.Description))
            {
                this.compilation.WithDescription(this.ReplaceParameterNameAndLoopVariableWithDoubleValue(algorithm.Description));
            }

            // run the compilation recursively
            await Task.Run(() => this.CompileRecursive(0, this.algorithmToCompile));

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
            foreach (var parameter in this.algorithmToCompile.ParameterList)
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
            foreach (var parameter in this.algorithmToCompile.ParameterList)
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

        private int CompileRecursive(int index, QuantumCircuit algorithm)
        {
            while (true)
            {
                // recursive break condition
                // if we finished all steps
                if (index >= algorithm.Steps.Count) return 1000000;
                var step = GetStepFromAlgorithm(index, algorithm);

                switch (step.Type)
                {
                    case StepType.Unitarian:
                        var unitarian = this.GetUnitarianFromAlgorithm(index, algorithm);
                        this.AddUnitarian(unitarian);
                        break;
                    case StepType.Hermitian:
                        var hermitian = this.GetHermitianFromAlgorithm(index, algorithm);
                        this.AddHermitian(hermitian);
                        break;
                    case StepType.Placeholder:
                        var placeholder = this.GetPlaceholderFromAlgorithm(index, algorithm);
                        this.ResolvePlaceholderAndAddAlgorithm(placeholder);
                        break;
                    case StepType.Loop:
                        var loop = this.GetLoopFromAlgorithm(index, algorithm);
                        index = this.AddLoop(loop, algorithm);
                        break;
                    case StepType.Condition:
                        var branch = this.GetConditionFromAlgorithm(index, algorithm);
                        index = this.AddCondition(branch, algorithm);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                index += 1;
            }
        }

        private Step GetStepFromAlgorithm(int index, QuantumCircuit algorithm)
        {
            var step = algorithm.Steps.FirstOrDefault(s => s.Index == index);
            if (step is null)
            {
                throw new InvalidOperationException($"The step with the index \"{index}\" could not be found.");
            }

            return step;
        }

        private Unitarian GetUnitarianFromAlgorithm(int index, QuantumCircuit algorithm)
        {
            var unitarian = algorithm.Unitarians.FirstOrDefault(u => u.Index == index);
            if (unitarian is null)
            {
                throw new InvalidOperationException($"The unitarian with the index \"{index}\" could not be found.");
            }

            return unitarian;
        }

        private Hermitian GetHermitianFromAlgorithm(int index, QuantumCircuit algorithm)
        {
            var hermitian = algorithm.Hermitians.FirstOrDefault(h => h.Index == index);
            if (hermitian is null)
            {
                throw new InvalidOperationException($"The hermitian with the index \"{index}\" could not be found.");
            }

            return hermitian;
        }

        private Condition GetConditionFromAlgorithm(int index, QuantumCircuit algorithm)
        {
            var branch = algorithm.Conditions.FirstOrDefault(b => b.Index == index);
            if (branch is null)
            {
                throw new InvalidOperationException($"The condition with the index \"{index}\" could not be found.");
            }

            return branch;
        }

        private Loop GetLoopFromAlgorithm(int index, QuantumCircuit algorithm)
        {
            var loop = algorithm.Loops.FirstOrDefault(l => l.Index == index);
            if (loop is null)
            {
                throw new InvalidOperationException($"The loop with the index \"{index}\" could not be found.");
            }

            return loop;
        }

        private Placeholder GetPlaceholderFromAlgorithm(int index, QuantumCircuit algorithm)
        {
            var placeholder = algorithm.Placeholders.FirstOrDefault(p => p.Index == index);
            if (placeholder is null)
            {
                throw new InvalidOperationException($"The placeholder with the index \"{index}\" could not be found.");
            }

            return placeholder;
        }

        private int AddLoop(Loop loop, QuantumCircuit algorithm)
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
                    foreach (var step in algorithm.Steps)
                    {
                        if (step.Index <= tempIndex || step.Type != StepType.Loop) continue;

                        var loopEnd = this.GetLoopFromAlgorithm(step.Index, algorithm);
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
                        this.CompileRecursive(tempIndex + 1, algorithm);
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

        private void ResolvePlaceholderAndAddAlgorithm(Placeholder placeholder)
        {
            if (!this.placeholderMapping.ContainsKey(placeholder.Name))
            {
                throw new InvalidOperationException($"The placeholder with the name \"{placeholder.Name}\" has no mapping.");
            }
            this.AddAlgorithm(this.placeholderMapping[placeholder.Name]);
        }

        private void AddAlgorithm(QuantumCircuit algorithm)
        {
            foreach (var parameter in algorithm.ParameterList)
            {
                this.compilation.WithParameter(parameter);
            }

            this.CompileRecursive(0, algorithm);
        }

        private int AddCondition(Condition condition, QuantumCircuit algorithm)
        {
            if (condition.Type != ConditionControlType.If)
            {
                return 100000;
            }

            var currentConditions = this.GetCurrentConditions(condition.Index, algorithm);
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

                    var elseIfExpression = algorithm.Conditions.First(cond => cond.Index == index).Expression;
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

            this.CompileRecursive(indexToGoTo, algorithm);
            return indexAfterCondition;
        }

        private IDictionary<int, ConditionControlType> GetCurrentConditions(int ifIndex, QuantumCircuit algorithm)
        {
            var amountNestedIfs = 0;
            var result = new Dictionary<int, ConditionControlType>();
            for (var index = ifIndex + 1; index < algorithm.Steps.Count; index++)
            {
                if (algorithm.Steps.First(step => step.Index == index).Type == StepType.Condition)
                {
                    switch (algorithm.Conditions.First(cond => cond.Index == index).Type)
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
