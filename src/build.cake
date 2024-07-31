#addin nuget:?package=Cake.Git&version=4.0.0
#addin nuget:?package=Cake.Pnpm&version=1.0.0
#tool nuget:?package=vswhere&version=3.1.7

var target = Argument("target", "Build");
var configuration = Argument("configuration", "Release");


var agentDir = Directory("./Cody.VisualStudio/Agent");
var codyDevDir = Directory("../../cody");
var codyDir = Directory("../cody-dist");
var nodeBinariesDir = Directory("../node-binaries");
var nodeExeFile = nodeBinariesDir + File("node-win-x64.exe");
var nodeArmExeFile = nodeBinariesDir + File("node-win-arm64.exe");
var solutionDir = MakeAbsolute(Context.Environment.WorkingDirectory);
var buildDir = solutionDir + Directory($"Cody.VisualStudio/bin/{configuration}");
var buildExtensionFile = buildDir + File("Cody.VisualStudio.vsix");
var publishManifestFile = buildDir + File("Marketplace/manifest.json");

var vsixPublisherFile = VSWhereLatest() + File("/VSSDK/VisualStudioIntegration/Tools/Bin/VsixPublisher.exe");


var codyRepo = "https://github.com/sourcegraph/cody.git";
var nodeBinaryUrl = "https://github.com/sourcegraph/node-binaries/raw/main/v20.12.2/node-win-x64.exe";
var nodeArmBinaryUrl = "https://github.com/sourcegraph/node-binaries/raw/main/v20.12.2/node-win-arm64.exe";

var codyCommit = "c41fec5aa3d4270e1a994b7bb17bfaffa4696997";

var marketplaceToken = "<HIDDEN>";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("BuildCodyAgent")
    .Does(() =>
{
	// Check for the env var to see if we should use the local cody directory.
	// This is used to build the agent from the local cody directory instead of cloning from github.
	var isDevMode = EnvironmentVariable("CODY_VS_DEV_MODE") == "true";
	if (isDevMode && DirectoryExists(codyDevDir))
	{
		codyDir = codyDevDir;
	}

	
	var codyAgentDir = MakeAbsolute(codyDir + Directory("agent"));
	var codyAgentDistDir = codyAgentDir + Directory("dist");

    if(!DirectoryExists(codyDir) || !GitIsValidRepository(codyDir))
	{
		// GitCheckout(codyDir, codyCommit);
		
		var branchName = "dpc/web-content"
		
		GitClone(codyRepo, codyDir, new GitCloneSettings{ BranchName = branchName });
		
		GitCheckout(codyDir, branchName);

		GitPull(codyDir, "cake", "cake@cake.com");
		
		CleanDirectory(codyAgentDistDir);
	}
	
	Context.Environment.WorkingDirectory = codyAgentDir;

	PnpmInstall();
	PnpmRun("build:agent");
	PnpmRun("build:webviews");

	Context.Environment.WorkingDirectory = solutionDir;
	
	CreateDirectory(agentDir);
	CopyDirectory(codyAgentDistDir, agentDir);
});

Task("DownloadNode")
    .Does(() =>
{
	if(!FileExists(nodeExeFile))
	{
		CreateDirectory(nodeBinariesDir);
		DownloadFile(nodeBinaryUrl, nodeExeFile);
	}
	
	if(!FileExists(nodeArmExeFile))
	{
		CreateDirectory(nodeBinariesDir);
		DownloadFile(nodeArmBinaryUrl, nodeArmExeFile);
	}
	
	CopyFileToDirectory(nodeExeFile, agentDir);
	CopyFileToDirectory(nodeArmExeFile, agentDir);
});

Task("Build")
    .IsDependentOn("BuildCodyAgent")
	.IsDependentOn("DownloadNode")
    .Does(() =>
{
    MSBuild("./Cody.sln", new MSBuildSettings { 
		Configuration = configuration,
		PlatformTarget = PlatformTarget.MSIL
	});
});

Task("Publish")
    //.IsDependentOn("Build")
    .Does(() =>
{
	var args = new ProcessSettings().WithArguments(x => x
					.Append("publish")
					.AppendSwitchQuoted("-payload", buildExtensionFile)
					.AppendSwitchQuoted("-publishManifest", publishManifestFile)
					.AppendSwitchQuoted("-personalAccessToken", marketplaceToken)
				);
    
	var returnCode = StartProcess(vsixPublisherFile, args);
	if(returnCode != 0) throw new Exception("Publishing error");
	
	
	//StartProcess(vsixPublisherFile, $"publish -payload \"{buildExtensionFile}\" -publishManifest \"{publishManifestFile}\" -personalAccessToken \"{marketplaceToken}\"");
    
});

Task("Clean")
    //.WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
{
    //todo
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    //todo
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);