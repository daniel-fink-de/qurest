using Microsoft.Extensions.DependencyInjection;
using System;

namespace QuRest.Client
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddQuRestClient(this IServiceCollection services, Uri quRestUri)
        {
            services.AddHttpClient<QuRestClient>(httpClient => httpClient.BaseAddress = quRestUri);
            return services;
        }

        public static IServiceCollection AddOpenApiClient(this IServiceCollection services, Uri quRestUri)
        {
            services.AddHttpClient<IQuRestClient, QuRestClient>(httpClient => httpClient.BaseAddress = quRestUri);
            return services;
        }
    }
}
