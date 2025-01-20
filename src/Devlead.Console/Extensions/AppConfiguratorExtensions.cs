namespace Devlead.Console.Extensions;

/// <summary>
/// Provides extension methods for configuring application commands and branches.
/// </summary>
public static class AppConfiguratorExtensions
{
    /// <summary>
    /// Adds a new branch to the command configuration.
    /// </summary>
    /// <param name="serviceConfig">The service configuration tuple.</param>
    /// <param name="name">The name of the branch.</param>
    /// <param name="action">The configuration action for the branch.</param>
    /// <returns>A tuple containing the service collection and branch configurator.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
    public static (CommandApp app, IServiceCollection services, IBranchConfigurator configurator) AddBranch(
        this (CommandApp app, IServiceCollection services, IConfigurator configurator) serviceConfig,
        string name,
        Action<(CommandApp app, IServiceCollection, IConfigurator<CommandSettings>)> action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(action);
       
        return (
            serviceConfig.app,
            serviceConfig.services,
            serviceConfig.configurator.AddBranch(name, configurator=>action((serviceConfig.app, serviceConfig.services, configurator)))
            );
    }

    /// <summary>
    /// Adds a command to the configuration.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to add.</typeparam>
    /// <param name="serviceConfig">The service configuration tuple.</param>
    /// <param name="name">The name of the command.</param>
    /// <returns>A tuple containing the service collection and command configurator.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    public static (CommandApp app, IServiceCollection services, ICommandConfigurator configurator) AddCommand<TCommand>(
        this (CommandApp app, IServiceCollection services, IConfigurator configurator) serviceConfig,
        string name
        )
        where TCommand : class, ICommand
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        serviceConfig.services.TryAddSingleton<TCommand>();

        return (
            serviceConfig.app,
            serviceConfig.services,
            serviceConfig.configurator.AddCommand<TCommand>(name)
            );
    }


    /// <summary>
    /// Adds an example of how to use the command.
    /// </summary>
    /// <param name="serviceConfig">The service configuration tuple.</param>
    /// <param name="args">The example arguments.</param>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    public static (CommandApp app, IServiceCollection services, ICommandConfigurator configurator) WithExample(this (CommandApp app, IServiceCollection services, ICommandConfigurator configurator) serviceConfig, params string[] args)
        => (
            serviceConfig.app,
            serviceConfig.services,
            serviceConfig.configurator.WithExample(args)
        );

    /// <summary>
    /// Adds an alias (an alternative name) to the command being configured.
    /// </summary>
    /// <param name="serviceConfig">The service configuration tuple.</param>
    /// <param name="name">The alias to add to the command being configured.</param>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    public static (CommandApp app, IServiceCollection services, ICommandConfigurator configurator) WithAlias(this (CommandApp app, IServiceCollection services, ICommandConfigurator configurator) serviceConfig, string name)
        => (
            serviceConfig.app,
            serviceConfig.services,
            serviceConfig.configurator.WithAlias(name)
        );

    /// <summary>
    /// Sets the description of the command.
    /// </summary>
    /// <param name="serviceConfig">The service configuration tuple.</param>
    /// <param name="description">The command description.</param>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    public static (CommandApp app, IServiceCollection services, ICommandConfigurator configurator) WithDescription(this (CommandApp app, IServiceCollection services, ICommandConfigurator configurator) serviceConfig, string description)
        => (
            serviceConfig.app,
            serviceConfig.services,
            serviceConfig.configurator.WithDescription(description)
        );

    /// <summary>
    /// Sets data that will be passed to the command via the <see cref="CommandContext"/>.
    /// </summary>
    /// <param name="serviceConfig">The service configuration tuple.</param>
    /// <param name="data">The data to pass to the command.</param>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    public static (CommandApp app, IServiceCollection services, ICommandConfigurator configurator) WithData(this (CommandApp app, IServiceCollection services, ICommandConfigurator configurator) serviceConfig, object data)
        => (
            serviceConfig.app,
            serviceConfig.services,
            serviceConfig.configurator.WithData(data)
        );

    /// <summary>
    /// Marks the command as hidden.
    /// Hidden commands do not show up in help documentation or
    /// generated XML models.
    /// </summary>
    /// <param name="serviceConfig">The service configuration tuple.</param>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    public static (CommandApp app, IServiceCollection services, ICommandConfigurator configurator) IsHidden(this (CommandApp app, IServiceCollection services, ICommandConfigurator configurator) serviceConfig)
    => (
            serviceConfig.app,
            serviceConfig.services,
            serviceConfig.configurator.IsHidden()
        );

    /// <summary>
    /// Adds a command with settings configuration.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to add.</typeparam>
    /// <param name="serviceConfig">The service configuration tuple.</param>
    /// <param name="name">The name of the command.</param>
    /// <returns>A tuple containing the service collection and command configurator.</returns>
    public static (CommandApp app, IServiceCollection services, ICommandConfigurator configurator) AddCommand<TCommand>(
        this (CommandApp app, IServiceCollection services, IConfigurator<CommandSettings> configurator) serviceConfig,
        string name)
        where TCommand : class, ICommandLimiter<CommandSettings>
    {
        return (
            serviceConfig.app,
            serviceConfig.services,
            serviceConfig.configurator.AddCommand<TCommand>(name)
           );
    }

    /// <summary>
    /// Sets the name of the application.
    /// </summary>
    /// <param name="serviceConfig">The service configuration tuple.</param>
    /// <param name="name">The name of the application.</param>
    /// <returns>A configurator that can be used to configure the application further.</returns>
    public static  (CommandApp app, IServiceCollection services, IConfigurator configuration) SetApplicationName(
        this (CommandApp app, IServiceCollection services, IConfigurator configuration) serviceConfig,
        string name)
    {
        ArgumentNullException.ThrowIfNull(serviceConfig.configuration);

        serviceConfig.configuration.SetApplicationName(name);
        return serviceConfig;
    }

    /// <summary>
    /// Sets the default command for the application.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to set as default.</typeparam>
    /// <param name="serviceConfig">The service configuration tuple.</param>
    /// <returns>A tuple containing the updated service configuration.</returns>
    public static (CommandApp app, IServiceCollection services, IConfigurator configuration) SetDefaultCommand<TCommand>(
        this (CommandApp app, IServiceCollection services, IConfigurator configuration) serviceConfig)
        where TCommand : class, ICommand
    {
            serviceConfig.app.SetDefaultCommand<TCommand>();
            return (serviceConfig.app, serviceConfig.services, serviceConfig.configuration);
    }
}
