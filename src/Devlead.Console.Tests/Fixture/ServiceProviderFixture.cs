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