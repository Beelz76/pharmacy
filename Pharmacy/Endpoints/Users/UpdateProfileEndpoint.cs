using FastEndpoints;
using FluentValidation;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Users;

public class UpdateProfileEndpoint : Endpoint<UpdateProfileRequest>
{
    private readonly ILogger<UpdateProfileEndpoint> _logger;
    private readonly IUserService _userService;
    public UpdateProfileEndpoint(ILogger<UpdateProfileEndpoint> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    public override void Configure()
    {
        Put("users/profile");
        Roles("User", "Admin");
        Tags("Users");
        Summary(s => { s.Summary = "Обновить данные пользователя"; });
    }

    public override async Task HandleAsync(UpdateProfileRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _userService.UpdateProfileAsync(userId, request);
        if (result.IsSuccess)
        {
            await SendOkAsync(ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.Code, ct);
        }
    }
}

public record UpdateProfileRequest(string LastName, string FirstName, string? Patronymic, string? Phone);

public class UpdateProfileRequestValidator : Validator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(50);
        
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(50);
        
        RuleFor(x => x.Patronymic)
            .MaximumLength(50);
        
        RuleFor(x => x.Phone)
            .MaximumLength(12)
            .Matches(@"^\+7\d{10}$")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Введите номер в формате +7XXXXXXXXXX");
    }
}