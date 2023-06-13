#addin nuget:?package=Cake.Common&version=3.0.0&loaddependencies=true&include=tools/addins/**/Cake.Common.dll
#addin nuget:?package=Cake.NuGet&version=3.0.0&loaddependencies=true&include=tools/addins/**/Cake.NuGet.dll
#addin nuget:?package=Cake.FileHelpers&version=6.1.3&loaddependencies=true&include=tools/addins/**/lib/net7.0/Cake.FileHelpers.dll

#load BuildConfiguration.cake

#reference Cake.Common
#reference Cake.NuGet

using Cake.Common.Tools.DotNet.Clean;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.Pack;

using Cake.Common.Tools.NuGet.Push;

var version = "1.1.34";

// build configuration
Setup<BuildConfiguration>(ctx => {
	Information("Running setup...");
	var token = FileReadText("token.key");
	return 
		 BuildConfiguration
		.New()
		.SetTarget(Argument<String>("target", "build"))
		.SetConfiguration(Argument<String>("configuration", "Debug"))
        .SetArchitecture(Argument<String>("architecture", "x64"))
		.SetFramework(Argument<String>("framework", "netcoreapp7"))
		.SetToken(token);
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
	CopyFile("releasenotes.txt", "release/releasenotes.txt");
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
Task("publish").IsDependentOn("pack").Does<BuildConfiguration>(config => {
	Information("Publishing NuGet package to github...");

	var token = config.Token;
	NuGetPush("release/NtApiDotNet.Core." + version + ".nupkg", new NuGetPushSettings() {
		ApiKey = token,
		Source = "https://nuget.pkg.github.com/Darkstar-GmbH/index.json",
		Verbosity = NuGetVerbosity.Detailed
		});

	NuGetPush("release/NtObjectManager.Core.1.1.34.nupkg", new NuGetPushSettings() {
		ApiKey = token,
		Source = "https://nuget.pkg.github.com/Darkstar-GmbH/index.json",
		Verbosity = NuGetVerbosity.Detailed
		});

	}).OnError(exception =>
	{
		Information("publish Task failed...");
		Error(exception);
		return;
	});

// execution
var target = Argument<String>("target", "build");

RunTarget(target);

Teardown(ctx =>
{
	Information("Done.");
});
