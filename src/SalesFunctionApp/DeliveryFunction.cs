using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using InfrastructureDto.Dto;

namespace SalesFunctionApp;

public class DeliveryFunction
{
    [Function("SaveInvoice")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
        FunctionContext executionContext)
    {

        ILogger<DeliveryFunction> logger = executionContext.InstanceServices.GetRequiredService<ILogger<DeliveryFunction>>();
        logger.LogInformation("SaveInvoice HTTP trigger function processed a request.");

        if (req.Body == null)
        {
            logger.LogError("SaveInvoice: Bad request");

            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            badRequestResponse.Headers.Add("Content-Type", "application/json");
            await badRequestResponse.WriteStringAsync(JsonSerializer.Serialize(new { message = "SaveInvoice: Bad Request" }));

            return badRequestResponse;
        }
        try
        {

            var config = executionContext.InstanceServices.GetRequiredService<IConfiguration>();

            CosmosClient cosmosClient = executionContext.InstanceServices.GetRequiredService<CosmosClient>();
            var invoice = await JsonSerializer.DeserializeAsync<Invoice>(req.Body, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });

            _ = invoice ?? throw new InvalidOperationException("Could not parse invoice");

            var container = cosmosClient.GetContainer("Delivery", "Invoices");

            await container.CreateItemAsync(invoice, new PartitionKey(invoice.InvoiceId));

            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch (Exception e)
        {
            logger.LogError($"SaveInvoice: {e.ToString()}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync(JsonSerializer.Serialize(new { message = e.Message }));
            errorResponse.Headers.Add("Content-Type", "application/json");
            return errorResponse;
        }
    }
}
