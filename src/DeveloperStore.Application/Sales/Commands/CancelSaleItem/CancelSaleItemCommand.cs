using MediatR;

namespace DeveloperStore.Application.Sales.Commands.CancelSaleItem;

public sealed record CancelSaleItemCommand(Guid SaleId, Guid ItemId) : IRequest<bool>;
