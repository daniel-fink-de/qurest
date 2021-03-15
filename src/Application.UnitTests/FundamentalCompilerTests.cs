using FluentAssertions;
using QuRest.Domain;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace QuRest.Application.UnitTests
{
    public class FundamentalCompilerTests : CompilerTestBase
    {
        [Fact]
        public async Task Can_Compile_Empty_Algorithm()
        {
            var algorithm = new QuantumCircuit();
            await this.Compiler.CompileAsync(algorithm);
        }

        [Fact]
        public async Task Can_Compile_Empty_Algorithm_With_Basic_Information()
        {
            var algorithm = new QuantumCircuit()
                .WithName("TestAlgorithm")
                .WithDescription("Just a test algorithm")
                .WithSize("10")
                .WithParameter("{i}")
                .WithParameter("{j}");
            var compilation = await this.Compiler.CompileAsync(algorithm);

            compilation.Name.Should().Contain("TestAlgorithm");
            compilation.Description.Should().Be("Just a test algorithm");
            compilation.Size.Should().Be("10");
            compilation.Parameters.Should().BeNullOrEmpty("because the compilation resolved all parameters");
        }

        [Fact]
        public async Task Can_Compile_Simple_Gates()
        {
            var algorithm = new QuantumCircuit()
                .WithSize("2")
                .H("0")
                .CX("0", "1");
            var compilation = await this.Compiler.CompileAsync(algorithm);

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
                unitarian.Qubits == "0,1" &&
                string.IsNullOrEmpty(unitarian.Parameters));
        }

        [Fact]
        public async Task Can_Compile_With_Static_For_Loop()
        {
            var algorithm = new QuantumCircuit()
                .WithSize("1")
                .For("{i}", "0", "4", "1")
                  .H("0")
                .EndFor();
            var compilation = await this.Compiler
                .CompileAsync(algorithm);

            compilation.Steps.Should().HaveCount(4);

            compilation.Unitarians.Should().HaveCount(4);
            compilation.Unitarians.Where(u => u.Type != UnitarianType.H).Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task Can_Compile_With_If_Condition()
        {
            var algorithm = new QuantumCircuit()
                .WithSize("{N}")
                .If("{N} = 2")
                    .H("0")
                    .H("1")
                .EndIf();
            var compilation = await this.Compiler
                .AddParameterMapping("{N}", 2)
                .CompileAsync(algorithm);

            compilation.Steps.Should().HaveCount(2);
            compilation.Unitarians.Should().HaveCount(2);
            compilation.Unitarians.Where(u => u.Type != UnitarianType.H).Should().BeNullOrEmpty();
            compilation.Unitarians.Should().Contain(u => u.Index == 0 && u.Qubits == "0");
            compilation.Unitarians.Should().Contain(u => u.Index == 1 && u.Qubits == "1");

            var compilation2 = await this.Compiler
                .AddParameterMapping("{N}", 1)
                .CompileAsync(algorithm);

            compilation2.Steps.Should().HaveCount(0);
            compilation2.Unitarians.Should().HaveCount(0);
        }

        [Fact]
        public async Task Can_Compile_With_If_Else_Condition()
        {
            var algorithm = new QuantumCircuit()
                .WithSize("{N}")
                .If("{N} = 2")
                    .H("0")
                    .H("1")
                .Else()
                    .H("0")
                .EndIf();
            var compilation = await this.Compiler
                .AddParameterMapping("{N}", 2)
                .CompileAsync(algorithm);

            compilation.Steps.Should().HaveCount(2);
            compilation.Unitarians.Should().HaveCount(2);
            compilation.Unitarians.Where(u => u.Type != UnitarianType.H).Should().BeNullOrEmpty();
            compilation.Unitarians.Should().Contain(u => u.Index == 0 && u.Qubits == "0");
            compilation.Unitarians.Should().Contain(u => u.Index == 1 && u.Qubits == "1");

            var compilation2 = await this.Compiler
                .AddParameterMapping("{N}", 1)
                .CompileAsync(algorithm);

            compilation2.Steps.Should().HaveCount(1);
            compilation2.Unitarians.Should().HaveCount(1);
            compilation.Unitarians.Should().Contain(u => u.Index == 0 && u.Qubits == "0");
        }

        [Fact]
        public async Task Can_Compile_With_If_ElseIf_Else_Condition()
        {
            var algorithm = new QuantumCircuit()
                .WithSize("{N}")
                .If("{N} = 2")
                    .H("0")
                    .H("1")
                .ElseIf("{N} = 1")
                    .H("1")
                .Else()
                    .H("0")
                .EndIf();
            var compilation = await this.Compiler
                .AddParameterMapping("{N}", 2)
                .CompileAsync(algorithm);

            compilation.Steps.Should().HaveCount(2);
            compilation.Unitarians.Should().HaveCount(2);
            compilation.Unitarians.Where(u => u.Type != UnitarianType.H).Should().BeNullOrEmpty();
            compilation.Unitarians.Should().Contain(u => u.Index == 0 && u.Qubits == "0");
            compilation.Unitarians.Should().Contain(u => u.Index == 1 && u.Qubits == "1");

            var compilation2 = await this.Compiler
                .AddParameterMapping("{N}", 1)
                .CompileAsync(algorithm);

            compilation2.Steps.Should().HaveCount(1);
            compilation2.Unitarians.Should().HaveCount(1);
            compilation2.Unitarians.Should().Contain(u => u.Index == 0 && u.Qubits == "1");

            var compilation3 = await this.Compiler
                .AddParameterMapping("{N}", 0)
                .CompileAsync(algorithm);

            compilation3.Steps.Should().HaveCount(1);
            compilation3.Unitarians.Should().HaveCount(1);
            compilation3.Unitarians.Should().Contain(u => u.Index == 0 && u.Qubits == "0");
        }
    }
}
