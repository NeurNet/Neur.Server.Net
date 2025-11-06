using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Infrastructure.Interfaces;

namespace Neur.Server.Net.Infrastructure;

public class JwtProvider : IJwtProvider {
    private readonly JwtOptions _options;
    
    public JwtProvider(IOptions<JwtOptions> options) {
        _options = options.Value;
    }
    
    public string GenerateToken(UserEntity user) {
        Claim[] claims = new[]
        {
            new Claim("userId", user.Username),
            new Claim("tokens", user.Tokens.ToString())
        };
        
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            claims: claims,
            signingCredentials: signingCredentials,
            expires: DateTime.UtcNow.AddHours(_options.ExpiresHours)
        );
        
        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
        return tokenValue;
    }
}