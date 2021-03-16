using QuRest.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace QuRest.Application.Services
{
    internal class DummyQuantumProgrammingStudioService : IQuantumProgrammingStudioService
    {
        public Task<string> QasmToSvg(string qasm)
        {
            throw new NotImplementedException();
        }

        public Task<string> QasmToPyquil(string qasm)
        {
            throw new NotImplementedException();
        }

        public Task<string> QasmToQiskit(string qasm)
        {
            throw new NotImplementedException();
        }
    }
}
