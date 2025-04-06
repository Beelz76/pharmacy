using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Pharmacy.Models.Entities;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Data.Authorization;

public class TokenProvider
{
    private readonly IConfiguration _configuration;
    public TokenProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string Create(User user)
    {
        string secretKey = _configuration["Jwt:Secret"]!;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                //new Claim(ClaimTypes.Role, ),
            }),
            Expires = DateTime.UtcNow.AddHours(_configuration.GetValue<int>("Jwt:ExpirationInHours")),
            SigningCredentials = credentials,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
        };
        
        var handler = new JsonWebTokenHandler();
        string token = handler.CreateToken(tokenDescriptor);
        
        return token;
    }
}