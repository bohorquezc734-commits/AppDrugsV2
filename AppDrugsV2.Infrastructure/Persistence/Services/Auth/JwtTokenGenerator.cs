using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using AppDrugsV2.Application.Common.Interfaces;

namespace AppDrugsV2.Infrastructure.Services.Auth
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public string GenerateToken(int userId, string email, string role)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email es requerido", nameof(email));

            if (string.IsNullOrEmpty(role))
                throw new ArgumentException("Role es requerido", nameof(role));

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret no configurado");
            var issuer = jwtSettings["Issuer"] ?? "AppDrugsV2";
            var audience = jwtSettings["Audience"] ?? "AppDrugsV2Client";
            var expiryMinutes = int.TryParse(jwtSettings["ExpiryMinutes"], out var minutes) ? minutes : 60;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}