using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Users.Authorization;

public class RegisterEndpoint : Endpoint<RegisterRequest>
{
    private readonly ILogger<RegisterEndpoint> _logger;
    private readonly IAuthorizationService _authorizationService;
    public RegisterEndpoint(ILogger<RegisterEndpoint> logger, IAuthorizationService authorizationService)
    {
        _logger = logger;
        _authorizationService = authorizationService;
    }

    public override void Configure()
    {
        Post("authorization/register-initial");
        AllowAnonymous();
        Tags("Authorization");
        Summary(s => { s.Summary = "Регистрация"; });
    }

    public override async Task HandleAsync(RegisterRequest request, CancellationToken ct)
    {
        var result = await _authorizationService.RegisterAsync(request);
        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.Code, ct);
        }
    }
}

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Patronymic,
    string? Phone);

public class RegisterRequestValidator : Validator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty();
        
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(50);
        
        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(50);
        
        RuleFor(x => x.Patronymic)
            .MaximumLength(50);
        
        RuleFor(x => x.Phone)
            .MaximumLength(11)
            .Matches(@"^7\d{10}$")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Введите номер в формате +7XXXXXXXXXX");
    }
}