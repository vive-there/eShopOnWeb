using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using InfrastructureDto.Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.eShopWeb.Infrastructure.Services;
public class OrderItemsReserverSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrderItemsReserverSender> _logger;

    public OrderItemsReserverSender(IConfiguration configuration, ILogger<OrderItemsReserverSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendSalesMessageAsync(Invoice invoice)
    {
        
        var json = JsonSerializer.Serialize(invoice, options: new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        await using var client = new ServiceBusClient(_configuration.GetSection("ServiceBusConnectionString").Value);

        await using ServiceBusSender sender = client.CreateSender(_configuration.GetSection("QueueName").Value);
        try
        {
            var message = new ServiceBusMessage(json);
            _logger.LogInformation($"Sending message: {json}");
            await sender.SendMessageAsync(message);
        }
        catch (Exception exception)
        {
            _logger.LogError($"{DateTime.Now} :: Exception: {exception.Message}");
        }
        finally
        {
            // Calling DisposeAsync on client types is required to ensure that network
            // resources and other unmanaged objects are properly cleaned up.
            await sender.DisposeAsync();
            await client.DisposeAsync();
        }
    }

}
