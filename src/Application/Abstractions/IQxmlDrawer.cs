using QuRest.Domain;
using System.Threading.Tasks;

namespace QuRest.Application.Abstractions
{
    public interface IQxmlDrawer
    {
        public Task<string> DrawAsync(string qasm);

        public Task<string> DrawAsync(QuantumCircuit quantumCircuit);
    }
}