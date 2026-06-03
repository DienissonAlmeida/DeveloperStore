using DeveloperStore.Application.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace DeveloperStore.Infrastructure.Messaging.Consumers;

public class ItemCancelledConsumer : IConsumer<ItemCancelled>
{
    private readonly ILogger<ItemCancelledConsumer> _logger;

    public ItemCancelledConsumer(ILogger<ItemCancelledConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ItemCancelled> context)
    {
        var e = context.Message;
        _logger.LogInformation(
            "ItemCancelled — SaleId: {SaleId} | ItemId: {ItemId} | Product: {ProductName} | At: {CancelledAt}",
            e.SaleId, e.ItemId, e.ProductName, e.CancelledAt);

        return Task.CompletedTask;
    }
}
