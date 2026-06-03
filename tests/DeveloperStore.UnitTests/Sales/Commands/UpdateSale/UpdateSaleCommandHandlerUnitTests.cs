using DeveloperStore.Application.Sales.Commands.CreateSale;
using DeveloperStore.Application.Sales.Commands.UpdateSale;
using DeveloperStore.Domain.Interfaces;
using DeveloperStore.UnitTests.Common;
using FluentAssertions;
using Moq;

namespace DeveloperStore.UnitTests.Sales.Commands.UpdateSale;

public class UpdateSaleCommandHandlerUnitTests
{
    private readonly Mock<ISaleRepository> _repositoryMock;
    private readonly UpdateSaleCommandHandler _handler;

    public UpdateSaleCommandHandlerUnitTests()
    {
        _repositoryMock = new Mock<ISaleRepository>();
        _handler = new UpdateSaleCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_SaleNotFound_ReturnsNull()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Sale?)null);

        // Act
        var result = await _handler.Handle(BuildCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_SaleNotFound_NeverCallsUpdateOrSave()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Sale?)null);

        // Act
        await _handler.Handle(BuildCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.Update(It.IsAny<Domain.Entities.Sale>()), Times.Never);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SaleFound_ReturnsSaleDtoWithUpdatedSaleNumber()
    {
        // Arrange
        var sale = SaleBuilder.Build();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(sale.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        var command = BuildCommand(sale.Id, saleNumber: "SALE-UPDATED");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.SaleNumber.Should().Be("SALE-UPDATED");
    }

    [Fact]
    public async Task Handle_SaleFound_ReplacesItemsCorrectly()
    {
        // Arrange
        var sale = SaleBuilder.Build(); // starts with 1 item
        _repositoryMock
            .Setup(r => r.GetByIdAsync(sale.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        var command = BuildCommand(sale.Id, itemCount: 3);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result!.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_SaleFound_CallsUpdateAndSaveChangesOnce()
    {
        // Arrange
        var sale = SaleBuilder.Build();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(sale.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        // Act
        await _handler.Handle(BuildCommand(sale.Id), CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.Update(sale), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SaleFound_UpdatesCustomerAndBranch()
    {
        // Arrange
        var sale = SaleBuilder.Build();
        var newCustomerId = Guid.NewGuid();
        var newBranchId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(sale.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        var command = BuildCommand(sale.Id, customerId: newCustomerId, branchId: newBranchId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result!.CustomerId.Should().Be(newCustomerId);
        result.BranchId.Should().Be(newBranchId);
    }

    // ──────────────────────────────────────────────────────────────────────────

    private static UpdateSaleCommand BuildCommand(
        Guid? id = null,
        string saleNumber = "SALE-001",
        Guid? customerId = null,
        Guid? branchId = null,
        int itemCount = 1) =>
        new(
            Id: id ?? Guid.NewGuid(),
            SaleNumber: saleNumber,
            SaleDate: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CustomerId: customerId ?? Guid.NewGuid(),
            CustomerName: "Jane Doe",
            BranchId: branchId ?? Guid.NewGuid(),
            BranchName: "Updated Branch",
            Items: Enumerable.Range(1, itemCount)
                .Select(i => new CreateSaleItemDto(
                    ProductId: Guid.NewGuid(),
                    ProductName: $"Product {i}",
                    Quantity: 1,
                    UnitPrice: 50m,
                    Discount: 0m))
                .ToList());
}
