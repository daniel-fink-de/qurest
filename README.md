# QuRest: A RESTful Approach for Hybrid Quantum-Classical Circuit Modeling
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![BuildAndTest](https://github.com/StuttgarterDotNet/qurest/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/StuttgarterDotNet/qurest/actions/workflows/dotnet.yml)
![Docker Cloud Automated build](https://img.shields.io/docker/cloud/automated/stuttgarterdotnet/qurest)
![Nuget](https://img.shields.io/nuget/v/qurest)

QuRest is an academical prototype with the intention to show how **classical control structures** like loops and conditionals can be implemented in a **gate-based quantum circuit model**. 
This can be achieved by introducing so-called **design variables**, i.e. variables that needs to be mapped to real values at compile-time but can be used freely when modelling a quantum circuit.

The full paper, which was part of a course assignment, can be found here [PDF](https://github.com/StuttgarterDotNet/qurest/blob/14acb09bd0a197b8c5a97a4835fb4ca49a836f1d/QuRest_Paper.pdf).


The following example shows the preparation of a *GHZ-State* with *n* qubits using a for loop:

```csharp
var qc = new QuantumCircuit()
    .WithName("{N}-Qubit-GHZ")
    .WithDescription("Prepares a {N} qubit GHZ state.")
    .WithSize("{N}")
    .H("0")
    .For("{i}", "0", "{N}-1", "1")
        .CX("{i}", "{i}+1")
    .EndFor();
```

The output of the compilation with *N=4*:

<div style="text-align: center">
<img src="https://raw.githubusercontent.com/StuttgarterDotNet/qurest/563592eb1099dba354118040003ed2db8819874b/images/4-Qubit-GHZ.svg" align="center">
</div>

## Getting Started

### Docker
The easiest way of using the prototype is via the [official docker image](https://hub.docker.com/r/stuttgarterdotnet/qurest).
If docker is installed, executing

```ps
docker run -d -p 88:80 stuttgarterdotnet/qurest
```
will run a container with the QuRest image listening on port 88.
The swagger ui can than be accessed via [http://localhost:88/swagger](http://localhost:88/swagger).

### NuGet
Beside the RestAPI, the underlying quantum compiler and quantum translator can also be used in dotnet using the [QuRest NuGet package](https://www.nuget.org/packages/QuRest/).
After installing the latest package, you can run the following code to compile and translate a parameterized GHZ state to qasm:

```csharp
using Microsoft.Extensions.DependencyInjection;
using QuRest;
using QuRest.Domain;
using System;
using System.Threading.Tasks;

namespace Test
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddQuRest()
                .BuildServiceProvider();

            var circuit = new QuantumCircuit()
                .WithName("N-Qubit-GHZ")
                .WithDescription("Prepares a N qubit GHZ state.")
                .WithSize("{N}")
                .H("0")
                .For("{i}", "0", "{N}-1", "1")
                    .CX("{i}", "{i}+1")
                .EndFor();

            var compiler = serviceProvider.GetRequiredService<IQuantumCircuitCompiler>()
                .AddParameterMapping("{N}", 4);

            var compilation = await compiler.CompileAsync(circuit);

            var translator = serviceProvider.GetRequiredService<IQuantumCircuitTranslator>();

            var qasm = await translator.TranslateToQasmAsync(compilation);

            Console.WriteLine(qasm);
        }
    }
}
```

You should see the following output:

```q
// [NAME]
// N-Qubit-GHZ_compilation
//
// [DESCRIPTION]
// Prepares a N qubit GHZ state.

OPENQASM 2.0;
include "qelib1.inc";
qreg q[4];
creg c[4];

h q[0];
cx q[0], q[1];
cx q[1], q[2];
cx q[2], q[3];
```

## Disclaimer of Warranty
Unless required by applicable law or agreed to in writing, Licensor provides the Work (and each Contributor provides its Contributions) on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied, including, without limitation, any warranties or conditions of TITLE, NON-INFRINGEMENT, MERCHANTABILITY, or FITNESS FOR A PARTICULAR PURPOSE.
You are solely responsible for determining the appropriateness of using or redistributing the Work and assume any risks associated with Your exercise of permissions under this License.

## Haftungsausschluss
Dies ist ein Forschungsprototyp.
Die Haftung für entgangenen Gewinn, Produktionsausfall, Betriebsunterbrechung, entgangene Nutzungen, Verlust von Daten und Informationen, Finanzierungsaufwendungen sowie sonstige Vermögens- und Folgeschäden ist, außer in Fällen von grober Fahrlässigkeit, Vorsatz und Personenschäden, ausgeschlossen.

## Third Party
QuRest uses parts of the [Quantum Programming Studio](https://github.com/quantastica/quantum-circuit) and [NCalc2](https://github.com/sklose/NCalc2) which are licensed under MIT:

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
