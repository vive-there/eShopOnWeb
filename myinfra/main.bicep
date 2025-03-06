targetScope = 'subscription'

param epoch string = '${dateTimeToEpoch(dateTimeAdd(utcNow(), 'P1Y'))}'
@allowed([
  'F1'
  'B1'
  'S1'
  'P0V3'
])
param aspSku string

var suffix = uniqueString(subscription().id, epoch)
@description('The name of resource group to deploy Web App and API')
var rgWebAndApiName = 'rg-web-and-api-${suffix}'
@description('The name of resource group to deploy Web App')
var rgWebName = 'rg-web-${suffix}'

var publicApiServiceName = 'api-vivethere-${suffix}'
var webAppFailoverServiceName = 'webapp-failover-vivethere-${suffix}'
var webAppMainServiceName = 'webapp-vivethere-${suffix}'

module rgWebAndApi './core/create-rg.bicep' = {
  name: rgWebAndApiName
  params: {
    location: 'westeurope'
    resourceGroupName: rgWebAndApiName
  }
}

module rgWeb1 './core/create-rg.bicep' = {
  name: rgWebName
  params: {
    location: 'northeurope'
    resourceGroupName: rgWebName
  }
}


// Create Public API App Service Plan
module aspApi './core/create-asp-windows.bicep' = {
  name: 'asp-api-vivethere-${suffix}'
  scope: resourceGroup(rgWebAndApiName)
  params: {
    aspName: 'asp-api-vivethere-${suffix}'
    skuName: aspSku
  }
  dependsOn:[
    rgWebAndApi
  ]
}


module publicApiService './core/create-appservice.bicep' = {
  name: 'api-vivethere-${suffix}'
  scope: resourceGroup(rgWebAndApiName)
  params: {
    appServiceName: publicApiServiceName
    aspPlanName: aspApi.outputs.asp.name
  }
  dependsOn:[
    rgWebAndApi
  ]  
}

// Create WebApp Failover ASP
module aspWebAppFailoverAsp './core/create-asp-windows.bicep' = {
  name: 'asp-webapp-failover-${suffix}'
  scope: resourceGroup(rgWebAndApiName)
  params: {
    aspName: 'asp-webapp-failover-${suffix}'
    skuName: aspSku
  }
  dependsOn:[
    rgWebAndApi
  ]  
}

module webAppFailoverService './core/create-appservice.bicep' = {
  name: 'webapp-failover-${suffix}'
  scope: resourceGroup(rgWebAndApiName)
  params: {
    appServiceName: webAppFailoverServiceName
    aspPlanName: aspWebAppFailoverAsp.outputs.asp.name
  }
  dependsOn:[
    rgWebAndApi
  ]  
}

// Create WebApp Failover ASP
module aspWebAppMainAsp './core/create-asp-windows.bicep' = {
  name: 'asp-webapp-main-${suffix}'
  scope: resourceGroup(rgWebName)
  params: {
    aspName: 'asp-webapp-main-${suffix}'
    skuName: aspSku
  }
  dependsOn:[
    rgWeb1
  ]  
}

module webAppMainService './core/create-appservice.bicep' = {
  name: 'webapp-main-${suffix}'
  scope: resourceGroup(rgWebName)
  params: {
    appServiceName: webAppMainServiceName
    aspPlanName: aspWebAppMainAsp.outputs.asp.name
  }
  dependsOn:[
    rgWeb1
  ]  
}


module apiAppsettings './core/set-appsettings.bicep' = {
  scope: resourceGroup(rgWebAndApiName)
  name: 'apiAppsettingDeploy${suffix}'
  params: {
    appServiceName: publicApiServiceName
    currentAppSettings: publicApiService.outputs.app.currentAppSettings
    appSettings: {
      PROJECT: 'src/PublicApi/PublicApi.csproj'
      SCM_DO_BUILD_DURING_DEPLOYMENT: 'true'      
      baseUrls__apiBase: 'https://${publicApiService.outputs.app.defaultHostName}/'
      baseUrls__webBase: 'https://${webAppFailoverService.outputs.app.defaultHostName}/'
    }
  }
  dependsOn:[
    rgWebAndApi
  ]   
}


module webAppFailoverServiceAppsettings './core/set-appsettings.bicep' = {
  scope: resourceGroup(rgWebAndApiName)
  name: 'webAppFailoverServiceAppsettingsDeploy${suffix}'
  params: {
    appServiceName: webAppFailoverServiceName
    currentAppSettings: webAppFailoverService.outputs.app.currentAppSettings
    appSettings: {
      PROJECT: 'src/Web/Web.csproj'
      SCM_DO_BUILD_DURING_DEPLOYMENT: 'true'      
      baseUrls__apiBase: 'https://${publicApiService.outputs.app.defaultHostName}/'
      baseUrls__webBase: 'https://${webAppFailoverService.outputs.app.defaultHostName}/'
    }
  }
}


module webAppMainServiceAppsettings './core/set-appsettings.bicep' = {
  scope: resourceGroup(rgWebName)
  name: 'webAppMainServiceAppsettingsDeploy${suffix}'
  params: {
    appServiceName: webAppMainServiceName
    currentAppSettings: webAppMainService.outputs.app.currentAppSettings
    appSettings: {
      PROJECT: 'src/Web/Web.csproj'
      SCM_DO_BUILD_DURING_DEPLOYMENT: 'true'      
      baseUrls__apiBase: 'https://${publicApiService.outputs.app.defaultHostName}/'
      baseUrls__webBase: 'https://${webAppMainService.outputs.app.defaultHostName}/'
    }
  }
}


// module webApiDeployment './core/appservice-externalgit-manualintegration-deployment.bicep' = {
//   name: 'api-depl-${suffix}'
//   scope: resourceGroup(rgWebAndApiName)
//   params:{
//     appServiceName: publicApiService.outputs.app.name
//     repoURL: 'https://github.com/vive-there/eShopOnWeb.git'
//     branch: 'main'
//   }
//   dependsOn:[
//     apiAppsettings
//   ]  
// }

// module webAppFailoverDeployment './core/appservice-externalgit-manualintegration-deployment.bicep' = {
//   name: 'webappfailover-depl-${suffix}'
//   scope: resourceGroup(rgWebAndApiName)
//   params:{
//     appServiceName: webAppFailoverService.outputs.app.name
//     repoURL: 'https://github.com/vive-there/eShopOnWeb.git'
//     branch: 'main'
//   }
//   dependsOn:[
//     webAppFailoverServiceAppsettings
//   ]  
// }

