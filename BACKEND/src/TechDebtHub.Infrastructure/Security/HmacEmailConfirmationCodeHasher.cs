using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TechDebtHub.Application.Interfaces;

namespace TechDebtHub.Infrastructure.Security
{
    public sealed class HmacEmailConfirmationCodeHasher : IEmailConfirmationCodeHasher
    {
        private readonly byte[] _key;

        public HmacEmailConfirmationCodeHasher(IOptions<EmailConfirmationSettings> options)
        {
            _key = Convert.FromBase64String(options.Value.HmacKey);
        }

        public string Hash(Guid usuarioId, string codigo)
        {
            var value = $"{usuarioId:N}:{codigo}";

            var valueBytes = Encoding.UTF8.GetBytes(value);

            var hash = HMACSHA256.HashData(_key, valueBytes);

            return Convert.ToHexString(hash);
        }
    }
}
