using MediatR;

namespace DeveloperStore.Application.Sales.Commands.CancelSale;

public sealed record CancelSaleCommand(Guid Id) : IRequest<bool>;
