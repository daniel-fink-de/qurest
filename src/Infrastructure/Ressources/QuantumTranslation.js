var QuantumCircuit = require("./quantum-circuit");

module.exports = {
    qasm2svg: function (callback, qasmString) {
        var circuit = new QuantumCircuit();
        circuit.importQASM(qasmString, null);
        var result = circuit.exportSVG(false);
        callback(null, result);
    },
    
    qasm2pyquil: function (callback, qasmString) {
        var circuit = new QuantumCircuit();
        circuit.importQASM(qasmString, null);
        var result = circuit.exportPyquil("", false, null, "2.1", "", false);
        callback(null, result);
    },

    qasm2qiskit: function (callback, qasmString) {
        var circuit = new QuantumCircuit();
        circuit.importQASM(qasmString, null);
        var result = circuit.exportQiskit("", false, null, null);
        callback(null, result);
    }
};
