using Spectre.Console.Cli.Internal.Configuration;

namespace Devlead.Console;

/// <summary>
/// A command-line application that uses Microsoft.Extensions.DependencyInjection for dependency injection.
/// </summary>
/// <param name="dependencyInjectionRegistrar">The dependency injection registrar.</param>
/// <param name="commandApp">The underlying command app.</param>
public class DependencyInjectionCommandApp(
    DependencyInjectionRegistrar dependencyInjectionRegistrar,
    CommandApp commandApp
    ) : IDisposable, ICommandApp
{
    /// <summary>
    /// Creates a new instance of <see cref="DependencyInjectionCommandApp"/> from an <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to use for dependency injection.</param>
    /// <returns>A new instance of <see cref="DependencyInjectionCommandApp"/>.</returns>
    public static DependencyInjectionCommandApp FromServiceCollection(IServiceCollection services)
    {
        var dependencyInjectionRegistrar = new DependencyInjectionRegistrar(services);
        var commandApp = new CommandApp(dependencyInjectionRegistrar);
        return new(dependencyInjectionRegistrar, commandApp);
    }

    /// <inheritdoc/>
    public void Configure(Action<IConfigurator> configuration)
        => commandApp.Configure(configuration);

    /// <inheritdoc/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        using(dependencyInjectionRegistrar)
        { }
    }

    /// <inheritdoc/>
    public int Run(IEnumerable<string> args)
        => commandApp.Run(args);

    /// <inheritdoc/>
    public Task<int> RunAsync(IEnumerable<string> args)
        => commandApp.RunAsync(args);

    /// <summary>
    /// Sets the default command.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <returns>A <see cref="DefaultCommandConfigurator"/> that can be used to configure the default command.</returns>
    public DefaultCommandConfigurator SetDefaultCommand<TCommand>()
        where TCommand : class, ICommand
        => commandApp.SetDefaultCommand<TCommand>();
}