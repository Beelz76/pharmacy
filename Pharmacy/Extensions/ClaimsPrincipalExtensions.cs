using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Pharmacy.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(value, out var id) ? id : throw new UnauthorizedAccessException("User ID не найден в этом токене");
    }
}