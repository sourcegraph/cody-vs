#addin nuget:?package=Cake.Git&version=4.0.0
#addin nuget:?package=Cake.Pnpm&version=1.0.0
#tool nuget:?package=vswhere&version=3.1.7

var target = Argument("target", "Build");
var configuration = Argument("configuration", "Release");


var agentDir = Directory("./Cody.VisualStudio/Agent");
var codyDir = Directory("../cody-dist");
var codyAgentDir = MakeAbsolute(codyDir + Directory("agent"));
var codyAgentDistDir = codyAgentDir + Directory("dist");
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

var codyCommit = "e8712db1d145fb7797297601b46b84297367ff8b";

var marketplaceToken = "<HIDDEN>";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("BuildCodyAgent")
    .Does(() =>
{
    if(!DirectoryExists(codyDir) || !GitIsValidRepository(codyDir))
	{
		GitClone(codyRepo, codyDir, new GitCloneSettings{ BranchName = "dpc/web-content" });
	}
	
	GitCheckout(codyDir, "dpc/web-content");
	//GitCheckout(codyDir, "main");
	GitPull(codyDir, "cake", "cake@cake.com");
	
	GitCheckout(codyDir, codyCommit);
	
	CleanDirectory(codyAgentDistDir);
	
	Context.Environment.WorkingDirectory = codyAgentDir;
	PnpmInstall();
	//PnpmRun("build:agent");

	// get the dpc/webviews branch for cody and try to build it manually
	PnpmRun("build:webviews");

	Context.Environment.WorkingDirectory = solutionDir;
	
	CreateDirectory(agentDir);
	CopyFiles($"{codyAgentDistDir}/*.*", agentDir);
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