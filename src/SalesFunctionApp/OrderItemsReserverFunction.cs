using System;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SalesFunctionApp;

public class OrderItemsReserverFunction
{
    private readonly ILogger<OrderItemsReserverFunction> _logger;

    public OrderItemsReserverFunction(ILogger<OrderItemsReserverFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(OrderItemsReserverFunction))]
    public async Task Run(
        [ServiceBusTrigger("sbq-orderitemsreserver", Connection = "ServiceBusConnectionString")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        FunctionContext executionContext)
    {
        try
        {
            _logger.LogInformation("OnEntry: OrderItemsReserverFunction ServiceBus Queue trigger function processed a request.");

            _logger.LogInformation("Get blob client");
            var config = executionContext.InstanceServices.GetRequiredService<IConfiguration>();
            var blobServiceClient = executionContext.InstanceServices.GetRequiredService<BlobServiceClient>();
            var containerClient = blobServiceClient.GetBlobContainerClient(config.GetValue<string>("SalesBlobContainer"));

            _logger.LogInformation("Check a blob container");
            await containerClient.CreateIfNotExistsAsync();

            _logger.LogInformation("Parse message body");
            // Read the request body into dynamic object
            var requestBody = await new StreamReader(message.Body.ToStream()).ReadToEndAsync();
            var json = JsonObject.Parse(requestBody);

            if(json["id"]?.GetValue<string>() == "100")
            {
                throw new InvalidOperationException("Test exception");
            }

            _logger.LogInformation("Upload a blob");
            await containerClient.UploadBlobAsync(
                $"invoice-{json["id"]}_{Guid.NewGuid().ToString("n")}.json",
                new BinaryData(Encoding.UTF8.GetBytes(requestBody))
            );

            _logger.LogInformation("Complete the message");
            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
        catch(Exception e)
        {
            await messageActions.DeadLetterMessageAsync(message);

            _logger.LogError(e.Message, e);
        }

    }
}
