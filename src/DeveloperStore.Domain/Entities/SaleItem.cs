using DeveloperStore.Domain.ValueObjects;

namespace DeveloperStore.Domain.Entities;

public class SaleItem : BaseEntity
{
    public Guid SaleId { get; private set; }
    public ExternalIdentity Product { get; private set; } = null!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Discount { get; private set; }
    public decimal TotalAmount => Quantity * UnitPrice * (1 - Discount);

    private SaleItem() { }

    public static SaleItem Create(
        Guid saleId,
        ExternalIdentity product,
        int quantity,
        decimal unitPrice,
        decimal discount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegative(unitPrice);
        ArgumentOutOfRangeException.ThrowIfNegative(discount);

        return new SaleItem
        {
            SaleId = saleId,
            Product = product,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Discount = discount
        };
    }

}
