using FluentAssertions;
using QuRest.Domain;
using System.Threading.Tasks;
using Xunit;

namespace QuRest.Application.UnitTests
{
    public class PlaceholderCompilerTests : CompilerTestBase
    {
        [Fact]
        public async Task Can_Compile_With_Placeholder()
        {
            var placeholder = new QuantumCircuit()
                .WithName("SimpleH")
                .WithSize("1")
                .H("0");

            var circuit = new QuantumCircuit()
                .WithName("SimpleCircuit")
                .WithSize("1")
                .Placeholder("placeholder");

            var compilation = await this.Compiler
                .AddPlaceholderMapping("placeholder", placeholder)
                .CompileAsync(circuit);

            compilation.Size.Should().Be("1");
            compilation.Steps.Should().HaveCount(1);
            compilation.Steps.Should().Contain(s => s.Index == 0 & s.Type == StepType.Unitarian);

            compilation.Unitarians.Should().HaveCount(1);
            compilation.Unitarians.Should().Contain(u => u.Index == 0 && u.Type == UnitarianType.H);
        }

        [Fact]
        public async Task Can_Compile_With_Placeholder_With_Parameters()
        {
            var placeholder = new QuantumCircuit()
                .WithName("SimpleH")
                .WithSize("1")
                .WithParameter("{p}")
                .H("{p}");

            var circuit = new QuantumCircuit()
                .WithName("SimpleCircuit")
                .WithSize("1")
                .WithParameter("{p}")
                .Placeholder("placeholder");

            var compilation = await this.Compiler
                .AddPlaceholderMapping("placeholder", placeholder)
                .AddParameterMapping("{p}", 0)
                .CompileAsync(circuit);

            compilation.Size.Should().Be("1");
            compilation.Steps.Should().HaveCount(1);
            compilation.Steps.Should().Contain(s => s.Index == 0 & s.Type == StepType.Unitarian);

            compilation.Unitarians.Should().HaveCount(1);
            compilation.Unitarians.Should().Contain(u => u.Index == 0 && u.Type == UnitarianType.H && u.Qubits == "0");
        }

        [Fact]
        public async Task Can_Compile_Loop_With_Placeholder()
        {
            var placeholder = new QuantumCircuit()
                .WithName("SimpleH")
                .WithSize("1")
                .H("0");

            var circuit = new QuantumCircuit()
                .WithName("SimpleCircuit")
                .WithSize("1")
                .For("{i}", "0", "4", "1")
                .Placeholder("placeholder")
                .EndFor();

            var compilation = await this.Compiler
                .AddPlaceholderMapping("placeholder", placeholder)
                .CompileAsync(circuit);

            compilation.Size.Should().Be("1");
            compilation.Steps.Should().HaveCount(4);
            compilation.Steps.Should().Contain(s => s.Index == 0 & s.Type == StepType.Unitarian);
            compilation.Steps.Should().Contain(s => s.Index == 1 & s.Type == StepType.Unitarian);
            compilation.Steps.Should().Contain(s => s.Index == 2 & s.Type == StepType.Unitarian);
            compilation.Steps.Should().Contain(s => s.Index == 3 & s.Type == StepType.Unitarian);

            compilation.Unitarians.Should().HaveCount(4);
            compilation.Unitarians.Should().Contain(u => u.Index == 0 && u.Type == UnitarianType.H && u.Qubits == "0");
            compilation.Unitarians.Should().Contain(u => u.Index == 1 && u.Type == UnitarianType.H && u.Qubits == "0");
            compilation.Unitarians.Should().Contain(u => u.Index == 2 && u.Type == UnitarianType.H && u.Qubits == "0");
            compilation.Unitarians.Should().Contain(u => u.Index == 3 && u.Type == UnitarianType.H && u.Qubits == "0");
        }

        [Fact]
        public async Task Can_Compile_Placeholder_With_Loop()
        {
            var placeholder = new QuantumCircuit()
                .WithName("SimpleH")
                .WithSize("1")
                .For("{i}", "0", "4", "1")
                    .H("0")
                .EndFor();

            var circuit = new QuantumCircuit()
                .WithName("SimpleCircuit")
                .WithSize("1")
                .Placeholder("placeholder");

            var compilation = await this.Compiler
                .AddPlaceholderMapping("placeholder", placeholder)
                .CompileAsync(circuit);

            compilation.Size.Should().Be("1");
            compilation.Steps.Should().HaveCount(4);
            compilation.Steps.Should().Contain(s => s.Index == 0 & s.Type == StepType.Unitarian);
            compilation.Steps.Should().Contain(s => s.Index == 1 & s.Type == StepType.Unitarian);
            compilation.Steps.Should().Contain(s => s.Index == 2 & s.Type == StepType.Unitarian);
            compilation.Steps.Should().Contain(s => s.Index == 3 & s.Type == StepType.Unitarian);

            compilation.Unitarians.Should().HaveCount(4);
            compilation.Unitarians.Should().Contain(u => u.Index == 0 && u.Type == UnitarianType.H && u.Qubits == "0");
            compilation.Unitarians.Should().Contain(u => u.Index == 1 && u.Type == UnitarianType.H && u.Qubits == "0");
            compilation.Unitarians.Should().Contain(u => u.Index == 2 && u.Type == UnitarianType.H && u.Qubits == "0");
            compilation.Unitarians.Should().Contain(u => u.Index == 3 && u.Type == UnitarianType.H && u.Qubits == "0");
        }

