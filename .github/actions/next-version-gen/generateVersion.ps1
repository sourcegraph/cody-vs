param(
	$publishType,
    $previewInfix
)

if($publishType -ne "Preview" -and $publishType -ne "Release") {
    Write-Host "::error::Publish type can only be 'Preview' or 'Release'"
    exit 1
}

$isPreview = $publishType -eq "Preview"

$pattern = "(?<Product>\w+)-v(?<Major>\d+)\.(?<Minor>\d+)\.(?<Patch>\w+)"

if ($env:GITHUB_REF_NAME -match $pattern) {
    $product = $Matches['Product']
    $majorVer = $Matches['Major']
    $minorVer = $Matches['Minor']
    $patchVer = $Matches['Patch']
}
else {
	Write-Host "::error::Invalid branch name. Example valid name: vs-v0.2.x"
    exit 1
}

if($patchVer -ne "x") {
	Write-Host "::error::Path version number must be set to 'x'"
    exit 1
}

if($minorVer % 2 -eq 1) {
	Write-Host "::error::Minor version number must be even"
    exit 1
}

if($isPreview) {
    if($minorVer -eq 0) {
        $newMinorVer = 999
        $majorVer = $majorVer - 1
    } else {
        $newMinorVer = $minorVer - 1
    }

	$infix = $previewInfix
} else {
	$newMinorVer = $minorVer
	$infix = ""
}

$nextPathVer = 0

do {
	$nextVersion = "$majorVer.$newMinorVer.$nextPathVer"
	$nextVersionTag = "$product$infix-v$nextVersion"
	git -C $env:GITHUB_WORKSPACE rev-parse "refs/tags/$nextVersionTag" --quiet 2> Out-Null
	if($?) { $nextPathVer = $nextPathVer + 1 }
	else { break }
} while($true)

"next-version=$nextVersion" >> $env:GITHUB_OUTPUT
"next-version-tag=$nextVersionTag" >> $env:GITHUB_OUTPUT

Write-Host "Next version: $nextVersion"
Write-Host "Next version tag name: $nextVersionTag"

exit 0