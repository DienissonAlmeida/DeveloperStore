using DeveloperStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperStore.Infrastructure.Persistence.Configurations;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.SaleId)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .IsRequired();

        builder.Property(i => i.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(i => i.Discount)
            .IsRequired()
            .HasPrecision(5, 4);

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.OwnsOne(i => i.Product, nav =>
        {
            nav.Property(e => e.Id).HasColumnName("ProductId").IsRequired();
            nav.Property(e => e.Name).HasColumnName("ProductName").HasMaxLength(200).IsRequired();
        });

        builder.Ignore(i => i.TotalAmount);
    }
}
