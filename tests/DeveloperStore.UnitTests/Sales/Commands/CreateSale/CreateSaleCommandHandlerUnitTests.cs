using DeveloperStore.Application.Common.Interfaces;
using DeveloperStore.Application.Sales.Commands.CreateSale;
using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace DeveloperStore.UnitTests.Sales.Commands.CreateSale;

public class CreateSaleCommandHandlerUnitTests
{
    private readonly Mock<ISaleRepository> _repositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly CreateSaleCommandHandler _handler;

    public CreateSaleCommandHandlerUnitTests()
    {
        _repositoryMock = new Mock<ISaleRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _handler = new CreateSaleCommandHandler(_repositoryMock.Object, _eventPublisherMock.Object);
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

    [Fact]
    public async Task Handle_ItemWithQuantityBetweenTenAndTwenty_AppliesTwentyPercentDiscount()
    {
        // Arrange
        // qty=10 (>=10, <=20) → 20% discount → total = 10 * 100 * 0.8 = 800
        var command = BuildCommand(quantity: 10);
        Sale? persisted = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
            .Callback<Sale, CancellationToken>((sale, _) => persisted = sale)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        persisted!.Items[0].Discount.Should().Be(0.20m);
        persisted.TotalAmount.Should().Be(800m);
    }

    [Fact]
    public async Task Handle_ItemWithQuantityAboveTwenty_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = BuildCommand(quantity: 21);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*20*");
    }

    [Fact]
    public async Task Handle_ItemWithQuantityBelowFour_AppliesNoDiscount()
    {
        // Arrange
        // qty=3 (<4) → no discount → total = 3 * 100 = 300
        var command = BuildCommand(quantity: 3);
        Sale? persisted = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
            .Callback<Sale, CancellationToken>((sale, _) => persisted = sale)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        persisted!.Items[0].Discount.Should().Be(0m);
        persisted.TotalAmount.Should().Be(300m);
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
