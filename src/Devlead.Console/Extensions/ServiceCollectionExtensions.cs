#if UseDefaultProgram
namespace Devlead.Console.Extensions;

/// <summary>
/// Provides extension methods for IServiceCollection to configure command app services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds command app services to the service collection.
    /// </summary>
    /// <typeparam name="TAnsiConsole">The type of ANSI console to use.</typeparam>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="ansiConsole">The ANSI console instance.</param>
    /// <param name="exceptionFormat">The format options for exception output.</param>
    public static void AddCommandApp<TAnsiConsole>(
        this IServiceCollection services,
        TAnsiConsole ansiConsole,
        ExceptionFormats exceptionFormat = ExceptionFormats.NoStackTrace
        )
        where TAnsiConsole : class, IAnsiConsole
    {
        services
            .AddSingleton(ansiConsole)
            .AddSingleton<IAnsiConsole>(static services => services.GetRequiredService<TAnsiConsole>())
            .AddSingleton(Program.GetNewCommandApp(services, ansiConsole, exceptionFormat))
            .AddSingleton<ICommandApp>(static services => services.GetRequiredService<DependencyInjectionCommandApp>());
    }
}
#endif