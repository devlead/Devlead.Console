namespace Devlead.Console.Integration.Test.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTestService(this IServiceCollection services)
        {
            services
               .AddOptions<TestServiceSettings>()
               .BindConfiguration(
                   nameof(TestService)
               );

            services.AddSingleton<TestService>();

            return services;
        }
    }
}
