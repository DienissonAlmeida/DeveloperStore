using DeveloperStore.Application.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace DeveloperStore.Infrastructure.Messaging.Consumers;

public class SaleCancelledConsumer : IConsumer<SaleCancelled>
{
    private readonly ILogger<SaleCancelledConsumer> _logger;

    public SaleCancelledConsumer(ILogger<SaleCancelledConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<SaleCancelled> context)
    {
        var e = context.Message;
        _logger.LogInformation(
            "SaleCancelled — Id: {SaleId} | Number: {SaleNumber} | At: {CancelledAt}",
            e.SaleId, e.SaleNumber, e.CancelledAt);

        return Task.CompletedTask;
    }
}
