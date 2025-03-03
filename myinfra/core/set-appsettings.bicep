@description('The name of the Azure App Service')
param appServiceName string

@description('Array of appsettings')
param appSettings array

resource appService 'Microsoft.Web/sites@2024-04-01' existing = {
  name: appServiceName
}

resource appServiceUpdate 'Microsoft.Web/sites/config@2024-04-01' = {
  parent: appService
  dependsOn: [
    appService
  ]  
  name: 'web'
  properties: {
    appSettings: appSettings
  }
}
