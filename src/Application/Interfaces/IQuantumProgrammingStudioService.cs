using System.Threading.Tasks;

namespace QuRest.Application.Interfaces
{
    public interface IQuantumProgrammingStudioService
    {
        public Task<string> QasmToSvg(string qasm);

        public Task<string> QasmToPyquil(string qasm);

        public Task<string> QasmToQiskit(string qasm);
    }
}