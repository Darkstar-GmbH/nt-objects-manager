#addin nuget:?package=Cake.Common&version=3.0.0&loaddependencies=true&include=tools/addins/**/Cake.Common.dll
#addin nuget:?package=Cake.NuGet&version=3.0.0&loaddependencies=true&include=tools/addins/**/Cake.NuGet.dll

#load BuildConfiguration.cake

#reference Cake.Common

using Cake.Common.Tools.DotNet.Clean;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.MSBuild;

// build configuration
Setup<BuildConfiguration>(ctx => {
	Information("Running setup...");
	return 
		 BuildConfiguration
		.New()
		.SetTarget(Argument<String>("target", "deploy"))
		.SetConfiguration(Argument<String>("configuration", "Debug"))
        .SetArchitecture(Argument<String>("architecture", "x64"))
		.SetFrameworks(new[] {"netcoreapp7"});
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
		Framework = config.Frameworks[0]
		});
	}).OnError(exception =>
	{
		Information("build Task failed...");
		Error(exception);
		return;
	});

// -- deploy --
Task("deploy").IsDependentOn("build").Does(() => {
	Information("Deploying...");
	}).OnError(exception =>
	{
		Information("deploy Task failed...");
		Error(exception);
		return;
	});

// -- publish --
Task("publish").IsDependentOn("deploy").Does(() => {
	Information("Publishing...");
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
