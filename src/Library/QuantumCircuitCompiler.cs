using QuRest.Application.Abstractions;
using QuRest.Application.Services;

namespace QuRest
{
    public interface IQuantumCircuitCompiler : IQxmlCompiler
    {
    }

    public class QuantumCircuitCompiler : QxmlCompiler, IQuantumCircuitCompiler
    {
    }
}
