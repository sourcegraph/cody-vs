param(
	$agentDir = "$PSScriptRoot",
	$outputDir = "$PSScriptRoot\..\src\Cody.VisualStudio\Agent",
	$version = "main",
	[bool] $skipGit = $false
)

$codyDir = Join-Path $agentDir "\cody"
$nodeDir = Join-Path $agentDir "\node"
$versionFile = Join-Path $agentDir "\agent.version"

$codyAgentDir = Join-Path $codyDir "\agent"
$codyAgentDistDir = Join-Path $codyAgentDir "\dist"

$authPart = if ($Env:GITHUB_TOKEN) { "$Env:GITHUB_TOKEN@" } else { "" }
$codyRepo = "https://$authPart" + "github.com/sourcegraph/cody.git"

$nodeBinFile = Join-Path $nodeDir "node-win-x64.exe"
$nodeArmBinFile = Join-Path $nodeDir "node-win-arm64.exe"
$nodeUrl = "https://github.com/sourcegraph/node-binaries/raw/main/v20.12.2/node-win-x64.exe"
$nodeArmUrl = "https://github.com/sourcegraph/node-binaries/raw/main/v20.12.2/node-win-arm64.exe"

$exclude = @(
    "src",
    "scripts",
	"noxide.linux*.node",
	"noxide.darwin*.node",
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
)


if (!(Test-Path -Path $agentDir -PathType Container)) {
    New-Item -Path $agentDir -ItemType Directory
	Write-Host "Created $agentDir directory"
}

if (!(Test-Path -Path $codyDir -PathType Container)) {
    New-Item -Path $codyDir -ItemType Directory
	Write-Host "Created $codyDir directory"
	
}

if($skipGit -eq $false) {
	$gitDir = Join-Path $codyDir "\.git"
	if(!(Test-Path -Path $gitDir -PathType Container)) {
		Write-Host "Cloning repository: $codyRepo"
		git clone $codyRepo $codyDir  2>&1 | Write-Host
	}

	git -C $codyDir fetch
	git -C $codyDir checkout $version

	$isBranch = [string](git -C $codyDir show-ref --verify refs/heads/$version 2>&1)
	if($isBranch -notlike 'fatal:*') {
		git -C $codyDir pull
		Write-Host "Pull branch $version"
	}
}

Write-Host "Installing pnpm"
npm install -g pnpm@8.6.7  2>&1 | Write-Host


# Downloading Node executables
if (!(Test-Path -Path $nodeDir -PathType Container)) {
    New-Item -Path $nodeDir -ItemType Directory
	Write-Host "Created $nodeDir directory"
}

if (!(Test-Path -Path $nodeBinFile -PathType Leaf)) {
    Write-Host "Downloading $nodeUrl"
    Invoke-WebRequest -Uri $nodeUrl -OutFile $nodeBinFile
}

if (!(Test-Path -Path $nodeArmBinFile -PathType Leaf)) {
    Write-Host "Downloading $nodeArmUrl"
    Invoke-WebRequest -Uri $nodeArmUrl -OutFile $nodeArmBinFile
}

#Clear agent\dist
Write-Host "Clearing $codyAgentDistDir"
Get-ChildItem -Path $codyAgentDistDir -Recurse | Remove-Item -Recurse

#pnpm install and build
Push-Location -Path $codyAgentDir
Write-Host "pnpm install"
pnpm install  2>&1 | Write-Host
Write-Host "pnpm build"
pnpm build  2>&1 | Write-Host
Pop-Location

if (!(Test-Path -Path $outputDir -PathType Container)) {
    New-Item -Path $outputDir -ItemType Directory
	Write-Host "Created $outputDir directory"
}

# Clear out directory
Write-Host "Clearing $outputDir"
Get-ChildItem -Path $outputDir -Recurse | Remove-Item -Recurse

# Coping artifacts
Write-Host "Coping artifacts to $outputDir directory"
Copy-Item "$codyAgentDistDir\*" -Destination $outputDir -Recurse -Exclude $exclude
Copy-Item $nodeBinFile -Destination $outputDir
Copy-Item $nodeArmBinFile -Destination $outputDir
Copy-Item $versionFile -Destination $outputDir

