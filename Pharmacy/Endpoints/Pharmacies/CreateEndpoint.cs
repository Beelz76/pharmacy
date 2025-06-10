using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto.Pharmacy;

namespace Pharmacy.Endpoints.Pharmacies;

public class CreateEndpoint : Endpoint<CreatePharmacyDto>
{
    private readonly IPharmacyService _service;

    public CreateEndpoint(IPharmacyService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Post("pharmacies");
        Roles("Admin");
        Tags("Pharmacy");
        Summary(s => { s.Summary = "Создать аптеку"; });
    }

    public override async Task HandleAsync(CreatePharmacyDto request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request);
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

public class CreatePharmacyDtoValidator : Validator<CreatePharmacyDto>
{
    public CreatePharmacyDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Address).NotNull();
        RuleFor(x => x.Address.Latitude).NotEmpty();
        RuleFor(x => x.Address.Longitude).NotEmpty();
    }
}