using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Configuration.AddUserSecrets<Program>(optional: true);
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddAzureClients(opts => {
    var blobEndpoint = new Uri(builder.Configuration.GetValue<string>("SalesBlobEndpoint")??string.Empty);
    opts.AddBlobServiceClient(blobEndpoint, new DefaultAzureCredential());
});

builder.Services.AddSingleton(s =>
{
    //https://github.com/Azure/azure-cosmos-dotnet-v3/blob/master/Microsoft.Azure.Cosmos.Samples/Usage/AzureFunctions/Startup.cs
    // Register the CosmosClient as a Singleton
    var cosmosDbConnectionString = builder.Configuration.GetValue<string>("COSMOSDB_CONNECTION_STRING");
    if (!string.IsNullOrEmpty(cosmosDbConnectionString))
    {
        return new CosmosClientBuilder(cosmosDbConnectionString)
        .WithConnectionModeDirect()
        .WithConsistencyLevel(ConsistencyLevel.Session)
        .WithSerializerOptions(new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.Default,
            IgnoreNullValues = true,
            Indented = false,
        })
        .Build();
    }

    var cosmosDbEndpoint = builder.Configuration.GetValue<string>("COSMOSDB_ENDPOINT");
    if (string.IsNullOrEmpty(cosmosDbConnectionString))
    {
        throw new Exception("Cosmso DB endpoint is null or empty");
    }

    return new CosmosClientBuilder(cosmosDbEndpoint, new DefaultAzureCredential())
        .WithConnectionModeDirect()
        .WithConsistencyLevel(ConsistencyLevel.Session)
        .WithSerializerOptions(new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
            IgnoreNullValues = true,
            Indented = false,
        })
        .Build();
});

builder.Build().Run();
