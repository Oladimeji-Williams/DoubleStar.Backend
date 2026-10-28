namespace DoubleStar.BuildingBlocks.Application.Common.Queries.Filtering;

public sealed record FilterCriterion(
    string Field,
    FilterOperator Operator,
    string? Value = null,
    IReadOnlyList<string>? Values = null);