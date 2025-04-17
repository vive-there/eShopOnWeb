using Azure.Identity;

using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Configuration.AddUserSecrets<Program>(optional: true);
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddAzureClients(opts => {
    var blobEndpoint = new Uri(builder.Configuration.GetValue<string>("SalesBlobEndpoint")??string.Empty);
    opts.AddBlobServiceClient(blobEndpoint, new DefaultAzureCredential());
});

builder.Build().Run();
