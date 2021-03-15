using QuRest.Application.Abstractions;
using QuRest.Application.Interfaces;
using QuRest.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuRest.Application.Services
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class QxmlTranslator : IQxmlTranslator
    {
        private readonly IQuantumProgrammingStudioService quantumProgrammingStudioService;

        public QxmlTranslator(IQuantumProgrammingStudioService quantumProgrammingStudioService)
        {
            this.quantumProgrammingStudioService = quantumProgrammingStudioService;
        }

        public async Task<string> TranslateToQasmAsync(QuantumCircuit quantumCircuit)
        {
            return await Task.Run(() =>
             {
                 var qasm = new StringBuilder();

                 qasm.Append(this.GenerateQasmHeader(quantumCircuit));

                 // add gates and measurements
                 foreach (var step in quantumCircuit.Steps)
                 {
                     switch (step.Type)
                     {
                         case StepType.Unitarian:
                             var unitarian = this.GetUnitarianFromCircuit(step.Index, quantumCircuit);
                             qasm.Append(GenerateQasmUnitarian(unitarian));
                             break;
                         case StepType.Hermitian:
                             var hermitian = this.GetHermitianFromCircuit(step.Index, quantumCircuit);
                             qasm.Append(GenerateQasmHermitian(hermitian));
                             break;
                         case StepType.Placeholder:
                             throw new InvalidOperationException();
                         case StepType.Loop:
                             throw new InvalidOperationException();
                         case StepType.Condition:
                             throw new InvalidOperationException();
                         default:
                             throw new InvalidOperationException();
                     }
                 }

                 return qasm.ToString();
             });
        }

        public async Task<string> TranslateToQiskitAsync(QuantumCircuit quantumCircuit)
        {
            return await this.quantumProgrammingStudioService.QasmToQiskit(await this.TranslateToQasmAsync(quantumCircuit));
        }

        public async Task<string> TranslateToPyQuilAsync(QuantumCircuit quantumCircuit)
        {
            return await this.quantumProgrammingStudioService.QasmToPyquil(await this.TranslateToQasmAsync(quantumCircuit));
        }

        private string GenerateQasmHeader(QuantumCircuit circuit)
        {
            var qasm = new StringBuilder();

            if (!int.TryParse(circuit.Size, out int size))
                throw new InvalidOperationException("No size specified!");

            qasm.Append(this.GenerateQasmCommentHeader(circuit));

            // add header information
            qasm.Append("OPENQASM 2.0;");
            qasm.Append(Environment.NewLine);
            qasm.Append("include \"qelib1.inc\";");
            qasm.Append(Environment.NewLine);

            // add registers
            qasm.Append($"qreg q[{size}];");
            qasm.Append(Environment.NewLine);
            qasm.Append($"creg c[{size}];");
            qasm.Append(Environment.NewLine);
            qasm.Append(Environment.NewLine);

            return qasm.ToString();
        }

        private string GenerateQasmCommentHeader(QuantumCircuit circuit)
        {
            var qasm = new StringBuilder();

            var nameComment = new StringBuilder("// [NAME]" + Environment.NewLine);
            var descriptionComment = new StringBuilder("// [DESCRIPTION]" + Environment.NewLine);

            if (!string.IsNullOrEmpty(circuit.Name))
            {
                nameComment.Append("// ").Append(circuit.Name).Append(Environment.NewLine);
            }
            else
            {
                nameComment.Append("//").Append(Environment.NewLine);
            }

            if (!string.IsNullOrEmpty(circuit.Description))
            {
                foreach (var line in circuit.Description.Split(Environment.NewLine))
                {
                    descriptionComment.Append("// ").Append(line).Append(Environment.NewLine);
                }
            }
            else
            {
                descriptionComment.Append("//").Append(Environment.NewLine);
            }

            qasm.Append(nameComment);
            qasm.Append("//").Append(Environment.NewLine);
            qasm.Append(descriptionComment);
            qasm.Append(Environment.NewLine);

            return qasm.ToString();
        }

        private Unitarian GetUnitarianFromCircuit(int index, QuantumCircuit circuit)
        {
            var unitarian = circuit.Unitarians.FirstOrDefault(u => u.Index == index);
            if (unitarian is null) throw new InvalidOperationException($"The unitarian with the index \"{index}\" could not be found.");
            return unitarian;
        }

        private Hermitian GetHermitianFromCircuit(int index, QuantumCircuit circuit)
        {
            var hermitian = circuit.Hermitians.FirstOrDefault(h => h.Index == index);
            if (hermitian is null) throw new InvalidOperationException($"The hermitian with the index \"{index}\" could not be found.");
            return hermitian;
        }

        private static string GenerateQasmUnitarian(Unitarian unitarian)
        {
            var qubits = unitarian.Qubits.Split(',').Select(s => Convert.ToInt32(s)).ToList();
            var parameters = new List<double>();
            if (!string.IsNullOrEmpty(unitarian.Parameters))
                parameters = unitarian.Parameters.Split(',').Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToList();

            return unitarian.Type switch
            {
                UnitarianType.H => $"h q[{qubits[0]}];\n",
                UnitarianType.X => $"x q[{qubits[0]}];\n",
                UnitarianType.Z => $"z q[{qubits[0]}];\n",
                UnitarianType.CX => $"cx q[{qubits[0]}], q[{qubits[1]}];\n",
                UnitarianType.CZ => $"cz q[{qubits[0]}], q[{qubits[1]}];\n",
                UnitarianType.RX => $"rx({parameters[0].ToString(new CultureInfo("en-US"))}) q[{qubits[0]}];\n",
                _ => throw new InvalidOperationException($"The unitarian with type \"{unitarian.Type}\" is invalid or takes more qubits than provided."),
            };
        }

        private static string GenerateQasmHermitian(Hermitian hermitian)
        {
            var qubits = hermitian.Qubits.Split(',').Select(s => Convert.ToInt32(s)).ToList();

            return hermitian.Type switch
            {
                HermitianType.X => $"measure q[{qubits[0]}] -> c[{qubits[0]}];\n",
                HermitianType.SET0 => $"reset q[{qubits[0]}];\n",
                HermitianType.SET1 => $"reset q[{qubits[0]}];\nx q[{qubits[0]}];",
                _ => throw new InvalidOperationException($"The hermitian with type \"{hermitian.Type}\" is invalid or takes more qubits than provided."),
            };
        }
    }
}
