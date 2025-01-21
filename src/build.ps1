param(
	$configuration = "Debug"
)

$current = $PSScriptRoot
Write-Output "Building using $configuration configuration ..."

& msbuild $current/Cody.VisualStudio.Completions/Cody.VisualStudio.Completions.csproj -t:Build -restore -verbosity:minimal -property:Configuration=$configuration

& msbuild $current/Cody.sln -t:Build -restore -verbosity:minimal -property:Configuration=$configuration
