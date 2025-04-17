using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.eShopWeb.Infrastructure.Dto;
using Microsoft.Extensions.Logging;

namespace Microsoft.eShopWeb.Infrastructure.Services;
public class OrderItemsReserver
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrderItemsReserver> _logger;

    public OrderItemsReserver(HttpClient httpClient, ILogger<OrderItemsReserver> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task ReserveOrderAsync(Invoice invoice)
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

        var response = await _httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Failed to reserve order items. Status code: {response.StatusCode}");
            throw new Exception("Failed to reserve order items.");
        }
    }
}
