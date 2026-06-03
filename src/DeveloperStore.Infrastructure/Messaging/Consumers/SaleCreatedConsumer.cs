using DeveloperStore.Application.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace DeveloperStore.Infrastructure.Messaging.Consumers;

public class SaleCreatedConsumer : IConsumer<SaleCreated>
{
    private readonly ILogger<SaleCreatedConsumer> _logger;

    public SaleCreatedConsumer(ILogger<SaleCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<SaleCreated> context)
    {
        var e = context.Message;
        _logger.LogInformation(
            "SaleCreated — Id: {SaleId} | Number: {SaleNumber} | Customer: {CustomerName} | Branch: {BranchName} | Total: {TotalAmount:C}",
            e.SaleId, e.SaleNumber, e.CustomerName, e.BranchName, e.TotalAmount);

        return Task.CompletedTask;
    }
}
