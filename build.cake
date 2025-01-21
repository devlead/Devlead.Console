#tool dotnet:?package=GitVersion.Tool&version=6.1.0
#load "build/records.cake"
#load "build/helpers.cake"
using System.Xml.Linq;
using System.Xml.XPath;

/*****************************
 * Setup
 *****************************/
Setup(
    static context => {
         var assertedVersions = context.GitVersion(new GitVersionSettings
            {
                OutputType = GitVersionOutput.Json
            });

        var branchName = assertedVersions.BranchName;
        var isMainBranch = StringComparer.OrdinalIgnoreCase.Equals("main", branchName);

        var gh = context.GitHubActions();
        var buildDate = DateTime.UtcNow;
        var runNumber = gh.IsRunningOnGitHubActions
                            ? gh.Environment.Workflow.RunNumber
                            : (short)((buildDate - buildDate.Date).TotalSeconds/3);

        var version = FormattableString
                    .Invariant($"{buildDate:yyyy.M.d}.{runNumber}");

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
            context.BuildSystem().IsLocalBuild,
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
                .WithProperty("ContinuousIntegrationBuild", gh.IsRunningOnGitHubActions ? "true" : "false")
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
        static (context, data) => context.DotNetTool(
                "tool",
                new DotNetToolSettings {
                    ArgumentCustomization = args => args
                                                        .Append("run")
                                                        .Append("dpi")
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
                }
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
        static (context, data) => context
            .GitHubActions() is var gh && gh != null
                ?   gh.Commands
                    .UploadArtifact(data.ArtifactsPath,  $"Artifact_{gh.Environment.Runner.ImageOS ?? gh.Environment.Runner.OS}_{context.Environment.Runtime.BuiltFramework.Identifier}_{context.Environment.Runtime.BuiltFramework.Version}")
                : throw new Exception("GitHubActions not available")
    )
.Then("Integration-Test")
    .WithCriteria<BuildData>( (context, data) => data.ShouldRunIntegrationTests())
    .Does<BuildData>(
        static (context, data) => {
            DirectoryPath sourceProjectPath = data.ProjectRoot.Combine("Devlead.Console.Integration.Test");
            DirectoryPath targetProjectPath = data.IntegrationTestPath.Combine("Devlead.Console.Integration.Test");
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
