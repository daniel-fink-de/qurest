# QuRest: A REST-full Approach for Hybrid Quantum-Classical Circuit Modeling
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![BuildAndTest](https://github.com/StuttgarterDotNet/qurest/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/StuttgarterDotNet/qurest/actions/workflows/dotnet.yml)
![Docker Cloud Automated build](https://img.shields.io/docker/cloud/automated/stuttgarterdotnet/qurest)
![Nuget](https://img.shields.io/nuget/v/qurest)

QuRest is an academical prototype with the intention to show how **classical control structures** like loops and conditionals can be implemented in a **gate-based quantum circuit model**. 
This can be achieved by introducing so-called **model variables**, i.e. variables that needs to be mapped to real values at compile-time but can be used freely when modelling a quantum circuit.


The following example shows the preparation of a *GHZ-State* with *n* qubits using a for loop:

```csharp
var qc = new QuantumCircuit()
    .WithName("N-Qubit-GHZ")
    .WithDescription("Prepares a N qubit GHZ state.")
    .WithSize("N")
    .H("0")
    .For("i", "0", "N-1", "1")
        .CX("i", "i+1")
    .EndFor();
```

The output of the compilation with *N=4*:


<div style="text-align: center">
<img src="https://raw.githubusercontent.com/StuttgarterDotNet/qurest/563592eb1099dba354118040003ed2db8819874b/images/4-Qubit-GHZ.svg" align="center">
</div>

## Getting Started
The easiest way of using the prototype is via the [official docker image](https://hub.docker.com/r/stuttgarterdotnet/qurest).
If docker is installed, executing

```ps
docker run -d -p 88:80 stuttgarterdotnet/qurest
```
will run a container with the QuRest image listening on port 88.
The swagger ui can than be accessed via [http://localhost:88/swagger](http://localhost:88/swagger).


## Disclaimer of Warranty
Unless required by applicable law or agreed to in writing, Licensor provides the Work (and each Contributor provides its Contributions) on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied, including, without limitation, any warranties or conditions of TITLE, NON-INFRINGEMENT, MERCHANTABILITY, or FITNESS FOR A PARTICULAR PURPOSE.
You are solely responsible for determining the appropriateness of using or redistributing the Work and assume any risks associated with Your exercise of permissions under this License.

## Haftungsausschluss
Dies ist ein Forschungsprototyp.
Die Haftung für entgangenen Gewinn, Produktionsausfall, Betriebsunterbrechung, entgangene Nutzungen, Verlust von Daten und Informationen, Finanzierungsaufwendungen sowie sonstige Vermögens- und Folgeschäden ist, außer in Fällen von grober Fahrlässigkeit, Vorsatz und Personenschäden, ausgeschlossen.

## Quantum Programming Studio
QuRest uses parts of the [Quantum Programming Studio](https://github.com/quantastica/quantum-circuit) which is licensed under MIT:

Copyright (c) 2016 Quantastica

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
