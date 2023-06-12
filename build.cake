#addin nuget:?package=Cake.Common&version=3.0.0&loaddependencies=true&include=tools/addins/**/Cake.Common.dll
#addin nuget:?package=Cake.NuGet&version=3.0.0&loaddependencies=true&include=tools/addins/**/Cake.NuGet.dll
#addin nuget:?package=Cake.GitHubReleases&version=1.0.10&loaddependencies=true&include=tools/addins/**/Cake.GitHubRelease.dll

#load BuildConfiguration.cake
#load token.cake;

#reference Cake.Common
#reference Cake.NuGet
#reference Cake.GitHubReleases

using Cake.Common.Tools.DotNet.Clean;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.Pack;
using Cake.Common.Tools.DotNet.NuGet.Push;

// build configuration
Setup<BuildConfiguration>(ctx => {
	Information("Running setup...");
	return 
		 BuildConfiguration
		.New()
		.SetTarget(Argument<String>("target", "deploy"))
		.SetConfiguration(Argument<String>("configuration", "Debug"))
        .SetArchitecture(Argument<String>("architecture", "x64"))
		.SetFramework(Argument<String>("framework", "netcoreapp7"));
    });

// -- clean --
Task("clean").Does<BuildConfiguration>(config => {
	Information("Cleaning solution...");
	CleanDirectory("release");
	DotNetClean("NtObjectManager/NtObjectManager.Core.csproj", new DotNetCleanSettings() {
		Configuration = config.Configuration,
		});
	DotNetClean("NtApiDotNet/NtApiDotNet.Core.csproj", new DotNetCleanSettings() {
		Configuration = config.Configuration
		});
	}).OnError(exception =>
	{
		Information("clean Task failed...");
		Error(exception);
		return;
	});

// -- build --
Task("build").IsDependentOn("clean").Does<BuildConfiguration>(config => {
	Information("Building solution...");
    DotNetBuild("nt-object-manager.sln", new DotNetBuildSettings() {
		Configuration = config.Configuration,
		Framework = config.Framework
		});
	}).OnError(exception =>
	{
		Information("build Task failed...");
		Error(exception);
		return;
	});

// -- pack --
Task("pack").IsDependentOn("build").Does<BuildConfiguration>(config => {
	Information("Packing NuGet package...");
	DotNetPack("nt-object-manager.sln", new DotNetPackSettings() {
		Configuration = config.Configuration,
		IncludeSource = false,
		IncludeSymbols = true,
		NoBuild = true,
		NoDependencies = false,
		NoRestore = false,
		OutputDirectory = "release"
		});
	}).OnError(exception =>
	{
		Information("pack Task failed...");
		Error(exception);
		return;
	});

// -- publish --
Task("publish").IsDependentOn("pack").Does<BuildConfiguration>(async (config) => {
	Information("Publishing NuGet package to github...");
	GitHubReleaseCreateAsync(new GitHubReleaseCreateSettings() (
        repositoryOwner: "vollsynthetik@gmail.com", 
        repositoryName: "nt-objects-manager", 
        tagName: "v1.1.34")
		{
        Name = $"v1.1.34",
        Body = "Description",
        Draft = false,
        Prerelease = false,
        TargetCommitish = "abc123",
        AccessToken = token,
        Overwrite = true
		});
	}).OnError(exception =>
	{
		Information("publish Task failed...");
		Error(exception);
		return;
	});

// execution
RunTarget(Argument<String>("target", "deploy"));

Teardown(ctx =>
{
	Information("Done.");
});
