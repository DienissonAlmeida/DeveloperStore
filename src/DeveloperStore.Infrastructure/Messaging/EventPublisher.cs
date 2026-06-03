using DeveloperStore.Application.Common.Interfaces;
using MassTransit;

namespace DeveloperStore.Infrastructure.Messaging;

public class EventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private const int MaxRetries = 3;

    public EventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        var delay = TimeSpan.FromSeconds(1);

        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                await _publishEndpoint.Publish(message, cancellationToken);
                return;
            }
            catch (Exception) when (attempt < MaxRetries)
            {
                await Task.Delay(delay, cancellationToken);
                delay *= 2; // exponential backoff: 1s → 2s → 4s
            }
        }
    }
}
