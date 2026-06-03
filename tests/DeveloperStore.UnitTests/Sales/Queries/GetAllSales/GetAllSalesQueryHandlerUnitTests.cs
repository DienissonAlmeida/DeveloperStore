using DeveloperStore.Application.Sales.Queries.GetAllSales;
using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.Interfaces;
using DeveloperStore.UnitTests.Common;
using FluentAssertions;
using Moq;

namespace DeveloperStore.UnitTests.Sales.Queries.GetAllSales;

public class GetAllSalesQueryHandlerUnitTests
{
    private readonly Mock<ISaleRepository> _repositoryMock;
    private readonly GetAllSalesQueryHandler _handler;

    public GetAllSalesQueryHandlerUnitTests()
    {
        _repositoryMock = new Mock<ISaleRepository>();
        _handler = new GetAllSalesQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_NoSalesExist_ReturnsEmptyCollection()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Sale>());

        // Act
        var result = await _handler.Handle(new GetAllSalesQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_SalesExist_ReturnsAllMappedDtos()
    {
        // Arrange
        var sales = new[]
        {
            SaleBuilder.Build(saleNumber: "SALE-001"),
            SaleBuilder.Build(saleNumber: "SALE-002"),
            SaleBuilder.Build(saleNumber: "SALE-003")
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sales);

        // Act
        var result = await _handler.Handle(new GetAllSalesQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_SalesExist_MapsSaleNumbersCorrectly()
    {
        // Arrange
        var sales = new[]
        {
            SaleBuilder.Build(saleNumber: "SALE-001"),
            SaleBuilder.Build(saleNumber: "SALE-002")
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sales);

        // Act
        var result = await _handler.Handle(new GetAllSalesQuery(), CancellationToken.None);

        // Assert
        result.Select(s => s.SaleNumber)
            .Should().BeEquivalentTo("SALE-001", "SALE-002");
    }

    [Fact]
    public async Task Handle_SalesExist_MapsIdsCorrectly()
    {
        // Arrange
        var sales = new[]
        {
            SaleBuilder.Build(),
            SaleBuilder.Build()
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sales);

        // Act
        var result = await _handler.Handle(new GetAllSalesQuery(), CancellationToken.None);

        // Assert
        result.Select(s => s.Id)
            .Should().BeEquivalentTo(sales.Select(s => s.Id));
    }

    [Fact]
    public async Task Handle_CallsGetAllAsyncOnce()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Sale>());

        // Act
        await _handler.Handle(new GetAllSalesQuery(), CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
