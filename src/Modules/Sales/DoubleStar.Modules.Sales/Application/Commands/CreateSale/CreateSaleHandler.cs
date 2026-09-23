// .../CreateSaleCommandHandler.cs
using DoubleStar.SharedKernel.Contracts.Customers;
using DoubleStar.Modules.Sales.Application.Abstractions;
using DoubleStar.Modules.Sales.Application.DTOs;
using DoubleStar.Modules.Sales.Application.Mappings;
using DoubleStar.Modules.Sales.Domain.Entities;

namespace DoubleStar.Modules.Sales.Application.Commands.CreateSaleCommand;

public sealed class CreateSaleCommandHandler(ISaleRepository saleRepository, ICustomerDirectory customerDirectory)
    : IRequestHandler<CreateSaleCommand, Result<SaleDto>>
{
    public async Task<Result<SaleDto>> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        Guid? customerId = request.CustomerId;

        if (customerId is null && !string.IsNullOrWhiteSpace(request.WalkInName))
        {
            var walkIn = await customerDirectory.GetOrCreateWalkInAsync(
                request.WalkInName, request.WalkInPhone, cancellationToken);
            customerId = walkIn.Id;
        }

        var sale = Sale.CreateDraft(customerId);
        await saleRepository.AddAsync(sale, cancellationToken);

        return Result<SaleDto>.Success(sale.ToDto());
    }
}