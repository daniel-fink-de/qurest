using QuRest.Domain;
using System.Collections.Generic;
// ReSharper disable CheckNamespace

namespace QuRest
{
    public static class QuantumCircuitExamples
    {
        public static IEnumerable<QuantumCircuit> All =>
            new List<QuantumCircuit>
            {
                CoinFlip(),
                Ghz2(),
                DeutschCxOracle(),
                GroverCzOracle2(),
                EntangleAllWithFirstQubit(),
                GhzN(),
                SwapWithCnots()
            };

        public static QuantumCircuit CoinFlip()
        {
            return new QuantumCircuit()
                .WithName("CoinFlip")
                .WithDescription("A single coin flip simulation.")
                .WithSize("1")
                .H("0")
                .MX("0");
        }

        public static QuantumCircuit Ghz2()
        {
            return new QuantumCircuit()
                .WithName("Ghz2")
                .WithDescription("Prepares a 2 qubit GHZ state.")
                .WithSize("2")
                .H("0")
                .CX("0", "1");
        }

        public static QuantumCircuit DeutschCxOracle()
        {
            return new QuantumCircuit()
                .WithName("DeutschCxOracle")
                .WithDescription("Deutsch Algorithm with CX as oracle.")
                .WithSize("2")
                .H("0")
                .H("1")
                .CX("0", "1") // Oracle
                .H("0")
                .H("1")
                .MX("0");
        }

        public static QuantumCircuit GroverCzOracle2()
        {
            return new QuantumCircuit()
                .WithName("GroverCzOracle2")
                .WithDescription("Grover Algorithm with CZ as oracle on 2 qubits.")
                .WithSize("2")
                // initialization
                .H("0")
                .H("1")
                // oracle for |11>
                .CZ("0", "1")
                // one Grover iteration
                .H("0")
                .H("1")
                .Z("0")
                .Z("1")
                .CZ("0", "1")
                .H("0")
                .H("1")
                // measurements
                .MX("0")
                .MX("1");
        }

        public static QuantumCircuit EntangleAllWithFirstQubit()
        {
            return new QuantumCircuit()
                .WithName("EntangleAllWithFirstQubit")
                .WithDescription("Entangles all {N} qubits with the first qubit.")
                .WithSize("{N}")
                .For("{i}", "0", "{N}-1", "1")
                    .CX("0", "{i}+1")
                .EndFor();
        }

        public static QuantumCircuit GhzN()
        {
            return new QuantumCircuit()
                .WithName("GhzN")
                .WithDescription("Prepares a {N} qubit GHZ state.")
                .WithSize("{N}")
                .H("0")
                .For("{i}", "0", "{N}-1", "1")
                    .CX("{i}", "{i}+1")
                .EndFor();
        }

        public static QuantumCircuit SwapWithCnots()
        {
            return new QuantumCircuit()
                .WithName("SwapWithCnots")
                .WithDescription("Applies a SWAP operation but only uses CNOT gates.")
                .WithSize("2")
                .CX("0", "1")
                .CX("1", "0")
                .CX("0", "1");
        }
    }
}
