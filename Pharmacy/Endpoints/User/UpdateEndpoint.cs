using FastEndpoints;

namespace Pharmacy.Endpoints.User;

public class UpdateEndpoint : Endpoint<UpdateUserRequest>
{
    private readonly ILogger<UpdateEndpoint> _logger;
    
    public UpdateEndpoint(ILogger<UpdateEndpoint> logger)
    {
        _logger = logger;
    }

    public override void Configure()
    {
        Post("users/login");
        AllowAnonymous();
        Tags("Authorization");
    }

    public override async Task HandleAsync(UpdateUserRequest request, CancellationToken ct)
    {

        await SendOkAsync(ct);
    }
}

public record UpdateUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Patronymic,
    string Phone);
