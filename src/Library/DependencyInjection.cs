using Microsoft.Extensions.DependencyInjection;
using QuRest.Application;

namespace QuRest
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddQuRest(this IServiceCollection services)
        {
            return services.AddApplication();
        }
    }
}
