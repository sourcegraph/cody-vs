#addin nuget:?package=Cake.Git&version=4.0.0
#addin nuget:?package=Cake.Pnpm&version=1.0.0


var agentDir = Directory("./Sourcegraph.Cody/Agent");
var codyDir = Directory("../cody-dist");
var codyAgentDir = MakeAbsolute(codyDir + Directory("agent"));
var codyAgentDistDir = codyAgentDir + Directory("dist");
var nodeBinariesDir = Directory("../node-binaries");
var nodeExeFile = nodeBinariesDir + File("node-win-x64.exe");
var solutionDir = MakeAbsolute(Context.Environment.WorkingDirectory);

var codyRepo = "https://github.com/sourcegraph/cody.git";
var nodeBinaryUrl = "https://github.com/sourcegraph/node-binaries/raw/main/v20.12.2/node-win-x64.exe";

var codyCommit = "1a155d5432370b31a1b1cf7a1b78412f237f66b0";

var target = Argument("target", "Build");
var configuration = Argument("configuration", "Release");

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
	
	CopyFileToDirectory(nodeExeFile, agentDir);
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