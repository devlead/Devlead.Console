using Devlead.Console;
using Devlead.Console.Integration.Test.Models;
using Devlead.Console.Integration.Test.Services;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
public static partial class ServiceProviderFixture
{
    static partial void ConfigureInMemory(IDictionary<string, string?> configData)
    {
        configData.Add("TestService:Version", "1.0.0.0");
    }

    static partial void InitServiceProvider(IServiceCollection services)
        => services
                .AddTestService()
                .AddCommandApp(new TestConsole());
}