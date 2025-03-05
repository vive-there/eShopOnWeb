# Azure infrastructure

## Simple schema

![Simple Schema](./my-infra.png)

## App Service eShopOnWeb Web

Web 1 - West Europe, F1 , autoscaling, 2 slots, deployment local git

Web 2 - North Europe, S1, deployment local git

PublicApp - West Europe, F1, deployment local git

## Azure Trafic Manager

Priority, Main endpoint - Web 1, Failover Web 2

## DatabaseInMemory

"UseOnlyInMemoryDatabase": true

## Azure CLI

```console
az deployment sub create --name depl00001 --template-file main.bicep --location westeurope --parameters aspSku=F1
```


https://blog.dotnetstudio.nl/posts/2021/04/merge-appsettings-with-bicep/

https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/bicep-functions-resource