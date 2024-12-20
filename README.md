# Devlead.Console

Devlead.Console is a streamlined NuGet source package designed to accelerate the development of .NET console applications. By providing out-of-the-box configurations for Inversion of Control (IoC), command-line parsing, logging, and other essential utilities, Devlead.Console allows developers to focus on building business logic rather than setting up foundational components.

## Features

- Dependency injection / IoC container via Microsoft.Extensions
- Command-line parsing via Spectre.Console
- Logging via Microsoft.Extensions.Logging.Console
- Configuration via Microsoft.Extensions.Configuration
- Source-link for GitHub (or Azure Repos by setting MSBuild property `AzureRepos` to `true`)
- ...

## Installation

```
dotnet add package Devlead.Console
```

## Usage Example

Here's how to create a console application using Devlead.Console, which comes with sensible defaults to streamline your setup. Additionally, you can utilize optional partial methods to tailor the project to your specific needs:

```csharp
public partial class Program
{
    // Configure in-memory settings (useful for development/testing)
    static partial void ConfigureInMemory(IDictionary<string, string?> configData)
    {
        configData.Add("TestService__Version", "1.0.0.0");
    }

    // Register your services
    static partial void AddServices(IServiceCollection services)
    {
        services
            .AddOptions<TestServiceSettings>()
            .BindConfiguration(nameof(TestService));

        services.AddSingleton<TestService>();
    }

    // Configure commands
    static partial void ConfigureApp((IServiceCollection services, IConfigurator configuration) serviceConfig)
    {
        // Add a root level command
        serviceConfig
            .AddCommand<TestCommand>("test")
                .WithDescription("Example test command.")
                .WithExample(["test"]);

        // Add nested commands using branches
        serviceConfig
            .AddBranch(
                "yolo",
                c => c.AddCommand<TestCommand>("test")
                        .WithDescription("Example test command.")
                        .WithExample(["yolo", "test"])
            );
    }
}
```

This example demonstrates:
- Setting up configuration values in memory for development
- Registering services with dependency injection
- Creating commands at both root and nested levels
- Adding command descriptions and usage examples

The resulting CLI will support commands like:
```bash
myapp test
myapp yolo test
```



