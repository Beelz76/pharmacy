using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Manufacturer;

public class UpdateEndpoint : Endpoint<UpdateManufacturerRequest>
{
    private readonly IManufacturerService _manufacturerService;

    public UpdateEndpoint(IManufacturerService manufacturerService)
    {
        _manufacturerService = manufacturerService;
    }
        
    public override void Configure()
    {
        Put("manufacturers/{id:int}");
        AllowAnonymous();
        Tags("Manufacturer");
        Summary(s => { s.Summary = "Изменить производителя"; });
    }

    public override async Task HandleAsync(UpdateManufacturerRequest request, CancellationToken ct)
    {
        var id = Route<int>("id");
        var result = await _manufacturerService.UpdateAsync(id, request);

        if (!result.IsSuccess)
        {
            await SendAsync(result.Error, (int)result.Error.Code, cancellation: ct);
        }
        else
        {
            await SendOkAsync(ct);
        }
    }
}

public record UpdateManufacturerRequest(string Name, string Country);
    
public class UpdateManufacturerValidator : Validator<UpdateManufacturerRequest>
{
    public UpdateManufacturerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);
            //.MustAsync(async (name));

        RuleFor(x => x.Country)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);
    }
}