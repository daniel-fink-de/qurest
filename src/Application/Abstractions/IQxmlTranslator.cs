using QuRest.Domain;
using System.Threading.Tasks;

namespace QuRest.Application.Abstractions
{
    public interface IQxmlTranslator
    {
        public Task<string> TranslateToQasmAsync(QuantumCircuit quantumCircuit);
        public Task<string> TranslateToQiskitAsync(QuantumCircuit quantumCircuit);
        public Task<string> TranslateToPyQuilAsync(QuantumCircuit quantumCircuit);
    }
}