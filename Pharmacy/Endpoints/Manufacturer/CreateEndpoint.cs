﻿using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Manufacturer;

public class CreateEndpoint : Endpoint<CreateManufacturerRequest>
{
    private readonly IManufacturerService _manufacturerService;

    public CreateEndpoint(IManufacturerService manufacturerService)
    {
        _manufacturerService = manufacturerService;
    }
        
    public override void Configure()
    {
        Post("manufacturers");
        AllowAnonymous();
        Tags("Manufacturer");
        Summary(s => { s.Summary = "Добавить производителя"; });
    }

    public override async Task HandleAsync(CreateManufacturerRequest request, CancellationToken ct)
    {
        var result = await _manufacturerService.CreateAsync(request);

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

public record CreateManufacturerRequest(string Name, string Country);

public class CreateManufacturerRequestValidator : Validator<CreateManufacturerRequest>
{
    public CreateManufacturerRequestValidator()
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