using QuRest.Domain;
using System.Threading.Tasks;

namespace QuRest.Application.Abstractions
{
    public interface IQxmlTranslator
    {
        public Task<string> TranslateToQasmAsync(QuantumCircuit algorithm);
        public Task<string> TranslateToQiskitAsync(QuantumCircuit algorithm);
        public Task<string> TranslateToPyQuilAsync(QuantumCircuit algorithm);
    }
}