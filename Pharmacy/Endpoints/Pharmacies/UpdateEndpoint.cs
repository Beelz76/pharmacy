using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto.Pharmacy;

namespace Pharmacy.Endpoints.Pharmacies;

public class UpdateEndpoint : Endpoint<UpdatePharmacyDto>
{
    private readonly IPharmacyService _service;

    public UpdateEndpoint(IPharmacyService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Put("pharmacies/{id:int}");
        Roles("Admin");
        Tags("Pharmacy");
        Summary(s => { s.Summary = "Обновить аптеку"; });
    }

    public override async Task HandleAsync(UpdatePharmacyDto request, CancellationToken ct)
    {
        int id = Route<int>("id");
        var result = await _service.UpdateAsync(id, request);
        if (result.IsSuccess)
        {
            await SendOkAsync(ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.StatusCode, ct);
        }
    }
}

public class UpdatePharmacyDtoValidator : Validator<UpdatePharmacyDto>
{
    public UpdatePharmacyDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Address).NotNull();
        RuleFor(x => x.Address.Latitude).NotEmpty();
        RuleFor(x => x.Address.Longitude).NotEmpty();
    }
}