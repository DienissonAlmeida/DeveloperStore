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
    public async Task Handle_ValidCommand_ReturnsNewSaleId()
    {
        // Arrange
        var command = BuildCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsAddAsyncOnce()
    {
        // Arrange
        var command = BuildCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsSaveChangesAsyncOnce()
    {
        // Arrange
        var command = BuildCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsSaleWithCorrectSaleNumber()
    {
        // Arrange
        var command = BuildCommand(saleNumber: "SALE-XYZ");
        Sale? persisted = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
            .Callback<Sale, CancellationToken>((sale, _) => persisted = sale)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        persisted.Should().NotBeNull();
        persisted!.SaleNumber.Should().Be("SALE-XYZ");
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsSaleWithAllItems()
    {
        // Arrange
        var command = BuildCommand();
        Sale? persisted = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
            .Callback<Sale, CancellationToken>((sale, _) => persisted = sale)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        persisted!.Items.Should().HaveCount(command.Items.Count);
    }

    [Fact]
    public async Task Handle_ValidCommand_ComputesCorrectTotalAmount()
    {
        // Arrange
        // qty=2 (<4) → no discount applied → 2 * 100 * 1.0 = 200
        var command = BuildCommand();
        Sale? persisted = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
            .Callback<Sale, CancellationToken>((sale, _) => persisted = sale)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        persisted!.TotalAmount.Should().Be(200m);
    }

    [Fact]
    public async Task Handle_ItemWithQuantityEqualOrAboveFour_AppliesTenPercentDiscount()
    {
        // Arrange
        // qty=4, price=100 → discount forced to 10% → 4 * 100 * 0.9 = 360
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
