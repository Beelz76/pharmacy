﻿using FastEndpoints;
using FluentValidation;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Users;

public class UpdatePasswordEndpoint : Endpoint<UpdatePasswordRequest>
{
    private readonly ILogger<UpdatePasswordEndpoint> _logger;
    private readonly IUserService _userService;
    public UpdatePasswordEndpoint(ILogger<UpdatePasswordEndpoint> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    public override void Configure()
    {
        Put("users/change-password");
        Roles("User", "Admin");
        Tags("Users");
        Summary(s => { s.Summary = "Изменить пароль пользователя"; });
    }

    public override async Task HandleAsync(UpdatePasswordRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }
        
        var result = await _userService.UpdatePasswordAsync(userId.Value, request.CurrentPassword, request.NewPassword);
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

public record UpdatePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmNewPassword);

public class UpdatePasswordRequestValidator : Validator<UpdatePasswordRequest>
{
    public UpdatePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty();
        
        RuleFor(x => x.NewPassword)
            .MinimumLength(6)
            .NotEmpty();
        
        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty()
            .Equal(x => x.NewPassword);
    }
}