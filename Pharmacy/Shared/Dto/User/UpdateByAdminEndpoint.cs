using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Enums;

namespace Pharmacy.Shared.Dto.User;

public class UpdateByAdminEndpoint : Endpoint<UpdateUserRequest>
{
    private readonly ILogger<UpdateByAdminEndpoint> _logger;
    private readonly IUserService _userService;
    public UpdateByAdminEndpoint(ILogger<UpdateByAdminEndpoint> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    public override void Configure()
    {
        Put("users/{userId:int}");
        Roles("Admin");
        Tags("Users");
        Summary(s => { s.Summary = "Обновить пользователя"; });
    }

    public override async Task HandleAsync(UpdateUserRequest request, CancellationToken ct)
    {
        var userId = Route<int>("userId");
        var dto = new UpdateUserDto(request.Email, request.FirstName, request.LastName, request.Patronymic, request.Phone, request.Role, request.PharmacyId);
        var result = await _userService.UpdateAsync(userId, dto);
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

public record UpdateUserRequest(
    string Email,
    string FirstName,
    string LastName,
    string? Patronymic,
    string? Phone,
    UserRoleEnum Role,
    int? PharmacyId);

public class UpdateUserRequestValidator : Validator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Patronymic).MaximumLength(50);
        RuleFor(x => x.Phone)
            .MaximumLength(11)
            .Matches(@"^7\d{10}$")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Введите номер в формате 7XXXXXXXXXX");
        RuleFor(x => x.Role)
            .Must(r => r == UserRoleEnum.Employee || r == UserRoleEnum.Admin)
            .WithMessage("Роль должна быть Employee или Admin");
        RuleFor(x => x.PharmacyId)
            .NotNull()
            .When(x => x.Role == UserRoleEnum.Employee)
            .WithMessage("Сотрудник должен быть привязан к аптеке");
    }
}