
using Devlead.Console.Integration.Test.Extensions;

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
            .AddTestService();
    }

    static partial void ConfigureApp(AppServiceConfig appServiceConfig)
    {
        appServiceConfig
            .AddCommand<TestCommand>("test")
                .WithDescription("Example test command.")
                .WithExample(["test"]);

        appServiceConfig
            .AddBranch(
                "yolo",
                c => c.AddCommand<TestCommand>("test")
                        .WithDescription("Example test command.")
                        .WithExample(["yolo", "test"])
            );

        appServiceConfig.SetApplicationName("MyApp");
        
        appServiceConfig.SetDefaultCommand<TestCommand>();
    }
}