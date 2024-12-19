
namespace Devlead.Console.Integration.Test.Services;

public class TestService(IOptions<TestServiceSettings> settings)
{
    private TestServiceSettings Settings { get; } = settings.Value;

    public string GetVersion() => Settings.Version;
}
