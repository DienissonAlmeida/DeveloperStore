using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.ValueObjects;

namespace DeveloperStore.UnitTests.Common;

internal static class SaleBuilder
{
    public static Sale Build(
        string saleNumber = "SALE-001",
        DateTime? saleDate = null,
        Guid? customerId = null,
        string customerName = "John Doe",
        Guid? branchId = null,
        string branchName = "Main Branch")
    {
        var sale = Sale.Create(
            saleNumber,
            saleDate ?? new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new ExternalIdentity(customerId ?? Guid.NewGuid(), customerName),
            new ExternalIdentity(branchId ?? Guid.NewGuid(), branchName));

        sale.AddItem(
            new ExternalIdentity(Guid.NewGuid(), "Widget A"),
            quantity: 2,
            unitPrice: 100m,
            discount: 0.10m);

        return sale;
    }
}
