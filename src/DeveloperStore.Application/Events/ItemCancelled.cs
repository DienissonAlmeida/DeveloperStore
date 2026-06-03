namespace DeveloperStore.Application.Events;

public record ItemCancelled(
    Guid SaleId,
    Guid ItemId,
    Guid ProductId,
    string ProductName,
    DateTime CancelledAt);
