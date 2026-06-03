using DeveloperStore.Application.Common.Interfaces;
using DeveloperStore.Application.Sales.Commands.DeleteSale;
using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.Interfaces;
using DeveloperStore.UnitTests.Common;
using FluentAssertions;
using Moq;

namespace DeveloperStore.UnitTests.Sales.Commands.DeleteSale;

public class DeleteSaleCommandHandlerUnitTests
{
    private readonly Mock<ISaleRepository> _repositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly DeleteSaleCommandHandler _handler;

    public DeleteSaleCommandHandlerUnitTests()
    {
        _repositoryMock = new Mock<ISaleRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _handler = new DeleteSaleCommandHandler(_repositoryMock.Object, _eventPublisherMock.Object);
    }

    [Fact]
    public async Task Handle_SaleFound_RemovesPublishesEventsAndReturnsTrue()
    {
        // Arrange
        var sale = SaleBuilder.Build(); // has 1 item
        _repositoryMock
            .Setup(r => r.GetByIdAsync(sale.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        // Act
        var result = await _handler.Handle(new DeleteSaleCommand(sale.Id), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.Remove(sale), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        // 1 ItemCancelled + 1 SaleCancelled
        _eventPublisherMock.Verify(
            p => p.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Exactly(sale.Items.Count + 1));
    }

    [Fact]
    public async Task Handle_SaleNotFound_ReturnsFalseAndDoesNotPersist()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sale?)null);

        // Act
        var result = await _handler.Handle(new DeleteSaleCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _repositoryMock.Verify(r => r.Remove(It.IsAny<Sale>()), Times.Never);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _eventPublisherMock.Verify(
            p => p.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
