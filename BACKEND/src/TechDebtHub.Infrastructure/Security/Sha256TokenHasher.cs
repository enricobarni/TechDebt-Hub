using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TechDebtHub.Application.Interfaces;

namespace TechDebtHub.Infrastructure.Security
{
    public sealed class Sha256TokenHasher : ITokenHasher
    {
        public string Hash(string token)
        {
            var tokenBytes = Encoding.UTF8.GetBytes(token);

            var hashBytes = SHA256.HashData(tokenBytes);

            return Convert.ToHexString(hashBytes);
        }
    }
}
