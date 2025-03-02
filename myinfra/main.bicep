targetScope = 'subscription'

var suffix = uniqueString(subscription().id)
@description('The name of resource group to deploy Web App and API')
var rgWebAndApiName = 'rg-web-and-api-${suffix}'
@description('The name of resource group to deploy Web App')
var rgWebName = 'rg-web-${suffix}'


module rgWebAndApi './core/create-rg.bicep' = {
  name: rgWebAndApiName
  params: {
    location: 'westeurope'
    resourceGroupName: rgWebAndApiName
  }
}

module rgWeb './core/create-rg.bicep' = {
  name: rgWebName
  params: {
    location: 'northeurope'
    resourceGroupName: rgWebName
  }
}


// Create App Service Plan
module aspFree './core/create-asp-windows.bicep' = {
  name: 'asp-free-${suffix}'
  scope: resourceGroup(rgWeb.name)
  params: {
    aspName: 'asp-free-${suffix}'
    skuName: 'F1'
  }
}


module webAppFree './core/create-appservice.bicep' = {
  name: 'webapp-free-${suffix}'
  scope: resourceGroup(rgWeb.name)
  params: {
    appServiceName: 'webapp-free-${suffix}'
    aspPlanName: aspFree.outputs.asp.name
  }
}

module webApiDeployment './core/appservice-deployment.bicep' = {
  name: 'webapp-depl-${suffix}'
  scope: resourceGroup(rgWeb.name)
  params:{
    appServiceName: webAppFree.outputs.app.name
    repoURL: 'https://github.com/vive-there/eShopOnWeb.git'
    branch: 'main'
    projectName: 'src/PublicApi/PublicApi.csproj'
  }
}


