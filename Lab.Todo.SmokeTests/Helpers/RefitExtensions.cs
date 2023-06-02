using Lab.Todo.SmokeTests.Options;
using Lab.Todo.SmokeTests.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using Lab.Todo.SmokeTests.Services.DataAccess;

namespace Lab.Todo.SmokeTests.Helpers
{
    public static class RefitExtensions
    {
        public static IServiceCollection AddRefitClients(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddRefitClient<IAccountService>()
                .ConfigureHttpClient(GetHttpClientDefaultConfiguration);

            serviceCollection
                .AddRefitClient<IToDoItemsService>(CreateRefitSettingsWithAuth)
                .ConfigureHttpClient(GetHttpClientDefaultConfiguration);

            return serviceCollection;
        }

        private static RefitSettings CreateRefitSettingsWithAuth(IServiceProvider provider) => new()
        {
            AuthorizationHeaderValueGetter = () =>
                provider.GetRequiredService<IAuthorizationManager>().GetAuthorizationToken()
        };

        private static void GetHttpClientDefaultConfiguration(IServiceProvider provider, HttpClient client)
        {
            var options = provider.GetRequiredService<IOptions<HttpOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseAddress);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
        }
    }
}
