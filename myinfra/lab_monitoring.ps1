param(
    [string]$solutionPath
)

if (-not $solutionPath) {
    Write-Information "usage: .\dotnet_publish_web.ps1 -solutionPath ""absolute path to \eShopOnWeb"" -project ""./src/Web/Web.csproj"" -projectName ""Web"""
    Write-Error "project parameter is required"
    exit 1
}

Write-Host "Solution path: $solutionPath"

$suffix=Get-Random -Minimum 1000 -Maximum 99999
$rgName="rg-vivethere$suffix"

az group create --name $rgName --location westeurope

$deployment = az deployment group create `
--name deployment$suffix `
--template-file .\lab_monitoring.json `
-g $rgName `
--parameters suffix=$suffix servicePlanSKU=F1 `
--query "properties.outputs" `
--output json

Write-Information $deployment

# # Parse the output
$deploymentOutput = $deployment | ConvertFrom-Json

Write-Information $deploymentOutput

$webappName = $deploymentOutput.webappName.value

$appZipArray = .\dotnet_publish_web.ps1 -solutionPath $solutionPath -project "./src/PublicApi/PublicApi.csproj" -projectName "PublicApi"
if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet_publish_web.ps1 failed for PublicApi"
    exit $LASTEXITCODE
}

$appZip = $appZipArray[-1]  # Get the latest element
az webapp deploy --resource-group $rgName --name $webappName --src-path $appZip --type zip
