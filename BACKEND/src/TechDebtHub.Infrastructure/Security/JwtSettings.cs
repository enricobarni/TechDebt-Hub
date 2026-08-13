using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechDebtHub.Infrastructure.Security
{
    public sealed class JwtSettings
    {
        public const string SectionName = "Jwt";
        public string Issuer { get; init; } = string.Empty;
        public string Audience { get; init; } = string.Empty;
        public string SigningKey { get; init; } = string.Empty;
        public int ExpirationMinutes { get; init; }
    }
}
