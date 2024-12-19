using System.Runtime;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
    public static (IServiceCollection services, IBranchConfigurator configurator) AddBranch(
        this (IServiceCollection services, IConfigurator configurator) serviceConfig,
        string name,
        Action<(IServiceCollection, IConfigurator<CommandSettings>)> action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(action);
       
        return (
            serviceConfig.services,
            serviceConfig.configurator.AddBranch(name, configurator=>action((serviceConfig.services, configurator)))
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
    public static (IServiceCollection services, ICommandConfigurator configurator) AddCommand<TCommand>(
        this (IServiceCollection services, IConfigurator configurator) serviceConfig,
        string name
        )
        where TCommand : class, ICommand
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        serviceConfig.services.TryAddSingleton<TCommand>();

        return (
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
    public static (IServiceCollection services, ICommandConfigurator configurator) WithExample(this (IServiceCollection services, ICommandConfigurator configurator) serviceConfig, params string[] args)
        => (
            serviceConfig.services,
            serviceConfig.configurator.WithExample(args)
        );

    /// <summary>
    /// Adds an alias (an alternative name) to the command being configured.
    /// </summary>
    /// <param name="serviceConfig">The service configuration tuple.</param>
    /// <param name="name">The alias to add to the command being configured.</param>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    public static (IServiceCollection services, ICommandConfigurator configurator) WithAlias(this (IServiceCollection services, ICommandConfigurator configurator) serviceConfig, string name)
        => (
            serviceConfig.services,
            serviceConfig.configurator.WithAlias(name)
        );

    /// <summary>
    /// Sets the description of the command.
    /// </summary>
    /// <param name="serviceConfig">The service configuration tuple.</param>
    /// <param name="description">The command description.</param>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    public static (IServiceCollection services, ICommandConfigurator configurator) WithDescription(this (IServiceCollection services, ICommandConfigurator configurator) serviceConfig, string description)
        => (
            serviceConfig.services,
            serviceConfig.configurator.WithDescription(description)
        );

    /// <summary>
    /// Sets data that will be passed to the command via the <see cref="CommandContext"/>.
    /// </summary>
    /// <param name="serviceConfig">The service configuration tuple.</param>
    /// <param name="data">The data to pass to the command.</param>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    public static (IServiceCollection services, ICommandConfigurator configurator) WithData(this (IServiceCollection services, ICommandConfigurator configurator) serviceConfig, object data)
        => (
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
    public static (IServiceCollection services, ICommandConfigurator configurator) IsHidden(this (IServiceCollection services, ICommandConfigurator configurator) serviceConfig)
    => (
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
    public static (IServiceCollection services, ICommandConfigurator configurator) AddCommand<TCommand>(
        this (IServiceCollection services, IConfigurator<CommandSettings> configurator) serviceConfig,
        string name)
        where TCommand : class, ICommandLimiter<CommandSettings>
    {
        return (
           serviceConfig.services,
           serviceConfig.configurator.AddCommand<TCommand>(name)
           );
    }
}
