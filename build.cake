
var target = Argument("target", "Build");
var configuration = Argument("configuration", "Release");

Task("Clean")
    .WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
{
    EnsureDirectoryExists("build");
    CleanDirectory($"build");
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetPublish("Leditor.sln", new DotNetPublishSettings
    {
        SelfContained = true,
        OutputDirectory = "build",
        MSBuildSettings = new DotNetMSBuildSettings()
    });

    CreateDirectory("build/assets");
    CopyDirectory("Leditor/assets", "build/assets");
    CopyFile("imgui.ini", "build/imgui.ini");
    CopyFile("LICENSE", "build/LICENSE");
});

RunTarget(target);
