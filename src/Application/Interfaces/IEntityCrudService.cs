using QuRest.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuRest.Application.Interfaces
{
    public interface IEntityCrudService<TEntity>
        where TEntity : IEntity
    {
        Task CreateAsync(TEntity entity);

        Task UpdateAsync(TEntity entity);

        Task<TEntity> ReadAsync(string name);

        Task<IEnumerable<TEntity>> ReadAllAsync();

        Task DeleteAsync(string name);
    }
}