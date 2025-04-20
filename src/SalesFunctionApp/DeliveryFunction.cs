using Azure.Storage.Blobs;
using System.Net;
using System.Text.Json.Nodes;
using System.Text;
using System.Threading;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

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
            //var config = executionContext.InstanceServices.GetRequiredService<IConfiguration>();
            //var blobServiceClient = executionContext.InstanceServices.GetRequiredService<BlobServiceClient>();
            //var containerClient = blobServiceClient.GetBlobContainerClient(config.GetValue<string>("SalesBlobContainer"));
            //await containerClient.CreateIfNotExistsAsync();


            //// Read the request body into dynamic object
            //var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //var json = JsonObject.Parse(requestBody);

            //await containerClient.UploadBlobAsync(
            //    $"invoice-{json["id"]}_{Guid.NewGuid().ToString("n")}.json",
            //    new BinaryData(Encoding.UTF8.GetBytes(requestBody))
            //);

            return req.CreateResponse(HttpStatusCode.OK);

        }
        catch (Exception e)
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync(JsonSerializer.Serialize(new { message = e.Message }));
            errorResponse.Headers.Add("Content-Type", "application/json");
            return errorResponse;
        }

    }
}
