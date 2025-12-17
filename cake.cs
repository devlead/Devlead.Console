#:sdk Cake.Sdk@6.0.0
#:property IncludeAdditionalFiles=./build/*.cs

using System.Xml.Linq;
using System.Xml.XPath;

/*****************************
 * Setup
 *****************************/
Setup(
    static context => {
        InstallTool("dotnet:https://api.nuget.org/v3/index.json?package=DPI&version=2025.11.25.337");
        InstallTool("dotnet:https://api.nuget.org/v3/index.json?package=GitVersion.Tool&version=6.5.1");
         var assertedVersions = context.GitVersion(new GitVersionSettings
            {
                OutputType = GitVersionOutput.Json
            });

        var branchName = assertedVersions.BranchName;
        var isMainBranch = StringComparer.OrdinalIgnoreCase.Equals("main", branchName);

        var buildDate = DateTime.UtcNow;
        var runNumber = GitHubActions.IsRunningOnGitHubActions
                            ? GitHubActions.Environment.Workflow.RunNumber
                            : 0;
      
        var suffix = runNumber == 0 
                       ? $"-{(short)((buildDate - buildDate.Date).TotalSeconds/3)}"
                       : string.Empty;

        var version = FormattableString
                          .Invariant($"{buildDate:yyyy.M.d}.{runNumber}{suffix}");

        context.Information("Building version {0} (Branch: {1}, IsMain: {2})",
            version,
            branchName,
            isMainBranch);

        var artifactsPath = context
                            .MakeAbsolute(context.Directory("./artifacts"));

        var projectRoot =  context
                            .MakeAbsolute(context.Directory("./src"));

        var projectPath = projectRoot.CombineWithFilePath("Devlead.Console/Devlead.Console.csproj");

        return new BuildData(
            version,
            isMainBranch,
            !context.IsRunningOnWindows(),
            BuildSystem.IsLocalBuild,
            projectRoot,
            projectPath,
            new DotNetMSBuildSettings()
                .SetConfiguration("Release")
                .SetVersion(version)
                .WithProperty("Copyright", $"Mattias Karlsson © {DateTime.UtcNow.Year}")
                .WithProperty("Authors", "devlead")
                .WithProperty("Company", "devlead")
                .WithProperty("PackageLicenseExpression", "MIT")
                .WithProperty("PackageTags", "Console")
                .WithProperty("PackageDescription", "An opinionated .NET source package simplifying bootstrapping console applications with IoC, command-line parsing, logging, and more.")
                .WithProperty("RepositoryUrl", "https://github.com/devlead/Devlead.Console.git")
                .WithProperty("ContinuousIntegrationBuild", GitHubActions.IsRunningOnGitHubActions ? "true" : "false")
                .WithProperty("EmbedUntrackedSources", "true"),
            artifactsPath,
            artifactsPath.Combine(version)
            );
    }
);

/*****************************
 * Tasks
 *****************************/
Task("Clean")
    .Does<BuildData>(
        static (context, data) => context.CleanDirectories(data.DirectoryPathsToClean)
    )
.Then("Restore")
    .Does<BuildData>(
        static (context, data) => context.DotNetRestore(
            data.ProjectRoot.FullPath,
            new DotNetRestoreSettings {
                MSBuildSettings = data.MSBuildSettings
            }
        )
    )
.Then("DPI")
    .Does<BuildData>(
        static (context, data) => Command(
                ["dpi", "dpi.exe"],
                new ProcessArgumentBuilder()
                    .Append("nuget")
                    .Append("--silent")
                    .AppendSwitchQuoted("--output", "table")
                    .Append(
                        (
                            !string.IsNullOrWhiteSpace(context.EnvironmentVariable("NuGetReportSettings_SharedKey"))
                            &&
                            !string.IsNullOrWhiteSpace(context.EnvironmentVariable("NuGetReportSettings_WorkspaceId"))
                        )
                            ? "report"
                            : "analyze"
                        )
                    .AppendSwitchQuoted("--buildversion", data.Version)
                
            )
    )
.Then("Build")
    .DoesForEach<BuildData, FilePath>(
        static (data, context) => context.GetFiles(data.ProjectRoot.FullPath + "/**/*.csproj")
                                    .OrderBy(path => path.FullPath.EndsWith("Devlead.Console.csproj") ? 0 : 1)
                                    .ThenBy(path => path.FullPath, StringComparer.OrdinalIgnoreCase),
        static (data, item, context) => context.DotNetBuild(
            item.FullPath,
            new DotNetBuildSettings {
                NoRestore = true,
                MSBuildSettings = data.MSBuildSettings
            }
        )
    )
.Then("Test")
    .Does<BuildData>(
        static (context, data) => context.DotNetTest(
            data.ProjectRoot.FullPath,
            new DotNetTestSettings {
                NoBuild = true,
                NoRestore = true,
                MSBuildSettings = data.MSBuildSettings
            }
        )
    )
.Then("Pack")
    .Does<BuildData>(
        static (context, data) => context.DotNetPack(
            data.ProjectPath.FullPath,
            new DotNetPackSettings {
                NoBuild = true,
                NoRestore = true,
                OutputDirectory = data.NuGetOutputPath,
                MSBuildSettings = data.MSBuildSettings
            }
        )
    )
.Then("Upload-Artifacts")
    .WithCriteria(BuildSystem.IsRunningOnGitHubActions, nameof(BuildSystem.IsRunningOnGitHubActions))
    .Does<BuildData>(
        static (context, data) => GitHubActions
            .Commands
            .UploadArtifact(data.ArtifactsPath, $"Artifact_{GitHubActions.Environment.Runner.ImageOS ?? GitHubActions.Environment.Runner.OS}_{context.Environment.Runtime.BuiltFramework.Identifier}_{context.Environment.Runtime.BuiltFramework.Version}")
    )
