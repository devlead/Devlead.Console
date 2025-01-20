namespace Devlead.Console.Extensions;

/// <summary>
/// Represents the configuration for application services.
/// </summary>
public record AppServiceConfig(CommandApp App, IServiceCollection Services, IConfigurator Configurator); 

/// <summary>
/// Represents the configuration for application services with custom settings.
/// </summary>
/// <typeparam name="TSettings">The type of command settings.</typeparam>
/// <param name="App">The command application instance.</param>
/// <param name="Services">The service collection.</param>
/// <param name="Configurator">The command configurator with settings.</param>
public record AppServiceConfig<TSettings>(CommandApp App, IServiceCollection Services, IConfigurator<TSettings> Configurator)
    where TSettings : CommandSettings
{
    /// <summary>
    /// Adds a command with the specified settings type.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to add.</typeparam>
    /// <param name="name">The name of the command.</param>
    /// <returns>The app service command configuration.</returns>
    public AppServiceCommandConfig AddCommand<TCommand>(string name)
        where TCommand : class, ICommandLimiter<TSettings>
        => this.AddCommand<TCommand, TSettings>(name);
}

/// <summary>
/// Represents the configuration for application services with command configuration.
/// </summary>
/// <param name="App">The command application instance.</param>
/// <param name="Services">The service collection.</param>
/// <param name="CommandConfigurator">The command configurator.</param>
public record AppServiceCommandConfig(CommandApp App, IServiceCollection Services, ICommandConfigurator CommandConfigurator);

/// <summary>
/// Represents the configuration for application services with branch configuration.
/// </summary>
/// <param name="App">The command application instance.</param>
/// <param name="Services">The service collection.</param>
/// <param name="BranchConfigurator">The branch configurator.</param>
public record AppServiceBranchConfig(CommandApp App, IServiceCollection Services, IBranchConfigurator BranchConfigurator);