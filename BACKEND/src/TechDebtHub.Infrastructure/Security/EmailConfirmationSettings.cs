using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechDebtHub.Infrastructure.Security
{
    public sealed class EmailConfirmationSettings
    {
        public const string SectionName = "EmailConfirmation";
        public int ExpirationMinutes { get; init; }
        public int MaxAttempts { get; init; }
        public string HmacKey { get; init; } = string.Empty;
    }
}
