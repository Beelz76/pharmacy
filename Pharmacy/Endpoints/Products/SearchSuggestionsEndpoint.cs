using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Products;

public class SearchSuggestionsEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<SearchSuggestionsEndpoint> _logger;
    private readonly IProductService _productService;
    public SearchSuggestionsEndpoint(ILogger<SearchSuggestionsEndpoint> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public override void Configure()
    {
        Get("products/search-suggestions");
        AllowAnonymous();
        Tags("Products");
        Summary(s => { s.Summary = "Поиск названий товаров (автодополнение)"; }); 
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var query = Query<string>("query");
        if (string.IsNullOrWhiteSpace(query))
        {
            await SendAsync(new List<string>(), cancellation: ct);
            return;
        }
        
        var result = await _productService.GetSearchSuggestionsAsync(query);
        await SendOkAsync (result, ct);
    }
}