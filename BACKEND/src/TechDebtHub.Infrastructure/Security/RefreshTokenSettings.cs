using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechDebtHub.Infrastructure.Security
{
    public sealed class RefreshTokenSettings
    {
        public const string SectionName = "RefreshToken";

        public int ExpirationDays { get; init; }
    }
}
