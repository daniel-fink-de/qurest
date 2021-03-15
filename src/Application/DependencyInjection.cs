using Microsoft.Extensions.DependencyInjection;
using QuRest.Application.Abstractions;
using QuRest.Application.Services;

namespace QuRest.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IQxmlCompiler, QxmlCompiler>();
            services.AddScoped<IQxmlDrawer, QxmlDrawer>();
            services.AddScoped<IQxmlTranslator, QxmlTranslator>();

            return services;
        }
    }
}