        [Fact]
        public async Task Can_Compile_Placeholder_With_Parameterized_If_ElseIf_Else_Condition()
        {
            var placeholder = new QuantumCircuit()
                .WithName("SimpleH")
                .WithSize("{N}")
                .If("{N} = 1")
                    .H("0")
                .ElseIf("{N} = 2")
                    .H("1")
                .Else()
                    .X("0")
                .EndIf();

            var circuit = new QuantumCircuit()
            .WithName("SimpleCircuit")
            .WithSize("{N}")
            .Placeholder("placeholder");

            var compilation = await this.Compiler
                .AddPlaceholderMapping("placeholder", placeholder)
                .AddParameterMapping("{N}", 1)
                .CompileAsync(circuit);

            compilation.Size.Should().Be("1");
            compilation.Steps.Should().HaveCount(1);
            compilation.Steps.Should().Contain(s => s.Index == 0 & s.Type == StepType.Unitarian);

            compilation.Unitarians.Should().HaveCount(1);
            compilation.Unitarians.Should().Contain(u => u.Index == 0 && u.Type == UnitarianType.H && u.Qubits == "0");

            var compilation2 = await this.Compiler
                .AddPlaceholderMapping("placeholder", placeholder)
                .AddParameterMapping("{N}", 2)
                .CompileAsync(circuit);

            compilation2.Size.Should().Be("2");
            compilation2.Steps.Should().HaveCount(1);
            compilation2.Steps.Should().Contain(s => s.Index == 0 & s.Type == StepType.Unitarian);

            compilation2.Unitarians.Should().HaveCount(1);
            compilation2.Unitarians.Should().Contain(u => u.Index == 0 && u.Type == UnitarianType.H && u.Qubits == "1");

            var compilation3 = await this.Compiler
                .AddPlaceholderMapping("placeholder", placeholder)
                .AddParameterMapping("{N}", 3)
                .CompileAsync(circuit);

            compilation3.Size.Should().Be("3");
            compilation3.Steps.Should().HaveCount(1);
            compilation3.Steps.Should().Contain(s => s.Index == 0 & s.Type == StepType.Unitarian);

            compilation3.Unitarians.Should().HaveCount(1);
            compilation3.Unitarians.Should().Contain(u => u.Index == 0 && u.Type == UnitarianType.X && u.Qubits == "0");
        }

        [Fact]
        public async Task Can_Compile_Parameterized_If_ElseIf_Else_Condition_With_Placeholder()
        {
            var placeholder1 = new QuantumCircuit()
                .WithName("SimpleH")
                .WithSize("1")
                .H("0");

            var placeholder2 = new QuantumCircuit()
                .WithName("SimpleX")
                .WithSize("1")
                .X("0");

            var placeholder3 = new QuantumCircuit()
                .WithName("SimpleZ")
                .WithSize("1")
                .Z("0");

            var circuit = new QuantumCircuit()
            .WithName("SimpleCircuit")
            .WithSize("{N}")
            .If("{N} = 1")
                .Placeholder("placeholder1")
            .ElseIf("{N} = 2")
                .Placeholder("placeholder2")
            .Else()
            .Placeholder("placeholder3")
            .EndIf();

            var compilation = await this.Compiler
                .AddPlaceholderMapping("placeholder1", placeholder1)
                .AddPlaceholderMapping("placeholder2", placeholder2)
                .AddPlaceholderMapping("placeholder3", placeholder3)
                .AddParameterMapping("{N}", 1)
                .CompileAsync(circuit);

            compilation.Size.Should().Be("1");
            compilation.Steps.Should().HaveCount(1);
            compilation.Steps.Should().Contain(s => s.Index == 0 & s.Type == StepType.Unitarian);

            compilation.Unitarians.Should().HaveCount(1);
            compilation.Unitarians.Should().Contain(u => u.Index == 0 && u.Type == UnitarianType.H && u.Qubits == "0");

            var compilation2 = await this.Compiler
                .AddPlaceholderMapping("placeholder1", placeholder1)
                .AddPlaceholderMapping("placeholder2", placeholder2)
                .AddPlaceholderMapping("placeholder3", placeholder3)
                .AddParameterMapping("{N}", 2)
                .CompileAsync(circuit);

            compilation2.Size.Should().Be("2");
            compilation2.Steps.Should().HaveCount(1);
            compilation2.Steps.Should().Contain(s => s.Index == 0 & s.Type == StepType.Unitarian);

            compilation2.Unitarians.Should().HaveCount(1);
            compilation2.Unitarians.Should().Contain(u => u.Index == 0 && u.Type == UnitarianType.X && u.Qubits == "0");

            var compilation3 = await this.Compiler
                .AddPlaceholderMapping("placeholder1", placeholder1)
                .AddPlaceholderMapping("placeholder2", placeholder2)
                .AddPlaceholderMapping("placeholder3", placeholder3)
                .AddParameterMapping("{N}", 3)
                .CompileAsync(circuit);

            compilation3.Size.Should().Be("3");
            compilation3.Steps.Should().HaveCount(1);
            compilation3.Steps.Should().Contain(s => s.Index == 0 & s.Type == StepType.Unitarian);

            compilation3.Unitarians.Should().HaveCount(1);
            compilation3.Unitarians.Should().Contain(u => u.Index == 0 && u.Type == UnitarianType.Z && u.Qubits == "0");
        }
    }
}
