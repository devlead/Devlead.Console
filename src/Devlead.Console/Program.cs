using Microsoft.Extensions.Configuration.Memory;

var configurationBuilder = new ConfigurationBuilder();

Configure(configurationBuilder);

var configuration = configurationBuilder
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();
services
    .AddSingleton((IConfiguration)configuration)
    .AddLogging(configure =>
        configure
            .AddSimpleConsole(opts =>
            {
                opts.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
            })
                .AddConfiguration(
                new ConfigurationBuilder()
                    .Add(new MemoryConfigurationSource
                    {
                        InitialData = GetIntitialInMemoryConfigurationData()
                    })
                .Build()
        ));

AddServices(services);

using var registrar = new DependencyInjectionRegistrar(services);
var app = new CommandApp(registrar);

app.Configure(
    config =>
    {
        config.UseAssemblyInformationalVersion();
        config.ValidateExamples();
        config.SetExceptionHandler(
            (ex, _) => AnsiConsole.WriteException(ex, ExceptionFormats.ShowLinks)
            );

        ConfigureApp((services, config));
    });

return await app.RunAsync(args);

static Dictionary<string, string?> GetIntitialInMemoryConfigurationData()
{
    var configData = new Dictionary<string, string?>
                        {
                            { "LogLevel:System.Net.Http.HttpClient", "Warning" }
                        };

    ConfigureInMemory(configData);

    return configData;
}

public partial class Program
{
    static partial void AddServices(IServiceCollection services);

    static partial void Configure(IConfigurationBuilder configuration);

    static partial void ConfigureApp((IServiceCollection services, IConfigurator configuration) serviceConfig);

    static partial void ConfigureInMemory(IDictionary<string, string?> configData);
}