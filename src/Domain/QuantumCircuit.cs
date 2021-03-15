using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace QuRest.Domain
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public partial class QuantumCircuit : IEntity
    {
        public IEnumerable<string> ParameterList => string.IsNullOrEmpty(this.Parameters) ? Array.Empty<string>() : this.Parameters.Split(',');

        private void AddParameters(string? names)
        {
            var parameterList = string.IsNullOrEmpty(names) ? Array.Empty<string>() : names.Split(',');
            foreach (var parameter in parameterList)
            {
                if (string.IsNullOrEmpty(parameter)) return;
                if (this.ParameterList.Contains(parameter)) return;
                if (double.TryParse(parameter, out _)) return;

                var parametersString = new StringBuilder(this.Parameters);
                if (!string.IsNullOrEmpty(this.Parameters)) parametersString.Append(',');
                parametersString.Append(parameter);
                this.Parameters = parametersString.ToString();
            }
        }

        private string? GenerateParameterString(params string[] parameters)
        {
            var parameterString = parameters.Length switch
            {
                0 => null,
                1 => parameters[0],
                _ => parameters.Aggregate((p1, p2) => $"{p1},{p2}")
            };
            return parameterString;
        }

        private string GenerateQubitString(params string[] qubits)
        {
            string qubitString = qubits.Length switch
            {
                0 => string.Empty,
                1 => qubits[0],
                _ => qubits.Aggregate((q1, q2) => $"{q1},{q2}")
            };
            return qubitString;
        }

        private void AddStep(int index, StepType type)
        {
            this.Steps.Add(new Step {Index = index, Type = type});
        }

        public QuantumCircuit WithName(string name)
        {
            this.Name = name;
            return this;
        }

        public QuantumCircuit WithDescription(string? description)
        {
            this.Description = description;
            return this;
        }

        public QuantumCircuit WithSize(string size)
        {
            this.Size = size;
            return this;
        }

        public QuantumCircuit H(string qubit) => this.Unitarian(UnitarianType.H, this.GenerateQubitString(qubit));

        public QuantumCircuit X(string qubit) => this.Unitarian(UnitarianType.X, this.GenerateQubitString(qubit));

        public QuantumCircuit Z(string qubit) => this.Unitarian(UnitarianType.Z, this.GenerateQubitString(qubit));

        public QuantumCircuit RX(string qubit, string theta) => this.Unitarian(UnitarianType.RX, this.GenerateQubitString(qubit), this.GenerateParameterString(theta));

        public QuantumCircuit CX(string controlQubit, string targetQubit) => this.Unitarian(UnitarianType.CX, this.GenerateQubitString(controlQubit, targetQubit));

        public QuantumCircuit CZ(string controlQubit, string targetQubit) => this.Unitarian(UnitarianType.CZ, this.GenerateQubitString(controlQubit, targetQubit));

        public QuantumCircuit SWAP(string qubit1, string qubit2) => this.Unitarian(UnitarianType.SWAP, this.GenerateQubitString(qubit1, qubit2));

        public QuantumCircuit MX(string qubit) => this.Hermitian(HermitianType.X, this.GenerateQubitString(qubit));

        public QuantumCircuit Unitarian(UnitarianType type, string qubits, string? parameters = null)
        {
            var index = this.Steps.Count;

            var unt = new Unitarian
            {
                Index = index,
                Type = type,
                Qubits = qubits,
                Parameters = parameters
            };
            this.Unitarians.Add(unt);
            this.AddParameters(parameters);
            this.AddStep(index, StepType.Unitarian);
            return this;
        }

        public QuantumCircuit Hermitian(HermitianType type, string qubits, string? parameters = null)
        {
            var index = this.Steps.Count;

            var her = new Hermitian
            {
                Index = index,
                Type = type,
                Qubits = qubits,
                Parameters = parameters
            };
            this.Hermitians.Add(her);
            this.AddParameters(parameters);
            this.AddStep(index, StepType.Hermitian);
            return this;
        }

        public QuantumCircuit Placeholder(string name)
        {
            var index = this.Steps.Count;
            var plh = new Placeholder
            {
                Index = index,
                Name = name
            };
            this.Placeholders.Add(plh);
            this.AddStep(index, StepType.Placeholder);
            return this;
        }

        public QuantumCircuit For(string variable, string start, string end, string increment)
        {
            var index = this.Steps.Count;

            var forStart = new Loop
            {
                Index = index,
                Type = LoopControlType.ForLoopStart,
                Variable = variable,
                Start = start,
                End = end,
                Increment = increment
            };
            this.Loops.Add(forStart);
            this.AddStep(index, StepType.Loop);
            return this;
        }

        public QuantumCircuit EndFor()
        {
            var index = this.Steps.Count;

            var forEnd = new Loop
            {
                Index = index,
                Type = LoopControlType.ForLoopEnd,
            };
            this.Loops.Add(forEnd);
            this.AddStep(index, StepType.Loop);
            return this;
        }

        public QuantumCircuit If(string expression)
        {
            var index = this.Steps.Count;

            var ifBranch = new Condition
            {
                Index = index,
                Type = ConditionControlType.If,
                Expression = expression
            };
            this.Conditions.Add(ifBranch);
            this.AddStep(index, StepType.Condition);
            return this;
        }

        public QuantumCircuit ElseIf(string expression)
        {
            var index = this.Steps.Count;

            var elseIfBranch = new Condition
            {
                Index = index,
                Type = ConditionControlType.ElseIf,
                Expression = expression
            };
            this.Conditions.Add(elseIfBranch);
            this.AddStep(index, StepType.Condition);
            return this;
        }

        public QuantumCircuit Else()
        {
            var index = this.Steps.Count;

            var elseBranch = new Condition
            {
                Index = index,
                Type = ConditionControlType.Else,
            };
            this.Conditions.Add(elseBranch);
            this.AddStep(index, StepType.Condition);
            return this;
        }

        public QuantumCircuit EndIf()
        {
            var index = this.Steps.Count;

            var endBranch = new Condition()
            {
                Index = index,
                Type = ConditionControlType.EndIf,
            };
            this.Conditions.Add(endBranch);
            this.AddStep(index, StepType.Condition);
            return this;
        }

        public QuantumCircuit WithParameter(string name)
        {
            this.AddParameters(name);
            return this;
        }
    }
}
