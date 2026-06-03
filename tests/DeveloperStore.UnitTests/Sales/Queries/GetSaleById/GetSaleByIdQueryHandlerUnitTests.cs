using DeveloperStore.Application.Sales.Queries.GetSaleById;
using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.Interfaces;
using DeveloperStore.UnitTests.Common;
using FluentAssertions;
using Moq;

namespace DeveloperStore.UnitTests.Sales.Queries.GetSaleById;

public class GetSaleByIdQueryHandlerUnitTests
{
    private readonly Mock<ISaleRepository> _repositoryMock;
    private readonly GetSaleByIdQueryHandler _handler;

    public GetSaleByIdQueryHandlerUnitTests()
    {
        _repositoryMock = new Mock<ISaleRepository>();
        _handler = new GetSaleByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_SaleFound_ReturnsMappedDto()
    {
        // Arrange
        // qty=2, price=100, discount=10% → total = 2 * 100 * 0.9 = 180
        var customerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var sale = SaleBuilder.Build(saleNumber: "SALE-ABC", customerId: customerId, branchId: branchId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(sale.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        // Act
        var result = await _handler.Handle(new GetSaleByIdQuery(sale.Id), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(sale.Id);
        result.SaleNumber.Should().Be("SALE-ABC");
        result.CustomerId.Should().Be(customerId);
        result.BranchId.Should().Be(branchId);
        result.Items.Should().HaveCount(1);
        result.TotalAmount.Should().Be(180m);
    }

    [Fact]
    public async Task Handle_SaleNotFound_ReturnsNull()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sale?)null);

        // Act
        var result = await _handler.Handle(new GetSaleByIdQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
