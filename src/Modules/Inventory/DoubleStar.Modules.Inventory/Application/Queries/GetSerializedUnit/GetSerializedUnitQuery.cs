// Application/Queries/GetSerializedUnitQuery/GetSerializedUnitQuery.cs
using DoubleStar.SharedKernel.Contracts.Inventory;

namespace DoubleStar.Modules.Inventory.Application.Queries.GetSerializedUnitQuery;

public sealed record GetSerializedUnitQuery(string SerialNumber) : IRequest<Result<SerializedUnitDto>>;