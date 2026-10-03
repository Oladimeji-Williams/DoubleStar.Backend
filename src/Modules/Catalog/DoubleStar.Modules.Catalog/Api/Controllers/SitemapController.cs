// Catalog/Api/Controllers/SitemapController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using DoubleStar.SharedKernel.Common.Options;
using DoubleStar.Modules.Catalog.Application.Abstractions;

namespace DoubleStar.Modules.Catalog.Api.Controllers;

[Route("api/v1/sitemap-products.xml")]
public sealed class SitemapController(
    IProductRepository productRepository,
    IOptions<FrontendOptions> frontendOptions)
    : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken)
    {
        var products =
            await productRepository.GetAllAsync(cancellationToken);

        var baseUrl = frontendOptions.Value.CustomerAppUrl;

        var urls = string.Join(
            "\n",
            products
                .Where(p => !p.IsArchived)
                .Select(p =>
                    $"  <url><loc>{baseUrl}/products/{p.Id}</loc></url>"));

        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
            """ + urls + """
            </urlset>
            """;

        return Content(xml, "application/xml");
    }
}