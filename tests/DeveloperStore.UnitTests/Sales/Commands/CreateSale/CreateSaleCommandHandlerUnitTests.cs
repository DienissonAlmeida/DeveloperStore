using DeveloperStore.Application.Sales.Commands.CreateSale;
using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace DeveloperStore.UnitTests.Sales.Commands.CreateSale;

public class CreateSaleCommandHandlerUnitTests
{
    private readonly Mock<ISaleRepository> _repositoryMock;
    private readonly CreateSaleCommandHandler _handler;

    public CreateSaleCommandHandlerUnitTests()
    {
        _repositoryMock = new Mock<ISaleRepository>();
        _handler = new CreateSaleCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesSaleAndPersistsCorrectly()
    {
        // Arrange
        // qty=2 (<4) → no discount → total = 2 * 100 = 200
        var command = BuildCommand(saleNumber: "SALE-001", quantity: 2);
        Sale? persisted = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
            .Callback<Sale, CancellationToken>((sale, _) => persisted = sale)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        persisted.Should().NotBeNull();
        persisted!.SaleNumber.Should().Be("SALE-001");
        persisted.Items.Should().HaveCount(1);
        persisted.Items[0].Discount.Should().Be(0m);
        persisted.TotalAmount.Should().Be(200m);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ItemWithQuantityEqualOrAboveFour_AppliesTenPercentDiscount()
    {
        // Arrange
        // qty=4 (>=4) → 10% discount → total = 4 * 100 * 0.9 = 360
        var command = BuildCommand(quantity: 4);
        Sale? persisted = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
            .Callback<Sale, CancellationToken>((sale, _) => persisted = sale)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        persisted!.Items[0].Discount.Should().Be(0.10m);
        persisted.TotalAmount.Should().Be(360m);
    }

    // ──────────────────────────────────────────────────────────────────────────

    private static CreateSaleCommand BuildCommand(string saleNumber = "SALE-001", int quantity = 2) =>
        new(
            SaleNumber: saleNumber,
            SaleDate: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CustomerId: Guid.NewGuid(),
            CustomerName: "John Doe",
            BranchId: Guid.NewGuid(),
            BranchName: "Main Branch",
            Items:
            [
                new CreateSaleItemDto(
                    ProductId: Guid.NewGuid(),
                    ProductName: "Widget A",
                    Quantity: quantity,
                    UnitPrice: 100m,
                    Discount: 0m)
            ]);
}
