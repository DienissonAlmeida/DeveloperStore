using DeveloperStore.Application.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace DeveloperStore.Infrastructure.Messaging.Consumers;

public class SaleModifiedConsumer : IConsumer<SaleModified>
{
    private readonly ILogger<SaleModifiedConsumer> _logger;

    public SaleModifiedConsumer(ILogger<SaleModifiedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<SaleModified> context)
    {
        var e = context.Message;
        _logger.LogInformation(
            "SaleModified — Id: {SaleId} | Number: {SaleNumber} | Customer: {CustomerName} | Branch: {BranchName} | Total: {TotalAmount:C}",
            e.SaleId, e.SaleNumber, e.CustomerName, e.BranchName, e.TotalAmount);

        return Task.CompletedTask;
    }
}
