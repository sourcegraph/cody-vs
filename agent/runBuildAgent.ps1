$version = Get-Content -Path "$PSScriptRoot\agent.version"
& "$PSScriptRoot\buildAgent.ps1" -version $version -agentDir $PSScriptRoot -outputDir "$PSScriptRoot\..\src\Cody.VisualStudio\Agent"