#addin nuget:?package=Cake.Git&version=4.0.0
#addin nuget:?package=Cake.Pnpm&version=1.0.0
#tool nuget:?package=vswhere&version=3.1.7

var target = Argument("target", "Build");
var configuration = Argument("configuration", "Release");


var agentDir = Directory("./Cody.VisualStudio/Agent");
var agentWebViewDir = agentDir + Directory("webviews");
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

var notIncludeFiles = new string[]
{
	"tree-sitter-bash.wasm",
	"tree-sitter-dart.wasm",
	"tree-sitter-elisp.wasm",
	"tree-sitter-elixir.wasm",
	"tree-sitter-elm.wasm",
	"tree-sitter-go.wasm",
	"tree-sitter-java.wasm",
	"tree-sitter-kotlin.wasm",
	"tree-sitter-lua.wasm",
	"tree-sitter-objc.wasm",
	"tree-sitter-ocaml.wasm",
	"tree-sitter-rescript.wasm",
	"tree-sitter-ruby.wasm",
	"tree-sitter-rust.wasm",
	"tree-sitter-scala.wasm",
	"tree-sitter-swift.wasm",
	"tree-sitter-php.wasm"
};


var codyRepo = "https://github.com/sourcegraph/cody.git";
var nodeBinaryUrl = "https://github.com/sourcegraph/node-binaries/raw/main/v20.12.2/node-win-x64.exe";
var nodeArmBinaryUrl = "https://github.com/sourcegraph/node-binaries/raw/main/v20.12.2/node-win-arm64.exe";

// The latest tag of the stable release from the cody repository https://github.com/sourcegraph/cody/tags
var codyStableReleaseTag = "vscode-v1.34.3";

var marketplaceToken = EnvironmentVariable("CODY_VS_MARKETPLACE_RELEASE_TOKEN");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("BuildCodyAgent")
	.Does(() =>
{

	// Check for the env var to see if we should use the local cody directory.
	// This is used to build the agent from the local cody directory instead of cloning from github.
	var isDevMode = EnvironmentVariable("CODY_VS_DEV_PORT") == "3113";
	if (isDevMode && DirectoryExists(codyDevDir))
	{
		Information($"--> Running in DevMode using:'{codyDir}'");
		codyDir = codyDevDir;
	}

	var codyAgentDir = MakeAbsolute(codyDir + Directory("agent"));
	var codyAgentDistDir = codyAgentDir + Directory("dist");
	var branchName = "main";

	if (!DirectoryExists(codyDir) || !GitIsValidRepository(codyDir))
	{
		Information($"--> Cloning repository:'{codyRepo}'");
		GitClone(codyRepo, codyDir, new GitCloneSettings { BranchName = branchName });
	}

	if (!isDevMode)
	{

		if (DirectoryExists(codyDir))
		{
			Information($"--> Switching to branch {branchName} ...");
			GitCheckout(codyDir, branchName);
		}

		Information($"--> git pull ...");
		GitPull(codyDir, "cake", "cake@cake.com", "", "", "origin");

		Information($"--> Checkout '{branchName}' ...");
		GitCheckout(codyDir, branchName);

        Information($"--> Fetching all tags...");
        GitFetch(codyDir, "origin");

        Information($"--> Checkout latest stable release using tag {codyStableReleaseTag} ...");
		GitCheckout(codyDir, codyStableReleaseTag);
	}

	Information($"--> Cleaning '{codyAgentDistDir}' ...");
	CleanDirectory(codyAgentDistDir);

	Context.Environment.WorkingDirectory = codyAgentDir;

    Information($"--> pnpm install ...");
    PnpmInstall();

	Information($"--> pnpm build ...");
	PnpmRun("build");

	Context.Environment.WorkingDirectory = solutionDir;

	var deleteSettings = new DeleteDirectorySettings
	{
		Recursive = true,
		Force = true
	};
	Information($"--> Cleaning '{agentDir}' ...");
	CleanDirectory(agentDir);

	Information($"--> Copying the agent to '{agentDir}'");
	CreateDirectory(agentDir);
	CopyDirectory(codyAgentDistDir, agentDir);

	foreach(var fileToRemove in notIncludeFiles)
		DeleteFile(agentDir + File(fileToRemove));

	var codyWebviewsFolder = MakeAbsolute(codyDir + Directory("agent/dist/webviews"));
	Information($"--> Cleaning '{agentWebViewDir}' ...");
	CreateDirectory(agentWebViewDir);

	Information($"--> Copying the webviews from '{codyWebviewsFolder}' to '{agentWebViewDir}' ...");
	CopyDirectory(codyWebviewsFolder, agentWebViewDir);


	// removing pnpm build:root artefacts (/src and /scripts folders)

	var srcFolder = @$"{agentDir}/src";
	var scriptsFolder = @$"{agentDir}/scripts";
	if (DirectoryExists(srcFolder))
		DeleteDirectory(srcFolder, deleteSettings);

	if (DirectoryExists(scriptsFolder))
		DeleteDirectory(scriptsFolder, deleteSettings);

});

Task("DownloadNode")
	.Does(() =>
{
	if (!FileExists(nodeExeFile))
	{
		CreateDirectory(nodeBinariesDir);
		DownloadFile(nodeBinaryUrl, nodeExeFile);
	}

	if (!FileExists(nodeArmExeFile))
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
	.IsDependentOn("Restore")
	.Does(() =>
{
	MSBuild("./Cody.sln", new MSBuildSettings
	{
		Configuration = configuration,
		PlatformTarget = PlatformTarget.MSIL,
		Verbosity = Verbosity.Minimal
	});
});

Task("Tests")
	//.IsDependentOn("Build")
	.Does(() =>
{
	MSBuild("./Cody.sln", new MSBuildSettings
	{
		Configuration = "Debug",
		PlatformTarget = PlatformTarget.MSIL,
		Verbosity = Verbosity.Minimal
	});

//	DotNetTest("./Cody.VisualStudio.Tests/bin/Debug/Cody.VisualStudio.Tests.dll", new DotNetTestSettings
//    {
//        NoBuild = true,
//        NoRestore = true
//    });
});

Task("Restore")
	.Does(() =>
{
	Information($"Restore ...");
	MSBuild("./Cody.sln", new MSBuildSettings {
		Targets = { "Restore" },
		Verbosity = Verbosity.Minimal
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
	if (returnCode != 0) throw new Exception("Publishing error");


	//StartProcess(vsixPublisherFile, $"publish -payload \"{buildExtensionFile}\" -publishManifest \"{publishManifestFile}\" -personalAccessToken \"{marketplaceToken}\"");

});

Task("Clean")
	//.WithCriteria(c => HasArgument("rebuild"))
	.Does(() =>
{
	//todo
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
