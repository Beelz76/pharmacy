using FastEndpoints;
using FluentValidation;
using Pharmacy.Endpoints.Users.Authentication;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.User;
using Pharmacy.Shared.Enums;
using Pharmacy.Shared.Result;

namespace Pharmacy.Endpoints.Users;

public class CreateEndpoint : Endpoint<CreateUserRequest>
{
    private readonly ILogger<CreateEndpoint> _logger;
    private readonly IUserService _userService;
    private readonly PasswordProvider _passwordProvider;
    public CreateEndpoint(ILogger<CreateEndpoint> logger, IUserService userService, PasswordProvider passwordProvider)
    {
        _logger = logger;
        _userService = userService;
        _passwordProvider = passwordProvider;
    }

    public override void Configure()
    {
        Post("users");
        Roles("Admin");
        Tags("Users");
        Summary(s => { s.Summary = "Создать сотрудника/администратора"; });
    }

    public override async Task HandleAsync(CreateUserRequest request, CancellationToken ct)
    {
        var existingUserResult = await _userService.GetByEmailAsync(request.Email);
        if (existingUserResult.IsSuccess)
        {
            await SendAsync(Error.Conflict("Пользователь с таким email уже зарегистрирован"),  409, ct);
            return;
        }
        
        var passwordHash = _passwordProvider.Hash(request.Password);
        
        var result = await _userService.CreateAsync(new CreateUserDto(
            request.Email,
            passwordHash,
            true,
            request.FirstName,
            request.LastName,
            request.Patronymic,
            request.Phone,
            request.Role,
            request.PharmacyId
        ));
        
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

public record CreateUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Patronymic,
    string? Phone,
    UserRoleEnum Role,
    int? PharmacyId);

public class CreateUserRequestValidator : Validator<CreateUserRequest>
{
    public CreateUserRequestValidator()
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
            .MaximumLength(12)
            .Matches(@"^\+7\d{10}$")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Введите номер в формате +7XXXXXXXXXX");
        
        RuleFor(x => x.Role)
            .Must(role => role == UserRoleEnum.Employee || role == UserRoleEnum.Admin)
            .WithMessage("Роль должна быть Employee или Admin");
        
        RuleFor(x => x.PharmacyId)
            .NotNull()
            .When(x => x.Role == UserRoleEnum.Employee)
            .WithMessage("Сотрудник должен быть привязан к аптеке");
    }
}