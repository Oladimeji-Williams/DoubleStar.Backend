// Modules/Notifications/Application/Templates/EmailTemplates.cs — full replacement
using DoubleStar.Modules.Notifications.Infrastructure.Email;
using DoubleStar.SharedKernel.Contracts.Repairs;

namespace DoubleStar.Modules.Notifications.Application.Templates;

internal static class EmailTemplates
{
    public static (string Subject, string Html) RepairStatusUpdate(string customerName, int ticketId, RepairStatus newStatus) =>
    (
        $"Update on your repair #{ticketId}",
        $"""
        <p>Hi {customerName},</p>
        <p>Your repair ticket <strong>#{ticketId}</strong> is now <strong>{newStatus}</strong>.</p>
        <p>Thanks for choosing Double Star.</p>
        """
    );

    public static (string Subject, string Html) SaleReceipt(string customerName, int saleId, long totalKobo) =>
    (
        $"Your receipt from Double Star — Sale #{saleId}",
        $"""
        <p>Hi {customerName},</p>
        <p>Thanks for your purchase — total <strong>{totalKobo / 100m:N2} NGN</strong> (Sale #{saleId}).</p>
        """
    );

    public static (string Subject, string Html) LowStockAlert(string productName, int availableQuantity) =>
    (
        $"Low stock: {productName}",
        $"<p><strong>{productName}</strong> has only <strong>{availableQuantity}</strong> unit(s) left.</p>"
    );

    public static string Notification(string title, string message, string? actionUrl, string? actionLabel, DateTimeOffset sentAt)
    {
        var formatted = sentAt.ToString("dd MMMM yyyy, h:mm tt");
        var button = actionUrl is null ? "" : $$"""
            <tr><td align="center" style="padding:24px 32px;">
              <table role="presentation" cellpadding="0" cellspacing="0" border="0" style="margin:0 auto;">
                <tr><td align="center" style="border-radius:8px; background-color:#f59e0b;">
                  <a href="{{actionUrl}}" style="display:inline-block; padding:14px 32px; font-size:15px; font-weight:600; color:#0f172a; text-decoration:none;">{{actionLabel}}</a>
                </td></tr>
              </table>
            </td></tr>
            """;

        return EmailTheme.Apply($$"""
            <!DOCTYPE html><html lang="en"><head><meta charset="utf-8" /><title>{{title}}</title></head>
            <body style="margin:0; padding:0; background-color:#f8fafc; font-family:system-ui,sans-serif;">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f8fafc;">
                <tr><td align="center" style="padding:32px 16px;">
                  <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="max-width:480px; background-color:#ffffff; border-radius:8px; overflow:hidden;">
                    <tr><td align="center" style="padding:32px 32px 0 32px;"><div style="font-size:18px; font-weight:700; color:#0f172a;">Double Star</div></td></tr>
                    <tr><td align="center" style="padding:24px 32px 8px 32px;">
                      <h1 style="margin:0 0 12px 0; font-size:20px; font-weight:700; color:#0f172a;">{{title}}</h1>
                      <p style="margin:0; font-size:14px; line-height:22px; color:#475569;">{{message}}</p>
                    </td></tr>
                    {{button}}
                    <tr><td style="padding:0 32px;"><div style="border-top:1px solid #e2e8f0; height:1px;"></div></td></tr>
                    <tr><td align="center" style="padding:20px 32px 32px 32px;">
                      <p style="margin:0; font-size:11px; color:#cbd5e1;">Sent on {{formatted}}</p>
                    </td></tr>
                  </table>
                  <p style="margin:20px 0 0 0; font-size:12px; color:#94a3b8; text-align:center;">&copy; {{sentAt.Year}} Double Star</p>
                </td></tr>
              </table>
            </body></html>
            """);
    }

