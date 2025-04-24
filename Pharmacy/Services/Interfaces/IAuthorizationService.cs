using Pharmacy.Endpoints.Users.Authorization;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface IAuthorizationService
{
    Task<Result<string>> RegisterAsync(RegisterRequest request);
    Task<Result<string>> LoginAsync(LoginRequest request);
}