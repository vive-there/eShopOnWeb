param(
    [string]$project,
    [string]$projectName
)

if (-not $project) {
    Write-Information "usage: .\dotnet_publish_web.ps1 -project ""./src/Web/Web.csproj"" -projectName ""Web"""
    Write-Error "project parameter is required"
    exit 1
}

if (-not $projectName) {
    Write-Information "usage: .\dotnet_publish_web.ps1 -project ""./src/Web/Web.csproj"" -projectName ""Web"""
    Write-Error "projectName parameter is required"
    exit 1
}


$solution="C:\Users\vadym\source\repos\eShopOnWeb"
$projectPath = Join-Path -Path $solution -ChildPath $project

$publishFolder="$solution\src\$projectName\bin\publish"
$suffix = (Get-Date).ToString("yyyyMMddHHmmssffff")
$zipFileLocation = "$publishFolder\$projectName-$suffix.zip"

Write-Information "Publishing project $project to $publishFolder"
if (Test-Path $publishFolder) {
    Write-Information "Removing existing publish folder $publishFolder"
    Remove-Item -Recurse -Force -Path $publishFolder
}

dotnet publish $projectPath -o $publishFolder -c Release
Compress-Archive -Path "$publishFolder\*" -DestinationPath $zipFileLocation -Force

Write-Output $zipFileLocation
