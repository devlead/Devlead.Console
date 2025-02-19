#if UseDefaultProgram
using Devlead.Console;
using Microsoft.Extensions.Configuration.Memory;
using Spectre.Console;

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

using DependencyInjectionCommandApp app = GetNewCommandApp(services);

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
    /// <param name="appServiceConfig">A AppServiceConfig containing the app, service collection and configurator.</param>
    static partial void ConfigureApp(AppServiceConfig appServiceConfig);

    /// <summary>
    /// Configures additional in-memory configuration data.
    /// </summary>
    /// <param name="configData">The dictionary to store configuration key-value pairs.</param>
    static partial void ConfigureInMemory(IDictionary<string, string?> configData);

    /// <summary>
    /// Creates and configures a new DependencyInjectionCommandApp instance.
    /// </summary>
    /// <param name="services">The ServiceCollection to use for dependency injection.</param>
    /// <param name="ansiConsole">The AnsiConsole instance to use for console output. If null, uses the default console.</param>
    /// <param name="exceptionFormat">The format options for exception output.</param>
    /// <returns>A configured DependencyInjectionCommandApp instance.</returns>
    public static DependencyInjectionCommandApp GetNewCommandApp(
        IServiceCollection services,
        IAnsiConsole? ansiConsole = null,
        ExceptionFormats exceptionFormat = ExceptionFormats.ShowLinks
        )
    {
        IAnsiConsole GetConsole() => AnsiConsole.Console = ansiConsole ?? AnsiConsole.Console;

        var app = DependencyInjectionCommandApp.FromServiceCollection(services);

        app.Configure(
            config =>
            {
                config.ConfigureConsole(GetConsole());
                config.UseAssemblyInformationalVersion();
                config.ValidateExamples();
                config.SetExceptionHandler(
                    (ex, _) => GetConsole().WriteException(ex, exceptionFormat)
                    );

                ConfigureApp(new(app, services, config));
            });
        return app;
    }
}
#endif