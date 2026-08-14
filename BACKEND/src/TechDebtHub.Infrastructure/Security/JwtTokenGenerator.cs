using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TechDebtHub.Application.Interfaces;

namespace TechDebtHub.Infrastructure.Security
{
    public sealed class JwtTokenGenerator : IJwtTokenGenerator
    {
        private const string SubjectClaim = "sub";
        private const string JwtIdClaim = "jti";

        private readonly JwtSettings _settings;
        private readonly JsonWebTokenHandler _tokenHandler;

        public JwtTokenGenerator(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
            _tokenHandler = new JsonWebTokenHandler();
        }

        public string Generate(Guid usuarioId)
        {
            var now = DateTime.UtcNow;

            var signingKey = new SymmetricSecurityKey(
                Convert.FromBase64String(_settings.SigningKey)
            );

            var signingCredentials = new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256
            );

            var tokenDescription = new SecurityTokenDescriptor
            {
                Issuer = _settings.Issuer,
                Audience = _settings.Audience,
                IssuedAt = now,
                NotBefore = now,
                Expires = now.AddMinutes(_settings.ExpirationMinutes),

                Claims = new Dictionary<string, object>
                {
                    [SubjectClaim] = usuarioId.ToString(),
                    [JwtIdClaim] = Guid.NewGuid().ToString(),
                },

                SigningCredentials = signingCredentials,
            };

            return _tokenHandler.CreateToken(tokenDescription);
        }
    }
}
