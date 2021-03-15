using QuRest.Application.Interfaces;
using QuRest.Domain;

namespace QuRest.Infrastructure.Services
{
    public class ApplicationDbContext : IApplicationDbContext
    {
        public IEntityCrudService<QuantumCircuit> QuantumCircuits { get; }
        public IEntityCrudService<QuantumCircuit> Compilations { get; }

        public ApplicationDbContext(IEntityCrudService<QuantumCircuit> circuits, IEntityCrudService<QuantumCircuit> compilations)
        {
            this.QuantumCircuits = circuits;
            this.Compilations = compilations;
        }
    }
}