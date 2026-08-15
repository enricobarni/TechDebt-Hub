using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TechDebtHub.Application.Interfaces;

namespace TechDebtHub.Infrastructure.Security
{
    public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
    {
        private const int TokenSizeInBytes = 32;
        private readonly RefreshTokenSettings _settings;

        public RefreshTokenGenerator(IOptions<RefreshTokenSettings> options)
        {
            _settings = options.Value;
        }

        public (string Token, DateTime DataExpiracao) Generate()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(TokenSizeInBytes);

            var token = Convert.ToHexString(tokenBytes);

            var dataExpiracao = DateTime.UtcNow.AddDays(_settings.ExpirationDays);

            return (token, dataExpiracao);
        }
    }
}
