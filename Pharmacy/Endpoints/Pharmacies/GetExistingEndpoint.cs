using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Pharmacies;

public class GetExistingEndpoint : Endpoint<GetExistingPharmacyRequest>
{
    private readonly IPharmacyService  _service;

    public GetExistingEndpoint(IPharmacyService service)
    {
        _service = service;
    }
        
    public override void Configure()
    {
        Post("pharmacy/existing");
        Roles("User", "Admin", "Employee");
        Tags("Pharmacy");
        Summary(s => { s.Summary = "Найти аптеку по параметрам"; });
    }

    public override async Task HandleAsync(GetExistingPharmacyRequest request, CancellationToken ct)
    {
        var result = await _service.GetExistingPharmacyIdAsync(request.Name, request.OsmId, request.Latitude, request.Longitude);

        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.StatusCode, ct);
        }
    }
}

public record GetExistingPharmacyRequest(
    string Name, 
    string? OsmId, 
    double Latitude, 
    double Longitude
);

public class GetExistingPharmacyRequestValidator : Validator<GetExistingPharmacyRequest>
{
    public GetExistingPharmacyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty();
        
        RuleFor(x => x.Latitude)
            .NotEmpty();
        
        RuleFor(x => x.Longitude)
            .NotEmpty();
    }
}