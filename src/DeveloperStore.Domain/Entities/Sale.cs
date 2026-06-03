using DeveloperStore.Domain.ValueObjects;

namespace DeveloperStore.Domain.Entities;

public class Sale : BaseEntity
{
    private readonly List<SaleItem> _items = [];

    public string SaleNumber { get; private set; } = null!;
    public DateTime SaleDate { get; private set; }
    public ExternalIdentity Customer { get; private set; } = null!;
    public ExternalIdentity Branch { get; private set; } = null!;
    public bool IsCancelled { get; private set; }

    public IReadOnlyList<SaleItem> Items => _items.AsReadOnly();
    public decimal TotalAmount => _items.Sum(i => i.TotalAmount);

    private Sale() { }

    public static Sale Create(
        string saleNumber,
        DateTime saleDate,
        ExternalIdentity customer,
        ExternalIdentity branch)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(saleNumber);

        return new Sale
        {
            SaleNumber = saleNumber,
            SaleDate = saleDate,
            Customer = customer,
            Branch = branch
        };
    }

    public void AddItem(ExternalIdentity product, int quantity, decimal unitPrice, decimal discount)
    {
        var item = SaleItem.Create(Id, product, quantity, unitPrice, discount);
        _items.Add(item);
        SetUpdatedAt();
    }

    public void Cancel()
    {
        IsCancelled = true;
        SetUpdatedAt();
    }
}
