namespace Devlead.Console.Extensions;

/// <summary>
/// Provides extension methods for configuring application commands and branches.
/// </summary>
public static class AppConfiguratorExtensions
{
    /// <summary>
    /// Adds a new branch to the command configuration.
    /// </summary>
    /// <param name="appServiceConfig">The app service configuration.</param>
    /// <param name="name">The name of the branch.</param>
    /// <param name="action">The configuration action for the branch.</param>
    /// <returns>A record containing the app, service collection and branch configurator.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
    public static AppServiceBranchConfig AddBranch(
        this AppServiceConfig appServiceConfig,
        string name,
        Action<AppServiceConfig<CommandSettings>>action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(action);
       
        return new AppServiceBranchConfig(
            appServiceConfig.App,
            appServiceConfig.Services,
            appServiceConfig.Configurator.AddBranch(name, configurator=>action(new AppServiceConfig<CommandSettings>(appServiceConfig.App, appServiceConfig.Services, configurator)))
            );
    }

    /// <summary>
    /// Adds a new branch to the command configuration.
    /// </summary>
    /// <typeparam name="TSettings">The type of command settings.</typeparam>
    /// <param name="appServiceConfig">The app service configuration.</param>
    /// <param name="name">The name of the branch.</param>
    /// <param name="action">The configuration action for the branch.</param>
    /// <returns>A record containing the app, service collection and branch configurator.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
    public static AppServiceBranchConfig AddBranch<TSettings>(
        this AppServiceConfig appServiceConfig,
        string name,
        Action<AppServiceConfig<TSettings>>action)
        where TSettings : CommandSettings
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(action);
       
