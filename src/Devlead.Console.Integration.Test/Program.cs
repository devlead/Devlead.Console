
using Devlead.Console.Integration.Test.Models;
using Devlead.Console.Integration.Test.Services;

public partial class Program
{
#if DEBUG
    static partial void ConfigureInMemory(IDictionary<string, string?> configData)
    {

        configData.Add("TestService__Version", "1.0.0.0");
    }
#endif

    static partial void AddServices(IServiceCollection services)
    {
        services
            .AddOptions<TestServiceSettings>()
            .BindConfiguration(
                nameof(TestService)
            );

        services.AddSingleton<TestService>();
    }

    static partial void ConfigureApp((IServiceCollection services, IConfigurator configuration) serviceConfig)
    {
        serviceConfig
            .AddCommand<TestCommand>("test")
                .WithDescription("Example test command.")
                .WithExample(["test"]);

        serviceConfig
            .AddBranch(
                "yolo",
                c => c.AddCommand<TestCommand>("test")
                        .WithDescription("Example test command.")
                        .WithExample(["yolo", "test"])
            );
    }
}