    public static string PasswordReset(string resetLink, DateTimeOffset sentAt)
    {
        var formatted = sentAt.ToString("dd MMMM yyyy, h:mm tt");
        return EmailTheme.Apply($$"""
            <!DOCTYPE html><html lang="en"><head><meta charset="utf-8" /><title>Reset your password</title></head>
            <body style="margin:0; padding:0; background-color:#f8fafc; font-family:system-ui,sans-serif;">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f8fafc;">
                <tr><td align="center" style="padding:32px 16px;">
                  <a href="{{resetLink}}" style="display:block; max-width:480px; margin:0 auto; text-decoration:none;">
                    <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="max-width:480px; background-color:#ffffff; border-radius:8px; overflow:hidden;">
                      <tr><td align="center" style="padding:32px 32px 0 32px;"><div style="font-size:18px; font-weight:700; color:#0f172a;">Double Star</div></td></tr>
                      <tr><td align="center" style="padding:24px 32px 8px 32px;">
                        <h1 style="margin:0 0 12px 0; font-size:20px; font-weight:700; color:#0f172a;">Reset your password</h1>
                        <p style="margin:0; font-size:14px; line-height:22px; color:#475569;">Click anywhere in this email to choose a new password.</p>
                      </td></tr>
                      <tr><td align="center" style="padding:24px 32px;">
                        <table role="presentation" cellpadding="0" cellspacing="0" border="0" style="margin:0 auto;">
                          <tr><td align="center" style="border-radius:8px; background-color:#f59e0b;">
                            <span style="display:inline-block; padding:14px 32px; font-size:15px; font-weight:600; color:#0f172a;">Reset password</span>
                          </td></tr>
                        </table>
                      </td></tr>
                      <tr><td style="padding:0 32px;"><div style="border-top:1px solid #e2e8f0; height:1px;"></div></td></tr>
                      <tr><td align="center" style="padding:20px 32px 32px 32px;">
                        <p style="margin:0; font-size:12px; color:#94a3b8;">If you didn't request this, you can safely ignore this email.</p>
                        <p style="margin:16px 0 0 0; font-size:11px; color:#cbd5e1;">Sent on {{formatted}}</p>
                      </td></tr>
                    </table>
                  </a>
                  <p style="margin:20px 0 0 0; font-size:12px; color:#94a3b8; text-align:center;">&copy; {{sentAt.Year}} Double Star</p>
                </td></tr>
              </table>
            </body></html>
            """);
    }

    public static string SignInCode(string code, string magicLink, DateTimeOffset sentAt)
    {
        var formatted = sentAt.ToString("dd MMMM yyyy, h:mm tt");
        var codeBoxes = string.Concat(code.Select(digit =>
            $"<span class=\"code\" style=\"display:inline-block; width:34px; height:44px; margin:0 3px; border:1px solid #e2e8f0; border-radius:4px; background-color:#ffffff; font-size:26px; font-weight:700; color:#0f172a; text-align:center; line-height:44px;\">{System.Net.WebUtility.HtmlEncode(digit.ToString())}</span>"));

        return EmailTheme.Apply($$"""
            <!DOCTYPE html><html lang="en"><head><meta charset="utf-8" /><title>Sign in to Double Star</title></head>
            <body style="margin:0; padding:0; background-color:#f8fafc; font-family:system-ui,sans-serif;">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f8fafc;">
                <tr><td align="center" style="padding:32px 16px;">
                  <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="max-width:480px; background-color:#ffffff; border-radius:8px; overflow:hidden;">
                    <tr><td align="center" style="padding:32px 32px 0 32px;"><div style="font-size:18px; font-weight:700; color:#0f172a;">Double Star</div></td></tr>
                    <tr><td align="center" style="padding:24px 32px 8px 32px;">
                      <h1 style="margin:0 0 12px 0; font-size:20px; font-weight:700; color:#0f172a;">Sign in to Double Star</h1>
                      <p style="margin:0; font-size:14px; line-height:22px; color:#475569;">Use the code below, or click the button to sign in instantly.</p>
                    </td></tr>
                    <tr><td align="center" style="padding:20px 32px;">{{codeBoxes}}</td></tr>
                    <tr><td align="center" style="padding:8px 32px 24px 32px;">
                      <table role="presentation" cellpadding="0" cellspacing="0" border="0" style="margin:0 auto;">
                        <tr><td align="center" style="border-radius:8px; background-color:#f59e0b;">
                          <a href="{{magicLink}}" style="display:inline-block; padding:14px 32px; font-size:15px; font-weight:600; color:#0f172a; text-decoration:none;">Sign in</a>
                        </td></tr>
                      </table>
                    </td></tr>
                    <tr><td style="padding:0 32px;"><div style="border-top:1px solid #e2e8f0; height:1px;"></div></td></tr>
                    <tr><td align="center" style="padding:20px 32px 32px 32px;">
                      <p style="margin:0; font-size:12px; color:#94a3b8;">This code and link expire in 10 minutes.</p>
                      <p style="margin:16px 0 0 0; font-size:11px; color:#cbd5e1;">Sent on {{formatted}}</p>
                    </td></tr>
                  </table>
                  <p style="margin:20px 0 0 0; font-size:12px; color:#94a3b8; text-align:center;">&copy; {{sentAt.Year}} Double Star</p>
                </td></tr>
              </table>
            </body></html>
            """);
    }

