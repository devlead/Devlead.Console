namespace Devlead.Console.Extensions;

/// <summary>
/// Provides extension methods for configuring application commands and branches.
/// </summary>
public static class AppConfiguratorExtensions
{
    /// <summary>
    /// Adds a new branch to the command configuration.
    /// </summary>
    /// <param name="serviceConfig">The app service configuration.</param>
    /// <param name="name">The name of the branch.</param>
    /// <param name="action">The configuration action for the branch.</param>
    /// <returns>A record containing the app, service collection and branch configurator.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
    public static AppServiceBranchConfig AddBranch(
        this AppServiceConfig serviceConfig,
        string name,
        Action<AppServiceConfig<CommandSettings>>action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(action);
       
        return new AppServiceBranchConfig(
            serviceConfig.App,
            serviceConfig.Services,
            serviceConfig.Configurator.AddBranch(name, configurator=>action(new AppServiceConfig<CommandSettings>(serviceConfig.App, serviceConfig.Services, configurator)))
            );
    }

    /// <summary>
    /// Adds a command to the configuration.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to add.</typeparam>
    /// <param name="serviceConfig">The app service configuration.</param>
    /// <param name="name">The name of the command.</param>
    /// <returns>A record containing the service collection and command configurator.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    public static AppServiceCommandConfig AddCommand<TCommand>(
        this AppServiceConfig serviceConfig,
        string name
        )
        where TCommand : class, ICommand
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        serviceConfig.Services.TryAddSingleton<TCommand>();

        return new AppServiceCommandConfig(
            serviceConfig.App,
            serviceConfig.Services,
            serviceConfig.Configurator.AddCommand<TCommand>(name)
            );
    }


    /// <summary>
    /// Adds an example of how to use the command.
    /// </summary>
    /// <param name="serviceConfig">The app service configuration.</param>
    /// <param name="args">The example arguments.</param>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    public static AppServiceCommandConfig WithExample(this AppServiceCommandConfig serviceConfig, params string[] args)
        => serviceConfig with {
            CommandConfigurator = serviceConfig.CommandConfigurator.WithExample(args)
        };

    /// <summary>
    /// Adds an alias (an alternative name) to the command being configured.
    /// </summary>
    /// <param name="serviceConfig">The app service configuration.</param>
    /// <param name="name">The alias to add to the command being configured.</param>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    public static AppServiceCommandConfig WithAlias(this AppServiceCommandConfig serviceConfig, string name)
        => serviceConfig with {
            CommandConfigurator = serviceConfig.CommandConfigurator.WithAlias(name)
        };

    /// <summary>
    /// Sets the description of the command.
    /// </summary>
    /// <param name="serviceConfig">The app service configuration.</param>
    /// <param name="description">The command description.</param>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    public static AppServiceCommandConfig WithDescription(this AppServiceCommandConfig serviceConfig, string description)
        => serviceConfig with {
            CommandConfigurator = serviceConfig.CommandConfigurator.WithDescription(description)
        };

    /// <summary>
    /// Sets data that will be passed to the command via the <see cref="CommandContext"/>.
    /// </summary>
    /// <param name="serviceConfig">The app service configuration.</param>
    /// <param name="data">The data to pass to the command.</param>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    public static AppServiceCommandConfig WithData(this AppServiceCommandConfig serviceConfig, object data)
        => serviceConfig with {
            CommandConfigurator = serviceConfig.CommandConfigurator.WithData(data)
        };

    /// <summary>
    /// Marks the command as hidden.
    /// Hidden commands do not show up in help documentation or
    /// generated XML models.
    /// </summary>
    /// <param name="serviceConfig">The app service configuration.</param>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    public static AppServiceCommandConfig IsHidden(this AppServiceCommandConfig serviceConfig)
    => serviceConfig with {
            CommandConfigurator = serviceConfig.CommandConfigurator.IsHidden()
        };

    /// <summary>
    /// Adds a command with settings configuration.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to add.</typeparam>
    /// <param name="serviceConfig">The app service configuration.</param>
    /// <param name="name">The name of the command.</param>
    /// <returns>A recor containing the app, service collection and command configurator.</returns>
    public static AppServiceCommandConfig AddCommand<TCommand>(
        this AppServiceConfig<CommandSettings> serviceConfig,
        string name)
        where TCommand : class, ICommandLimiter<CommandSettings>
    {
        return new AppServiceCommandConfig(
            serviceConfig.App,
            serviceConfig.Services,
            serviceConfig.Configurator.AddCommand<TCommand>(name)
           );
    }

    /// <summary>
    /// Sets the name of the application.
    /// </summary>
    /// <param name="serviceConfig">The app service configuration.</param>
    /// <param name="name">The name of the application.</param>
    /// <returns>A configurator that can be used to configure the application further.</returns>
    public static  AppServiceConfig SetApplicationName(
        this AppServiceConfig serviceConfig,
        string name)
    {
        serviceConfig.Configurator.SetApplicationName(name);
        return serviceConfig;
    }

    /// <summary>
    /// Sets the default command for the application.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to set as default.</typeparam>
    /// <param name="serviceConfig">The app service configuration.</param>
    /// <returns>A record containing the updated service configuration.</returns>
    public static AppServiceConfig SetDefaultCommand<TCommand>(
        this AppServiceConfig serviceConfig)
        where TCommand : class, ICommand
    {
        serviceConfig.App.SetDefaultCommand<TCommand>();
        return serviceConfig;
    }
}
