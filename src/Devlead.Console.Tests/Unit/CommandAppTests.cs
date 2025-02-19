namespace Devlead.Console.Tests.Unit;

public class CommandAppTests
{
    [TestCase("--help")]
    [TestCase("test")]
    [TestCase("test", "--test-version=1.0.0.0")]
    [TestCase("test", "--throw-error")]
    [TestCase("yolo")]
    public async Task RunAsync(params string[] args)
    {
        // Given
        var (commandApp, testConsole) = ServiceProviderFixture
                                            .GetRequiredService<ICommandApp, TestConsole>();

        // When
        var result = await commandApp.RunAsync(args);

        // Then
        await Verify(
                new
                {
                    ExitCode = result,
                    testConsole.Output
                }
            )
            .DontIgnoreEmptyCollections()
            .AddExtraSettings(setting => setting.DefaultValueHandling = Argon.DefaultValueHandling.Include);
    }
}
