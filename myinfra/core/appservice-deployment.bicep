@description('The name of the Azure App Service')
param appServiceName string

@description('The URL for the GitHub repository that contains the project to deploy.')
param repoURL string = 'https://github.com/vive-there/eShopOnWeb.git'
@description('The branch of the GitHub repository to use.')
param branch string = 'main'
@description('The name of the project to deploy.')
param projectName string

// Get existing app service
resource appService 'Microsoft.Web/sites@2024-04-01' existing = {
  name: appServiceName
}


resource appServiceDeployment 'Microsoft.Web/sites/sourcecontrols@2024-04-01' = {
  parent: appService
  name: 'web'
  dependsOn: [
    appService
  ]
  properties: {
    isManualIntegration: true
    branch: branch
    repoUrl: repoURL
  }
}

// update appsettings PROJECT with projectName
resource appServiceUpdate 'Microsoft.Web/sites/config@2024-04-01' = {
  parent: appService
  dependsOn: [
    appService
  ]  
  name: 'web'
  properties: {
    appSettings: [
      {
        name: 'PROJECT'
        value: projectName
      }
      {
        name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
        value: 'true'
      }
    ]
  }
}

