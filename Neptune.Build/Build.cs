using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.GitVersion;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.Tooling.ProcessTasks;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

class Build : NukeBuild
{
    // Console application entry point. Also defines the default target.
    public static int Main() => Execute<Build>(x => x.Compile);

    // Auto-injection fields:

    // [GitVersion] readonly GitVersion GitVersion;
    // Semantic versioning. Must have 'GitVersion.CommandLine' referenced.

    // [GitRepository] readonly GitRepository GitRepository;
    // Parses origin, branch name and head from git config.

    // [Parameter] readonly string MyGetApiKey;
    // Returns command-line arguments and environment variables.

    // [Solution] readonly Solution Solution;
    // Provides access to the structure of the solution.

    Target CheckSystem => _ => _
        .Executes(() =>
        {
            Console.WriteLine(
                $"Your system is: {Environment.NewLine}{EnvironmentInfo.Platform}{Environment.NewLine}{(EnvironmentInfo.Is64Bit ? "x64" : "x86")}");
            
            if (EnvironmentInfo.IsUnix == false)
            {
                throw new Exception("Sorry, for now building is supported only on Linux machines");
            }
        });
    
    Target Clean => _ => _
        .OnlyWhen(() => false) // Disabled for safety.
        .Executes(() =>
        {
            DeleteDirectories(GlobDirectories(SourceDirectory, "**/bin", "**/obj"));
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => DefaultDotNetRestore);
        });

    Target BuildShaders => _ => _
        .DependsOn(CheckSystem)
        .Executes(() =>
        {
            var process = StartProcess(SolutionDirectory / "Neptune" / "build-shaders.sh", workingDirectory: SolutionDirectory / "Neptune");
            process.WaitForExit();
        });


    Target Compile => _ => _
        .DependsOn(Restore, BuildShaders, CheckSystem)
        .Executes(() =>
        {
            DotNetBuild(s => DefaultDotNetBuild);
        });
}