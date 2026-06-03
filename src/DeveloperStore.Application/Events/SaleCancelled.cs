namespace DeveloperStore.Application.Events;

public record SaleCancelled(
    Guid SaleId,
    string SaleNumber,
    DateTime CancelledAt);
