param(
	$configuration = "Debug"
)

Write-Output "Building using $configuration configuration ..."

& msbuild Cody.VisualStudio.Completions/Cody.VisualStudio.Completions.csproj -t:Build -restore -verbosity:minimal -property:Configuration=$configuration

& msbuild Cody.sln -t:Build -restore -verbosity:minimal -property:Configuration=$configuration
