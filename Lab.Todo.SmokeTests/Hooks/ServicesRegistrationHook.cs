using Lab.Todo.SmokeTests.Helpers;
using Lab.Todo.SmokeTests.Options;
using Lab.Todo.SmokeTests.Services;
using Lab.Todo.SmokeTests.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolidToken.SpecFlow.DependencyInjection;
using TechTalk.SpecFlow;

namespace Lab.Todo.SmokeTests.Hooks
{
    [Binding]
    public class ServicesRegistrationHook
    {
        private static IConfiguration? _configuration;

        private static IConfiguration Configuration => _configuration ??= new ConfigurationBuilder()
            .AddJsonFile("specflow.json", optional: false, reloadOnChange: true)
            .Build();

        [ScenarioDependencies]
        public static IServiceCollection ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.Configure<DatabaseOptions>(Configuration.GetSection("DatabaseOptions"))
                .Configure<UserOptions>(options => options.Username = Configuration["Username"])
                .Configure<HttpOptions>(Configuration.GetSection("HttpOptions"));

            serviceCollection.AddRefitClients();

            serviceCollection.AddTransient<IToDoItemsDbAccessor, ToDoItemsDbAccessor>();
            serviceCollection.AddTransient<IUserContext, UserContext>();

            serviceCollection.AddSingleton<IAuthorizationManager, AuthorizationManager>();
            serviceCollection.AddSingleton<ITokenHolder, TokenHolder>();

            return serviceCollection;
        }
    }
}