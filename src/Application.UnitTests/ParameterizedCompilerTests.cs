using FluentAssertions;
using QuRest.Domain;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace QuRest.Application.UnitTests
{
    public class ParameterizedCompilerTests : CompilerTestBase
    {
        [Fact]
        public async Task Can_Compile_Parameterized_Gates()
        {
            var circuit = new QuantumCircuit()
                .WithSize("2")
                .RX("0", "1.2")
                .RX("1", "{theta}");
            var compilation = await this.Compiler
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
        }

        [Fact]
        public async Task Can_Compile_With_Size_Parameter()
        {
            var circuit = new QuantumCircuit()
                .WithSize("{N}")
                .H("0")
                .CX("0", "2");
            var compilation = await this.Compiler
                .AddParameterMapping("{N}", 3)
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
                unitarian.Type == UnitarianType.H &&
                unitarian.Qubits == "0" &&
                string.IsNullOrEmpty(unitarian.Parameters));
            compilation.Unitarians.Should().Contain(unitarian =>
                unitarian.Index == 1 &&
                unitarian.Type == UnitarianType.CX &&
                unitarian.Qubits == "0,2" &&
                string.IsNullOrEmpty(unitarian.Parameters));

            compilation.Size.Should().Be("3");
        }

        [Fact]
        public async Task Can_Compile_With_Using_Size_Parameter_In_Quantum_Circuit()
        {
            var circuit = new QuantumCircuit()
                .WithSize("{N}")
                .H("{N}-1")
                .CX("0", "2");
            var compilation = await this.Compiler
                .AddParameterMapping("{N}", 3)
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
                unitarian.Type == UnitarianType.H &&
                unitarian.Qubits == "2" &&
                string.IsNullOrEmpty(unitarian.Parameters));
            compilation.Unitarians.Should().Contain(unitarian =>
                unitarian.Index == 1 &&
                unitarian.Type == UnitarianType.CX &&
                unitarian.Qubits == "0,2" &&
                string.IsNullOrEmpty(unitarian.Parameters));

            compilation.Size.Should().Be("3");
        }

        [Fact]
        public async Task Can_Compile_With_Using_Parameters_In_Quantum_Circuit()
        {
            var circuit = new QuantumCircuit()
                .WithSize("{N}")
                .WithParameter("{p}")
                .H("{N}-1")
                .CX("{p}", "{p}-1");
            var compilation = await this.Compiler
                .AddParameterMapping("{N}", 3)
                .AddParameterMapping("{p}", 1)
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
                unitarian.Type == UnitarianType.H &&
                unitarian.Qubits == "2" &&
                string.IsNullOrEmpty(unitarian.Parameters));
            compilation.Unitarians.Should().Contain(unitarian =>
                unitarian.Index == 1 &&
                unitarian.Type == UnitarianType.CX &&
                unitarian.Qubits == "1,0" &&
                string.IsNullOrEmpty(unitarian.Parameters));

            compilation.Size.Should().Be("3");
        }

        [Fact]
        public async Task Can_Compile_With_Parameterized_For_Loop()
        {
            var circuit = new QuantumCircuit()
                .WithSize("{N}")
                .For("{i}", "0", "{N}", "1")
                  .H("{i}")
                .EndFor();
            var compilation = await this.Compiler
                .AddParameterMapping("{N}", 4)
                .CompileAsync(circuit);

            compilation.Steps.Should().HaveCount(4);

            compilation.Unitarians.Should().HaveCount(4);
            compilation.Unitarians.Where(u => u.Type != UnitarianType.H).Should().BeNullOrEmpty();
            compilation.Unitarians.Should().Contain(u => u.Index == 0 && u.Qubits == "0");
            compilation.Unitarians.Should().Contain(u => u.Index == 1 && u.Qubits == "1");
            compilation.Unitarians.Should().Contain(u => u.Index == 2 && u.Qubits == "2");
            compilation.Unitarians.Should().Contain(u => u.Index == 3 && u.Qubits == "3");
        }

        [Fact]
        public async Task Can_Compile_With_Nested_Parameterized_For_Loop()
        {
            var circuit = new QuantumCircuit()
                .WithSize("{N}")
                .For("{i}", "0", "{N}", "1")
                    .For("{j}", "0", "{N}", "1")
                        .RX("0", "{theta_{i}_{j}}")
                   .EndFor()
                .EndFor();
            var compilation = await this.Compiler
                .AddParameterMapping("{N}", 2)
                .AddParameterMapping("{theta_0_0}", 1.1)
                .AddParameterMapping("{theta_0_1}", 1.2)
                .AddParameterMapping("{theta_1_0}", 2.1)
                .AddParameterMapping("{theta_1_1}", 2.2)
                .CompileAsync(circuit);

            compilation.Steps.Should().HaveCount(4);

            compilation.Unitarians.Should().HaveCount(4);
            compilation.Unitarians.Where(u => u.Type != UnitarianType.RX).Should().BeNullOrEmpty();
            compilation.Unitarians.Should().Contain(u => u.Index == 0 && u.Qubits == "0" && u.Parameters == "1.1");
            compilation.Unitarians.Should().Contain(u => u.Index == 1 && u.Qubits == "0" && u.Parameters == "1.2");
            compilation.Unitarians.Should().Contain(u => u.Index == 2 && u.Qubits == "0" && u.Parameters == "2.1");
            compilation.Unitarians.Should().Contain(u => u.Index == 3 && u.Qubits == "0" && u.Parameters == "2.2");
        }
    }
}
