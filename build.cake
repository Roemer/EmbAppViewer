//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var artifactsDir = new DirectoryPath("./artifacts");
var outputDir = new DirectoryPath($"./src/EmbAppViewer/bin/{configuration}");
var slnPath = "./src/EmbAppViewer.sln";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDir);
    CleanDirectory(outputDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetRestore(slnPath);
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    DotNetBuild(slnPath, new DotNetBuildSettings
    {
        Configuration = configuration,
        Verbosity = DotNetVerbosity.Minimal,
    });
});

Task("Package")
    .IsDependentOn("Build")
    .Does(() =>
{
    Zip(outputDir, artifactsDir.CombineWithFilePath("EmbAppViewer.zip"), new[] {
        outputDir.CombineWithFilePath("EmbAppViewer.exe"),
        outputDir.CombineWithFilePath("YamlDotNet.dll"),
        outputDir.CombineWithFilePath("config.yaml")
    });
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
