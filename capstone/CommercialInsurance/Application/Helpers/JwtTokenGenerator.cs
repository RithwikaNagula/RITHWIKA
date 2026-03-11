// Generates signed JWT access tokens containing the user's ID, email, and role claims; used by AuthService after successful login.
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SecurityClaim = System.Security.Claims.Claim;
using ClaimTypes = System.Security.Claims.ClaimTypes;

namespace Application.Helpers
{
    public class JwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Constructs a signed JWT access token for the given user.
        // The token contains identity claims consumed by [Authorize] middleware and is valid for 8 hours.
        public string GenerateToken(User user)
        {
            // Derive a symmetric key from the secret stored in appsettings.json
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            // Pack identity claims into the token so downstream controllers can read the user's ID and role
            var claims = new List<SecurityClaim>
            {
                new SecurityClaim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new SecurityClaim(ClaimTypes.Email, user.Email),
                new SecurityClaim(ClaimTypes.Name, user.FullName),
                new SecurityClaim(ClaimTypes.Role, user.Role.ToString())
            };

            // Sign the token using HMAC-SHA256; this algorithm provides a good balance of speed and security
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Assemble the final token with configured issuer, audience, claims, and an 8-hour lifespan
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials
            );

            // Serialize the token object to a compact string format (header.payload.signature)
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
