using DeveloperStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperStore.Infrastructure.Persistence.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SaleNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.SaleDate)
            .IsRequired();

        builder.Property(s => s.IsCancelled)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.OwnsOne(s => s.Customer, nav =>
        {
            nav.Property(e => e.Id).HasColumnName("CustomerId").IsRequired();
            nav.Property(e => e.Name).HasColumnName("CustomerName").HasMaxLength(200).IsRequired();
        });

        builder.OwnsOne(s => s.Branch, nav =>
        {
            nav.Property(e => e.Id).HasColumnName("BranchId").IsRequired();
            nav.Property(e => e.Name).HasColumnName("BranchName").HasMaxLength(200).IsRequired();
        });

        builder.HasMany(s => s.Items)
            .WithOne()
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
