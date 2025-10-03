///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
   // Executed BEFORE the first task.
   Information("Running tasks...");
});

Teardown(ctx =>
{
   // Executed AFTER the last task.
   Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory($"./ZtmBus/bin/{configuration}");
    CleanDirectory($"./ZtmBus/obj/{configuration}");
    CleanDirectory($"./ZtmBus.Test/bin/{configuration}");
    CleanDirectory($"./ZtmBus.Test/obj/{configuration}");
    CleanDirectory("./publish");
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetRestore("./ZtmBus.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    DotNetBuild("./ZtmBus.sln", new DotNetBuildSettings
    {
        Configuration = configuration,
        NoRestore = true,
    });
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetTest("./ZtmBus.Test/ZtmBus.Test.csproj", new DotNetTestSettings
    {
        Configuration = configuration,
        NoRestore = true,
        NoBuild = true,
        Verbosity = DotNetVerbosity.Normal,
    });
});

Task("Clean-Publish-Folder")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
    if (DirectoryExists("./publish"))
    {
        DeleteDirectory("./publish", new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        });
    }
    CreateDirectory("./publish");
});

Task("Publish")
    .IsDependentOn("Clean-Publish-Folder")
    .Does(() =>
{
    var settings = new DotNetPublishSettings
    {
        Configuration = configuration,
        OutputDirectory = "./publish/",
        NoRestore = true,
        NoBuild = true,
    };

    DotNetPublish("./ZtmBus/ZtmBus.csproj", settings);
});

Task("Pack")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
    DotNetPack("./ZtmBus/ZtmBus.csproj", new DotNetPackSettings
    {
        Configuration = configuration,
        OutputDirectory = "./publish/",
        NoRestore = true,
        NoBuild = true,
    });
});

///////////////////////////////////////////////////////////////////////////////
// TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Publish");

Task("CI")
    .IsDependentOn("Pack");

RunTarget(target);