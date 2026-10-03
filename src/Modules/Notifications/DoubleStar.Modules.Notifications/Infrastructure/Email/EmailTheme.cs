// Modules/Notifications/Infrastructure/Email/EmailTheme.cs — full replacement
namespace DoubleStar.Modules.Notifications.Infrastructure.Email;

internal static class EmailTheme
{
    public static string Apply(string html) => html.Replace("</head>", """
        <style>
          :root { color-scheme: light dark; }
          @media (prefers-color-scheme: dark) {
            body, body > table, body > table > tbody > tr > td { background-color:#0d1117 !important; }
            body > table table { background-color:#151b24 !important; }
            body > table table h1, body > table table div, body > table table p { color:#f4f7fb !important; }
            body > table table .code span { background-color:#1d2632 !important; border-color:#3b4858 !important; color:#f4f7fb !important; }
            body > table table td[style*="border-top"] { border-color:#2c3745 !important; }
          }
        </style>
        </head>
        """, StringComparison.Ordinal);
}