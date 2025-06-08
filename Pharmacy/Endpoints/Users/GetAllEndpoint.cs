using FastEndpoints;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.User;

namespace Pharmacy.Endpoints.Users;

public class GetAllEndpoint : Endpoint<UserFilters>
{
    private readonly ILogger<GetAllEndpoint> _logger;
    private readonly IUserService _userService;
    public GetAllEndpoint(ILogger<GetAllEndpoint> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    public override void Configure()
    {
        Post("users/paginated");
        //Roles("Admin");
        AllowAnonymous();
        Tags("Users");
        Summary(s => { s.Summary = "Получить пользователей"; });
    }

    public override async Task HandleAsync(UserFilters filters, CancellationToken ct)
    {
        int pageNumber = Query<int>("pageNumber", isRequired: false) == 0 ? 1 : Query<int>("pageNumber", isRequired: false);
        int pageSize = Query<int>("pageSize", isRequired: false) == 0 ? 20 : Query<int>("pageSize", isRequired: false);
        
        var result = await _userService.GetPaginatedUsersAsync(filters, pageNumber, pageSize);
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