using Microsoft.AspNetCore.NodeServices;
using QuRest.Application.Interfaces;
using System.IO;
using System.Threading.Tasks;

#pragma warning disable 618

namespace QuRest.Infrastructure.Services
{
    /// <summary>
    /// This class wraps the access to the quantum programming studio javascript files.
    /// </summary>
    public class QuantumProgrammingStudioService : IQuantumProgrammingStudioService
    {
        private readonly INodeServices nodeServices;
        private const string JsFile = @"./QuantumTranslation.js";

        /// <summary>
        /// Instantiates a new quantum programming studio service,
        /// given the node service to execute javascript files.
        /// </summary>
        /// <param name="nodeServices"></param>
        public QuantumProgrammingStudioService(INodeServices nodeServices)
        {
            this.nodeServices = nodeServices;

            // extract the js resource files
            File.WriteAllText("QASMImport.js", Properties.Resources.QASMImport);
            File.WriteAllText("QASMLexer.js", Properties.Resources.QASMLexer);
            File.WriteAllText("QASMListener.js", Properties.Resources.QASMListener);
            File.WriteAllText("QASMParser.js", Properties.Resources.QASMParser);
            File.WriteAllText("quantum-circuit.js", Properties.Resources.quantum_circuit);
            File.WriteAllText("QuantumTranslation.js", Properties.Resources.QuantumTranslation);
        }

        /// <summary>
        /// Converts the given qasm text to the desired output format.
        /// </summary>
        /// <param name="qasm">Qasm text</param>
        /// <param name="exportFormat">Output format as string identifier</param>
        /// <returns>The string representing the output format.</returns>
        private async Task<string> QasmTo(string qasm, string exportFormat)
        {
            return await nodeServices.InvokeExportAsync<string>(JsFile, $"qasm2{exportFormat}", qasm);
        }

        /// <inheritdoc/>
        public async Task<string> QasmToSvg(string qasm)
        {
            return await QasmTo(qasm, "svg");
        }

        /// <inheritdoc/>
        public async Task<string> QasmToPyquil(string qasm)
        {
            return await QasmTo(qasm, "pyquil");
        }

        /// <inheritdoc/>
        public async Task<string> QasmToQiskit(string qasm)
        {
            return await QasmTo(qasm, "qiskit");
        }
    }
}
