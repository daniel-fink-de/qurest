using QuRest.Application.Interfaces;
using QuRest.Domain;

namespace QuRest.Infrastructure.Services
{
    public class ApplicationDbContext : IApplicationDbContext
    {
        public IEntityCrudService<QuantumCircuit> Algorithms { get; }
        public IEntityCrudService<QuantumCircuit> Compilations { get; }

        public ApplicationDbContext(IEntityCrudService<QuantumCircuit> algorithms, IEntityCrudService<QuantumCircuit> compilations)
        {
            this.Algorithms = algorithms;
            this.Compilations = compilations;
        }
    }
}