﻿using QuRest.Domain;

namespace QuRest.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        public IEntityCrudService<QuantumCircuit> QuantumCircuits { get; }
    }
}