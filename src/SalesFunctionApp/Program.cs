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
    opts.AddBlobServiceClient(builder.Configuration.GetValue<string>("SalesStorageConnectionString"));
});

builder.Build().Run();
