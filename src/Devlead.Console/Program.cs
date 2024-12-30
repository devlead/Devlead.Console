#if UseDefaultProgram
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
                        InitialData = GetInitialInMemoryConfigurationData()
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

static Dictionary<string, string?> GetInitialInMemoryConfigurationData()
{
    var configData = new Dictionary<string, string?>
                        {
                            { "LogLevel:System.Net.Http.HttpClient", "Warning" }
                        };

    ConfigureInMemory(configData);

    return configData;
}
/// <summary>
/// The Program
/// </summary>
public partial class Program
{
    /// <summary>
    /// Adds additional services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    static partial void AddServices(IServiceCollection services);

    /// <summary>
    /// Configures the application configuration builder.
    /// </summary>
    /// <param name="configuration">The configuration builder to configure.</param>
    static partial void Configure(IConfigurationBuilder configuration);

    /// <summary>
    /// Configures the command-line application with additional settings.
    /// </summary>
    /// <param name="serviceConfig">A tuple containing the service collection and configurator.</param>
    static partial void ConfigureApp((IServiceCollection services, IConfigurator configuration) serviceConfig);

    /// <summary>
    /// Configures additional in-memory configuration data.
    /// </summary>
    /// <param name="configData">The dictionary to store configuration key-value pairs.</param>
    static partial void ConfigureInMemory(IDictionary<string, string?> configData);
}
#endif