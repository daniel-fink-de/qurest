using Microsoft.Extensions.DependencyInjection;
using QuRest.Application.Abstractions;
using QuRest.Application.Interfaces;
using QuRest.Application.Services;

namespace QuRest
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddQuRest(this IServiceCollection services)
        {
            services.AddTransient<IQxmlCompiler, QxmlCompiler>();
            services.AddTransient<IQxmlTranslator, QxmlTranslator>();
            services.AddSingleton<IQuantumProgrammingStudioService, DummyQuantumProgrammingStudioService>();
            services.AddTransient<IQuantumCircuitCompiler, QuantumCircuitCompiler>();
            services.AddTransient<IQuantumCircuitTranslator, QuantumCircuitTranslator>();
            return services;
        }
    }
}
