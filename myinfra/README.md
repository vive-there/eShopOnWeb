# Azure infrastructure

## Simple schema

![Simple Schema](./my-infra.png)

## App Service eShopOnWeb Web

Web 1 - West Europe, S1 , autoscaling, 2 slots, deployment local git

Web 2 - North Europe, F1, deployment local git

PublicApp - West Europe, F1, deployment local git

## Azure Trafic Manager

Priority, Main endpoint - Web 1, Failover Web 2

## DatabaseInMemory

"UseOnlyInMemoryDatabase": true

## Azure CLI

```console
az deployment sub create --name depl00001 --template-file main.bicep
```

