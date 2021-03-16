using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using QuRest.Domain;
using System.Threading.Tasks;
using Xunit;

namespace QuRest.Library.UnitTests
{
    public class DependencyInjectionTests
    {
        [Fact]
        public async Task Can_Register_QuRest_Services()
        {
            var serviceProvider = new ServiceCollection()
                .AddQuRest()
                .BuildServiceProvider();

            var circuit = new QuantumCircuit()
                .WithSize("2")
                .RX("0", "1.2")
                .RX("1", "{theta}");

            var compiler = serviceProvider.GetRequiredService<IQuantumCircuitCompiler>()
                .AddParameterMapping("{N}", 4);

            var compilation = await compiler
                .AddParameterMapping("{theta}", 2.2)
                .CompileAsync(circuit);

            compilation.Steps.Should().HaveCount(2);
            compilation.Steps.Should().Contain(step =>
                step.Index == 0 &&
                step.Type == StepType.Unitarian);
            compilation.Steps.Should().Contain(step =>
                step.Index == 1 &&
                step.Type == StepType.Unitarian);

            compilation.Unitarians.Should().HaveCount(2);
            compilation.Unitarians.Should().Contain(unitarian =>
                unitarian.Index == 0 &&
                unitarian.Type == UnitarianType.RX &&
                unitarian.Qubits == "0" &&
                unitarian.Parameters == "1.2");
            compilation.Unitarians.Should().Contain(unitarian =>
                unitarian.Index == 1 &&
                unitarian.Type == UnitarianType.RX &&
                unitarian.Qubits == "1" &&
                unitarian.Parameters == "2.2");

            var translator = serviceProvider.GetRequiredService<IQuantumCircuitTranslator>();

            var qasm = await translator.TranslateToQasmAsync(compilation);
        }
    }
}
