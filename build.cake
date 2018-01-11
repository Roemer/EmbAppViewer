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
    NuGetRestore(slnPath);
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    MSBuild(slnPath, new MSBuildSettings {
        Verbosity = Verbosity.Minimal,
        ToolVersion = MSBuildToolVersion.VS2017,
        Configuration = configuration
    }.AddFileLogger(new MSBuildFileLogger {
        LogFile = artifactsDir.CombineWithFilePath("BuildLog.txt"),
        Verbosity = Verbosity.Normal,
        MSBuildFileLoggerOutput = MSBuildFileLoggerOutput.All,
        ShowTimestamp = true
    }));
});

Task("Package")
    .IsDependentOn("Build")
    .Does(() =>
{
    Zip(outputDir, artifactsDir.CombineWithFilePath("EmbAppViewer.zip"), new[] { outputDir.CombineWithFilePath("EmbAppViewer.exe") });
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
