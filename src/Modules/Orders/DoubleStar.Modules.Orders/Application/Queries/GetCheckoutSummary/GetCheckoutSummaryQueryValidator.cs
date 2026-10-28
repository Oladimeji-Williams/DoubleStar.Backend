using Microsoft.Extensions.Options;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.SharedKernel.Contracts.Orders;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.DTOs;
using DoubleStar.Modules.Orders.Application.Options;
using DoubleStar.Modules.Orders.Application.Services;

namespace DoubleStar.Modules.Orders.Application.Queries.GetCheckoutSummary;
public sealed class GetCheckoutSummaryQueryValidator : AbstractValidator<GetCheckoutSummaryQuery>
{
    public GetCheckoutSummaryQueryValidator() => RuleFor(x => x.FulfillmentMethod).IsInEnum();
}