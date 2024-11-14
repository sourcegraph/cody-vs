param(
	$vsix-directory,
	$feed-file,
	$source-path,
	$gallery-name
)

$downloadUrl = "https://github.com/madskristensen/PrivateGalleryCreator/releases/download/1.0.64/PrivateGalleryCreator.zip"
$creatorFolder = Join-Path $env:GITHUB_WORKSPACE "PrivateGalleryCreator"
$zipFile = Join-Path $creatorFolder "PrivateGalleryCreator.zip"
$exePath = Join-Path $creatorFolder "PrivateGalleryCreator.exe"

if (!(Test-Path -Path $exePath -PathType Leaf)) {
	md -Force $creatorFolder | Out-Null
	Invoke-WebRequest $downloadUrl -OutFile $zipFile
	Expand-Archive $zipFile -DestinationPath $creatorFolder
	Remove-Item $zipFile
}

$prm = '--input="$vsix-directory" --output="$feed-file" --source="$source-path" --name="$gallery-name" --terminate'

Start-Process -FilePath $exePath -ArgumentList $prm -Wait -NoNewWindow