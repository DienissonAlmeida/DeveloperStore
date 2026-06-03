using MediatR;

namespace DeveloperStore.Application.Sales.Commands.DeleteSale;

public sealed record DeleteSaleCommand(Guid Id) : IRequest<bool>;
