#addin nuget:?package=Cake.Git&version=4.0.0
#addin nuget:?package=Cake.Pnpm&version=1.0.0
#tool nuget:?package=vswhere&version=3.1.7

var target = Argument("target", "Build");
var configuration = Argument("configuration", "Release");


var agentDir = Directory("./Sourcegraph.Cody/Agent");
var codyDir = Directory("../cody-dist");
var codyAgentDir = MakeAbsolute(codyDir + Directory("agent"));
var codyAgentDistDir = codyAgentDir + Directory("dist");
var nodeBinariesDir = Directory("../node-binaries");
var nodeExeFile = nodeBinariesDir + File("node-win-x64.exe");
var nodeArmExeFile = nodeBinariesDir + File("node-win-arm64.exe");
var solutionDir = MakeAbsolute(Context.Environment.WorkingDirectory);
var buildDir = solutionDir + Directory($"Sourcegraph.Cody/bin/{configuration}");
var buildExtensionFile = buildDir + File("Sourcegraph.Cody.vsix");
var publishManifestFile = buildDir + File("marketplace-manifest.json");

var vsixPublisherFile = VSWhereLatest() + File("/VSSDK/VisualStudioIntegration/Tools/Bin/VsixPublisher.exe");


var codyRepo = "https://github.com/sourcegraph/cody.git";
var nodeBinaryUrl = "https://github.com/sourcegraph/node-binaries/raw/main/v20.12.2/node-win-x64.exe";
var nodeArmBinaryUrl = "https://github.com/sourcegraph/node-binaries/raw/main/v20.12.2/node-win-arm64.exe";

var codyCommit = "1a155d5432370b31a1b1cf7a1b78412f237f66b0";

var marketplaceToken = "<HIDDEN>";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("BuildCodyAgent")
    .Does(() =>
{
    if(!DirectoryExists(codyDir) || !GitIsValidRepository(codyDir))
	{
		GitClone(codyRepo, codyDir, new GitCloneSettings{ BranchName = "main" });
	}
	
	GitCheckout(codyDir, "main");
	GitPull(codyDir, "cake", "cake@cake.com");
	
	GitCheckout(codyDir, codyCommit);
	
	CleanDirectory(codyAgentDistDir);
	
	Context.Environment.WorkingDirectory = codyAgentDir;
	PnpmInstall();
	PnpmRun("build:agent");
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
    MSBuild("./Sourcegraph.Cody.sln", new MSBuildSettings { 
		Configuration = configuration,
		PlatformTarget = PlatformTarget.MSIL
	});
});

Task("Publish")
    .IsDependentOn("Build")
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