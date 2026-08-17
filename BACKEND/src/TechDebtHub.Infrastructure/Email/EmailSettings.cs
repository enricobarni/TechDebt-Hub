using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechDebtHub.Infrastructure.Email
{
    public sealed class EmailSettings
    {
        public const string SectionName = "Email";
        public string ApiKey { get; init; } = string.Empty;
        public string FromEmail { get; init; } = string.Empty;
        public string FromName { get; init; } = string.Empty;
    }
}
