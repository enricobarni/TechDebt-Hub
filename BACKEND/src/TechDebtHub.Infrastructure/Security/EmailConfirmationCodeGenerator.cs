using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TechDebtHub.Application.Interfaces;

namespace TechDebtHub.Infrastructure.Security
{
    public sealed class EmailConfirmationCodeGenerator : IEmailConfirmationCodeGenerator
    {
        private const int MinCode = 0;
        private const int MaxCodeExclusive = 100_000_000;

        private readonly EmailConfirmationSettings _settings;

        public EmailConfirmationCodeGenerator(IOptions<EmailConfirmationSettings> options)
        {
            _settings = options.Value;
        }

        public (string Codigo, DateTime DataExpiracao, int MaximoTentativas) Generate()
        {
            var valor = RandomNumberGenerator.GetInt32(MinCode, MaxCodeExclusive);

            var codigo = valor.ToString("D8");

            var dataExpiracao = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);

            return (codigo, dataExpiracao, _settings.MaxAttempts);
        }
    }
}
