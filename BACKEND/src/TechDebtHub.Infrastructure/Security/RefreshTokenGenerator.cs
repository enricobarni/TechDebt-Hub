using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TechDebtHub.Application.Interfaces;

namespace TechDebtHub.Infrastructure.Security
{
    public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
    {
        private const int TokenSizeInBytes = 32;

        public string Generate()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(TokenSizeInBytes);

            return Convert.ToHexString(tokenBytes);
        }
    }
}
