using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuRest.Application.Interfaces;
using QuRest.Domain;
using QuRest.Infrastructure.Services;
using System.IO;

#pragma warning disable 618

namespace QuRest.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddSingleton<IEntityCrudService<QuantumCircuit>, MemoryCrudService<QuantumCircuit>>();
            }
            else
            {
                var persistenceDirectory = new DirectoryInfo(Directory.GetCurrentDirectory()).CreateSubdirectory("quantum-circuits");
                services.AddSingleton<IEntityCrudService<QuantumCircuit>>(_ => new FileCrudService<QuantumCircuit>(persistenceDirectory));
            }

            services.AddSingleton<IApplicationDbContext, ApplicationDbContext>();

            services.AddSingleton<IQuantumProgrammingStudioService, QuantumProgrammingStudioService>();

            var nodeJsDirectory = configuration.GetValue<string>("NodeJsDirectory");

            if (!string.IsNullOrEmpty(nodeJsDirectory))
            {
                // Add node services to execute quantum programming studio javascript
                services.AddNodeServices(action =>
                {
                    action.EnvironmentVariables.Add("PATH", nodeJsDirectory);
                });
                services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            }

            return services;
        }
    }
}
