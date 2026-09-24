// Api/Contracts/SendLowStockAlertRequest.cs
namespace DoubleStar.Modules.Notifications.Api.Contracts;

public sealed record SendLowStockAlertRequest(string ProductName, int AvailableQuantity, string StaffEmail);