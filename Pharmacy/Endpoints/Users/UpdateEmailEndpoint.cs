using FastEndpoints;
using FluentValidation;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Users;

public class UpdateEmailEndpoint : Endpoint<UpdateEmailRequest>
{
    private readonly ILogger<UpdateEmailEndpoint> _logger;
    private readonly IUserService _userService;
    public UpdateEmailEndpoint(ILogger<UpdateEmailEndpoint> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    public override void Configure()
    {
        Post("users/change-email");
        Roles("User", "Admin");
        Tags("Users");
        Summary(s => { s.Summary = "Отправить код подтверждения на новый email"; });
    }

    public override async Task HandleAsync(UpdateEmailRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }
        
        var result = await _userService.UpdateEmailRequestAsync(userId.Value, request.NewEmail);
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

public record UpdateEmailRequest(string NewEmail);

public class UpdateEmailRequestValidator : Validator<UpdateEmailRequest>
{
    public UpdateEmailRequestValidator()
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty()
            .EmailAddress();
    }
}