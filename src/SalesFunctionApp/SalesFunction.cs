using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

using Azure;
using Azure.Storage.Blobs;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SalesFunctionApp;

public class SalesFunction
{
    [Function("SalesFunction")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            FunctionContext executionContext)
    {
        ILogger<SalesFunction> logger = executionContext.InstanceServices.GetRequiredService<ILogger<SalesFunction>>();
        logger.LogInformation("SalesFunction HTTP trigger function processed a request.");

        if (req.Body == null)
        {
            logger.LogError("SalesFunction: Bad request");

            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            badRequestResponse.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            await badRequestResponse.WriteStringAsync("HttpExample: Bad request");

            return badRequestResponse;
        }
        try
        {
            var config = executionContext.InstanceServices.GetRequiredService<IConfiguration>();
            var blobServiceClient = executionContext.InstanceServices.GetRequiredService<BlobServiceClient>();
            var containerClient = blobServiceClient.GetBlobContainerClient(config.GetValue<string>("SalesBlobContainer"));
            await containerClient.CreateIfNotExistsAsync();


            // Read the request body into dynamic object
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var json = JsonObject.Parse(requestBody);
            
            var invoiceData = json["invoice"];
            await containerClient.UploadBlobAsync(
                $"invoice-{invoiceData["id"]}_{Guid.NewGuid().ToString("n")}.json",
                new BinaryData(Encoding.UTF8.GetBytes(invoiceData.ToJsonString()))
            );

            return req.CreateResponse(HttpStatusCode.OK);
            
        }
        catch(Exception e)
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync(e.Message);
            errorResponse.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            return errorResponse;
        }
    }
}
