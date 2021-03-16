using QuRest.Application.Abstractions;
using QuRest.Domain;
using System.Threading.Tasks;

namespace QuRest
{
    public interface IQuantumCircuitTranslator
    {
        public Task<string> TranslateToQasmAsync(QuantumCircuit quantumCircuit);
    }

    public class QuantumCircuitTranslator : IQuantumCircuitTranslator
    {
        private readonly IQxmlTranslator qxmlTranslator;

        public QuantumCircuitTranslator(IQxmlTranslator qxmlTranslator)
        {
            this.qxmlTranslator = qxmlTranslator;
        }

        public async Task<string> TranslateToQasmAsync(QuantumCircuit quantumCircuit)
        {
            return await this.qxmlTranslator.TranslateToQasmAsync(quantumCircuit);
        }
    }
}
