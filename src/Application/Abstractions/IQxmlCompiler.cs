using QuRest.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuRest.Application.Abstractions
{
    public interface IQxmlCompiler
    {
        public Task<QuantumCircuit> CompileAsync(QuantumCircuit algorithm);

        public IQxmlCompiler WithPlaceholderMapping(IDictionary<string, QuantumCircuit> mapping);

        public IQxmlCompiler WithParameterMapping(IDictionary<string, double> mapping);

        public IQxmlCompiler AddPlaceholderMapping(string placeholderName, QuantumCircuit algorithm);

        public IQxmlCompiler AddParameterMapping(string parameterName, double value);
    }
}