        return new AppServiceBranchConfig(
            appServiceConfig.App,
            appServiceConfig.Services,
            appServiceConfig.Configurator.AddBranch<TSettings>(name, configurator=>action(new AppServiceConfig<TSettings>(appServiceConfig.App, appServiceConfig.Services, configurator)))
            );
    }

    /// <summary>
    /// Adds a command to the configuration.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to add.</typeparam>
    /// <param name="appServiceConfig">The app service configuration.</param>
    /// <param name="name">The name of the command.</param>
    /// <returns>A record containing the service collection and command configurator.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    public static AppServiceCommandConfig AddCommand<TCommand>(
        this AppServiceConfig appServiceConfig,
        string name
        )
        where TCommand : class, ICommand
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        appServiceConfig.Services.TryAddSingleton<TCommand>();

        return new AppServiceCommandConfig(
            appServiceConfig.App,
            appServiceConfig.Services,
            appServiceConfig.Configurator.AddCommand<TCommand>(name)
            );
    }


    /// <summary>
    /// Adds an example of how to use the command.
    /// </summary>
    /// <param name="appServiceCommandConfig">The app service configuration.</param>
    /// <param name="args">The example arguments.</param>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    public static AppServiceCommandConfig WithExample(this AppServiceCommandConfig appServiceCommandConfig, params string[] args)
        => appServiceCommandConfig with {
            CommandConfigurator = appServiceCommandConfig.CommandConfigurator.WithExample(args)
        };

    /// <summary>
    /// Adds an alias (an alternative name) to the command being configured.
    /// </summary>
    /// <param name="appServiceCommandConfig">The app service configuration.</param>
    /// <param name="name">The alias to add to the command being configured.</param>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    public static AppServiceCommandConfig WithAlias(this AppServiceCommandConfig appServiceCommandConfig, string name)
        => appServiceCommandConfig with {
            CommandConfigurator = appServiceCommandConfig.CommandConfigurator.WithAlias(name)
        };

    /// <summary>
    /// Sets the description of the command.
    /// </summary>
    /// <param name="appServiceCommandConfig">The app service configuration.</param>
    /// <param name="description">The command description.</param>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    public static AppServiceCommandConfig WithDescription(this AppServiceCommandConfig appServiceCommandConfig, string description)
        => appServiceCommandConfig with {
            CommandConfigurator = appServiceCommandConfig.CommandConfigurator.WithDescription(description)
        };

    /// <summary>
    /// Sets data that will be passed to the command via the <see cref="CommandContext"/>.
    /// </summary>
    /// <param name="appServiceCommandConfig">The app service configuration.</param>
    /// <param name="data">The data to pass to the command.</param>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    public static AppServiceCommandConfig WithData(this AppServiceCommandConfig appServiceCommandConfig, object data)
        => appServiceCommandConfig with {
            CommandConfigurator = appServiceCommandConfig.CommandConfigurator.WithData(data)
        };

    /// <summary>
    /// Marks the command as hidden.
    /// Hidden commands do not show up in help documentation or
    /// generated XML models.
    /// </summary>
    /// <param name="appServiceCommandConfig">The app service configuration.</param>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    public static AppServiceCommandConfig IsHidden(this AppServiceCommandConfig appServiceCommandConfig)
    => appServiceCommandConfig with {
            CommandConfigurator = appServiceCommandConfig.CommandConfigurator.IsHidden()
        };

    /// <summary>
    /// Adds a command with settings configuration.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to add.</typeparam>
    /// <param name="appServiceConfig">The app service configuration.</param>
    /// <param name="name">The name of the command.</param>
    /// <returns>A recor containing the app, service collection and command configurator.</returns>
    public static AppServiceCommandConfig AddCommand<TCommand>(
        this AppServiceConfig<CommandSettings> appServiceConfig,
        string name)
        where TCommand : class, ICommandLimiter<CommandSettings>
    {
        return new AppServiceCommandConfig(
            appServiceConfig.App,
            appServiceConfig.Services,
            appServiceConfig.Configurator.AddCommand<TCommand>(name)
           );
    }

    /// <summary>
    /// Sets the name of the application.
    /// </summary>
    /// <param name="appServiceConfig">The app service configuration.</param>
    /// <param name="name">The name of the application.</param>
    /// <returns>A configurator that can be used to configure the application further.</returns>
    public static  AppServiceConfig SetApplicationName(
        this AppServiceConfig appServiceConfig,
        string name)
    {
        appServiceConfig.Configurator.SetApplicationName(name);
        return appServiceConfig;
    }

    /// <summary>
    /// Sets the default command for the application.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to set as default.</typeparam>
    /// <param name="appServiceConfig">The app service configuration.</param>
    /// <returns>A record containing the updated service configuration.</returns>
    public static AppServiceConfig SetDefaultCommand<TCommand>(
        this AppServiceConfig appServiceConfig)
        where TCommand : class, ICommand
    {
        appServiceConfig.App.SetDefaultCommand<TCommand>();
        return appServiceConfig;
    }

    /// <summary>
    /// Sets the description for the command settings configuration.
    /// </summary>
    /// <typeparam name="TSettings">The type of command settings.</typeparam>
    /// <param name="appServiceConfig">The app service configuration.</param>
    /// <param name="description">The description to set.</param>
    /// <returns>The app service configuration.</returns>
    public static AppServiceConfig<TSettings> SetDescription<TSettings>(this AppServiceConfig<TSettings> appServiceConfig, string description)
        where TSettings : CommandSettings
    {
        appServiceConfig.Configurator.SetDescription(description);
        return appServiceConfig;
    }

    /// <summary>
    /// Adds a command with custom settings configuration.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to add.</typeparam>
    /// <typeparam name="TSettings">The type of settings for the command.</typeparam>
    /// <param name="appServiceConfig">The app service configuration.</param>
    /// <param name="name">The name of the command.</param>
    /// <returns>A record containing the app service command configuration.</returns>
    public static AppServiceCommandConfig AddCommand<TCommand, TSettings>(this AppServiceConfig<TSettings> appServiceConfig, string name)
        where TCommand : class, ICommandLimiter<TSettings>
        where TSettings : CommandSettings
    {
        return new AppServiceCommandConfig(
            appServiceConfig.App,
            appServiceConfig.Services,
            appServiceConfig.Configurator.AddCommand<TCommand>(name)
           );
    }
}