    public static string EmailConfirmation(string confirmLink, DateTimeOffset sentAt)
    {
        var formatted = sentAt.ToString("dd MMMM yyyy, h:mm tt");
        return EmailTheme.Apply($$"""
            <!DOCTYPE html><html lang="en"><head><meta charset="utf-8" /><title>Confirm your email</title></head>
            <body style="margin:0; padding:0; background-color:#f8fafc; font-family:system-ui,sans-serif;">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f8fafc;">
                <tr><td align="center" style="padding:32px 16px;">
                  <a href="{{confirmLink}}" style="display:block; max-width:480px; margin:0 auto; text-decoration:none;">
                    <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="max-width:480px; background-color:#ffffff; border-radius:8px; overflow:hidden;">
                      <tr><td align="center" style="padding:32px 32px 0 32px;"><div style="font-size:18px; font-weight:700; color:#0f172a;">Double Star</div></td></tr>
                      <tr><td align="center" style="padding:24px 32px 8px 32px;">
                        <h1 style="margin:0 0 12px 0; font-size:20px; font-weight:700; color:#0f172a;">Confirm your email</h1>
                        <p style="margin:0; font-size:14px; line-height:22px; color:#475569;">Thanks for signing up for Double Star. Click anywhere in this email to verify your email address — you'll need to do this before you can sign in.</p>
                      </td></tr>
                      <tr><td align="center" style="padding:24px 32px;">
                        <table role="presentation" cellpadding="0" cellspacing="0" border="0" style="margin:0 auto;">
                          <tr><td align="center" style="border-radius:8px; background-color:#f59e0b;">
                            <span style="display:inline-block; padding:14px 32px; font-size:15px; font-weight:600; color:#0f172a;">Verify email</span>
                          </td></tr>
                        </table>
                      </td></tr>
                      <tr><td style="padding:0 32px;"><div style="border-top:1px solid #e2e8f0; height:1px;"></div></td></tr>
                      <tr><td align="center" style="padding:20px 32px 32px 32px;">
                        <p style="margin:0; font-size:12px; color:#94a3b8;">If you didn't create this account, you can safely ignore this email.</p>
                        <p style="margin:16px 0 0 0; font-size:11px; color:#cbd5e1;">Sent on {{formatted}}</p>
                      </td></tr>
                    </table>
                  </a>
                  <p style="margin:20px 0 0 0; font-size:12px; color:#94a3b8; text-align:center;">&copy; {{sentAt.Year}} Double Star</p>
                </td></tr>
              </table>
            </body></html>
            """);
    }
}