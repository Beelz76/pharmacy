using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Pharmacy.Shared.Enums;

namespace Pharmacy.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(value, out var id) ? id : null;
    }
    
    public static UserRoleEnum GetUserRole(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(ClaimTypes.Role)?.Value;
        return Enum.TryParse<UserRoleEnum>(value, out var role) ? role : UserRoleEnum.User;
    }
}