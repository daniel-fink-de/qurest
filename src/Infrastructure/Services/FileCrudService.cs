using Newtonsoft.Json;
using QuRest.Application.Interfaces;
using QuRest.Domain;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuRest.Infrastructure.Services
{
    public class FileCrudService<TEntity> : IEntityCrudService<TEntity>
        where TEntity : IEntity
    {
        private const string FileExtension = "json";
        private readonly DirectoryInfo persistenceDirectory;

        public FileCrudService(DirectoryInfo persistenceDirectory)
        {
            this.persistenceDirectory = persistenceDirectory;
        }

        private void CreateDirectoryIfNotExist()
        {
            if (!persistenceDirectory.Exists)
            {
                persistenceDirectory.Create();
            }
        }

        private FileInfo GetFileInfo(string name)
        {
            return new(Path.Combine(this.persistenceDirectory.FullName, $"{name}.{FileExtension}"));
        }

        public Task CreateAsync(TEntity entity)
        {
            this.CreateDirectoryIfNotExist();
            var file = this.GetFileInfo(entity.Name);
            if (file.Exists)
            {
                file.Delete();
            }

            File.WriteAllText(file.FullName, JsonConvert.SerializeObject(entity, Formatting.Indented));

            return Task.CompletedTask;
        }

        public Task DeleteAsync(string name)
        {
            this.CreateDirectoryIfNotExist();
            var file = this.GetFileInfo(name);
            if (file.Exists)
            {
                file.Delete();
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<TEntity>> ReadAllAsync()
        {
            this.CreateDirectoryIfNotExist();
            var fileInfos = persistenceDirectory.GetFiles($"*.{FileExtension}", SearchOption.TopDirectoryOnly);
            var entities = fileInfos.Select(fileInfo => JsonConvert.DeserializeObject<TEntity>(File.ReadAllText(fileInfo.FullName))).ToList();

            return Task.FromResult<IEnumerable<TEntity>>(entities);
        }

        public Task<TEntity> ReadAsync(string name)
        {
            this.CreateDirectoryIfNotExist();
            var fileInfo = this.GetFileInfo(name);
            var entity = JsonConvert.DeserializeObject<TEntity>(File.ReadAllText(fileInfo.FullName));

            return Task.FromResult(entity);
        }

        public Task UpdateAsync(TEntity entity)
        {
            return this.CreateAsync(entity);
        }
    }
}
