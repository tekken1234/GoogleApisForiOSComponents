
#tool nuget:?package=XamarinComponent&version=1.1.0.42

#addin nuget:?package=Cake.Xamarin.Build&version=1.0.14.0
#addin nuget:?package=Cake.Xamarin
#addin nuget:?package=Cake.XCode

BuildSpec buildSpec = null;

void InvokeOtherGoogleModules (string [] otherPaths, string target)
{
	if (otherPaths == null)
		return;

	var cakeSettings = new CakeSettings { 
			ToolPath = GetCakeToolPath (),
			Arguments = new Dictionary<string, string> { { "target", target } },
		};

	// Run the script from the subfolder
	foreach (var module in otherPaths)
		CakeExecuteScript ("../" + module + "/build.cake", cakeSettings);
}

string [] MyDependencies = null;

Task ("externals")
	.Does (() => 
{
	InvokeOtherGoogleModules (MyDependencies, "externals");
	RunMake ("./externals/", "all");
});

Task ("tmp-nuget").IsDependentOn ("libs").Does (() => 
{
	InvokeOtherGoogleModules (MyDependencies, "tmp-nuget");

	if (buildSpec.NuGets == null || buildSpec.NuGets.Length == 0)
		return;

	var newList = new List<NuGetInfo> ();

	foreach (var nuget in buildSpec.NuGets) {
		newList.Add (new NuGetInfo {
			BuildsOn = nuget.BuildsOn,
			NuSpec = nuget.NuSpec,
			RequireLicenseAcceptance = nuget.RequireLicenseAcceptance,
			Version = nuget.Version,
			OutputDirectory = "../tmp-nugets",
		});
	}

	PackNuGets (newList.ToArray ());
});

Task ("component").IsDependentOn ("nuget").IsDependentOn ("tmp-nuget").IsDependentOn ("component-base");

FilePath GetCakeToolPath ()
{
	var possibleExe = GetFiles ("../**/tools/Cake/Cake.exe").FirstOrDefault ();

	if (possibleExe != null)
		return possibleExe;
		
	var p = System.Diagnostics.Process.GetCurrentProcess ();  
	return new FilePath (p.Modules[0].FileName);
}



