using QuRest.Application.Interfaces;
using QuRest.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuRest.Infrastructure.Services
{
    public class MemoryCrudService<TEntity> : IEntityCrudService<TEntity>
        where TEntity : IEntity
    {
        private readonly IDictionary<string, TEntity> dictionary = new Dictionary<string, TEntity>();

        public Task CreateAsync(TEntity entity)
        {
            this.dictionary[entity.Name] = entity;

            return Task.CompletedTask;
        }

        public Task DeleteAsync(string name)
        {
            this.dictionary.Remove(name);

            return Task.CompletedTask;
        }

        public Task<IEnumerable<TEntity>> ReadAllAsync()
        {
            return Task.FromResult<IEnumerable<TEntity>>(this.dictionary.Values);
        }

        public Task<TEntity> ReadAsync(string name)
        {
            return Task.FromResult(this.dictionary[name]);
        }

        public Task UpdateAsync(TEntity entity)
        {
            return this.CreateAsync(entity);
        }
    }
}