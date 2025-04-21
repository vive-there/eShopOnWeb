using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using InfrastructureDto.Dto;

using Microsoft.Extensions.Logging;

namespace Microsoft.eShopWeb.Infrastructure.Services;
public class DeliverySender(HttpClient httpClient, ILogger<OrderItemsReserver> logger)
{
    public async Task SendInvoiceAsync(Invoice invoice)
    {
        _ = invoice ?? throw new ArgumentNullException(nameof(invoice));

        // sent to queue
        var json = JsonSerializer.Serialize(invoice, options: new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Post;
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError($"Failed to notify delivery service. Status code: {response.StatusCode}");
            throw new Exception("Failed to notify delivery service.");
        }
    }
}
