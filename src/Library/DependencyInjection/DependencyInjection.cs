using Microsoft.Extensions.DependencyInjection;
using QuRest.Application;

namespace QuRest.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddQuRest(this IServiceCollection services)
        {
            return services.AddApplication();
        }
    }
}
