namespace DoubleStar.Modules.Notifications.Api.Contracts;

public sealed record SendSaleReceiptRequest(Guid CustomerId, int SaleId, long TotalKobo);