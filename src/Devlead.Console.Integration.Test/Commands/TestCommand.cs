
namespace Devlead.Console.Integration.Test.Commands;

public class TestCommand(TestService testService) : Command<TestSettings>
{
    public override int Execute(CommandContext context, TestSettings settings)
    {
        var version = testService.GetVersion();
        
        AnsiConsole.WriteLine($"Version: {version}");
        AnsiConsole.WriteLine($"TestVersion: {settings.TestVersion}");

        return version == settings.TestVersion ? 0 : 1;
    }
}