.Then("Integration-Test")
    .WithCriteria<BuildData>( (context, data) => data.ShouldRunIntegrationTests())
     .DoesForEach<BuildData, string>(
        static (data, context) => new [] { "net9.0", "net10.0" },
        static (data, targetFramework, context) => {
            context.Information("Running integration tests for {0}", targetFramework);
            DirectoryPath sourceProjectPath = data.ProjectRoot.Combine("Devlead.Console.Integration.Test");
            DirectoryPath targetProjectPath = data.IntegrationTestPath.Combine($"Devlead.Console.Integration.Test.{targetFramework}");
            FilePath nuGetConfigPath = data.IntegrationTestPath.CombineWithFilePath("nuget.config");
            FilePath cpmPath = data.IntegrationTestPath.CombineWithFilePath("Directory.Packages.props");

            context.CopyDirectory(sourceProjectPath, targetProjectPath);
            context.CleanDirectories(
                [
                    targetProjectPath.Combine("bin").FullPath,
                    targetProjectPath.Combine("obj").FullPath
                ]
            );
            
            using(var stream = context.FileSystem.GetFile(nuGetConfigPath).OpenWrite())
            {
                ReadOnlySpan<byte> content = System.Text.Encoding.UTF8.GetBytes(
                                                    $"""
                                                    <configuration>
                                                        <packageSources>
                                                            <clear />
                                                            <add key="artifacts" value="{data.NuGetOutputPath}" />
                                                            <add key="nuget" value="https://api.nuget.org/v3/index.json" />
                                                        </packageSources>
                                                        <packageSourceMapping>
                                                            <packageSource key="artifacts">
                                                                <package pattern="Devlead.*" />
                                                            </packageSource>
                                                            <packageSource key="nuget">
                                                                <package pattern="*" />
                                                            </packageSource>
                                                        </packageSourceMapping>
                                                    </configuration>
                                                    """
                                                );

                stream.Write(content);
            }

            using(Stream
                    stream = context.FileSystem.GetFile(cpmPath).OpenWrite(),
                    xmlStream = context.FileSystem.GetFile("./src/Directory.Packages.props").OpenRead())
            {
                

                ReadOnlySpan<byte> content = System.Text.Encoding.UTF8.GetBytes(
                                                    $"""
                                                    <Project>
                                                        <ItemGroup>
                                                           <PackageVersion Include="Devlead.Console" Version="{data.Version}" />{
                                                            string.Concat(
                                                                XDocument
                                                                    .Load(xmlStream)
                                                                    .Descendants("PackageVersion")
                                                                    .Select(packageVersion =>    
                                                    $"""

                                                           <PackageVersion Include="{packageVersion.Attribute("Include")?.Value}" Version="{packageVersion.Attribute("Version")?.Value}" />
                                                    """)
                                                            )}
                                                        </ItemGroup>
                                                    </Project>
                                                    """
                                                );
                stream.Write(content);
            }

            context.DotNetRun(
                targetProjectPath.FullPath,
                new ProcessArgumentBuilder()
                    .Append("test")
                    .AppendSwitchQuoted("--test-version", data.Version),
                new DotNetRunSettings {
                    EnvironmentVariables = {
                        { "TestService__Version", data.Version }
                    },
                    MSBuildSettings = new DotNetMSBuildSettings()
                        .SetConfiguration("IntegrationTest")
                        .WithProperty("DevleadConsoleVersion", data.Version)
                        .WithProperty("TargetFramework", targetFramework)
                }
            );
        }
    )
.Default()
.Then("Push-GitHub-Packages")
    .WithCriteria<BuildData>( (context, data) => data.ShouldPushGitHubPackages())
    .DoesForEach<BuildData, FilePath>(
        static (data, context)
            => context.GetFiles(data.NuGetOutputPath.FullPath + "/*.nupkg"),
        static (data, item, context)
            => context.DotNetNuGetPush(
                item.FullPath,
            new DotNetNuGetPushSettings
            {
                Source = data.GitHubNuGetSource,
                ApiKey = data.GitHubNuGetApiKey
            }
        )
    )
.Then("Push-NuGet-Packages")
    .WithCriteria<BuildData>( (context, data) => data.ShouldPushNuGetPackages())
    .DoesForEach<BuildData, FilePath>(
        static (data, context)
            => context.GetFiles(data.NuGetOutputPath.FullPath + "/*.nupkg"),
        static (data, item, context)
            => context.DotNetNuGetPush(
                item.FullPath,
                new DotNetNuGetPushSettings
                {
                    Source = data.NuGetSource,
                    ApiKey = data.NuGetApiKey
                }
        )
    )
.Then("Create-GitHub-Release")
    .WithCriteria<BuildData>( (context, data) => data.ShouldPushNuGetPackages())
    .Does<BuildData>(
        static (context, data) => context
            .Command(
                new CommandSettings {
                    ToolName = "GitHub CLI",
                    ToolExecutableNames = new []{ "gh.exe", "gh" },
                    EnvironmentVariables = { { "GH_TOKEN", data.GitHubNuGetApiKey } }
                },
                new ProcessArgumentBuilder()
                    .Append("release")
                    .Append("create")
                    .Append(data.Version)
                    .AppendSwitchQuoted("--title", data.Version)
                    .Append("--generate-notes")
                    .Append(string.Join(
                        ' ',
                        context
                            .GetFiles(data.NuGetOutputPath.FullPath + "/*.nupkg")
                            .Select(path => path.FullPath.Quote())
                        ))

            )
    )
.Then("GitHub-Actions")
.Run();
