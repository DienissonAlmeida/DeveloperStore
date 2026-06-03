using DeveloperStore.Application.Common.Interfaces;
using MassTransit;

namespace DeveloperStore.Infrastructure.Messaging;

public class EventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public EventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
        => _publishEndpoint.Publish(message, cancellationToken);
}
