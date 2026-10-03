// SecurityHeadersExtensions.cs — full replacement
namespace DoubleStar.Api.Common.Security;

public static class SecurityHeadersExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;
            headers.Append("X-Content-Type-Options", "nosniff");
            headers.Append("X-Frame-Options", "DENY");
            headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
            headers.Append("Cross-Origin-Opener-Policy", "same-origin");
            headers.Append("Cross-Origin-Resource-Policy", "same-site");
            // This host serves only JSON and uploaded images (no server-rendered HTML),
            // so a strict CSP costs nothing: block everything by default, allow images
            // from this origin only (product photos, avatars under wwwroot/uploads).
            headers.Append("Content-Security-Policy", "default-src 'none'; img-src 'self'; frame-ancestors 'none'");
            await next();
        });
    }
}