using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Konscious.Security.Cryptography;
using TechDebtHub.Application.Interfaces;

namespace TechDebtHub.Infrastructure.Security
{
    public sealed class Argon2idPasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 3;
        private const int MemorySize = 65536;
        private const int DegreeOfParallelism = 1;

        public string Hash(string senha)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);

            var hash = GerarHash(senha, salt, Iterations, MemorySize, DegreeOfParallelism);

            return string.Join(
                "$",
                "argon2id",
                "v=19",
                $"m={MemorySize},t={Iterations},p={DegreeOfParallelism}",
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash)
            );
        }

        public bool Verify(string senha, string hashArmazenamento)
        {
            if (string.IsNullOrWhiteSpace(hashArmazenamento))
            {
                return false;
            }

            var partes = hashArmazenamento.Split('$');

            if (partes.Length != 5 || partes[0] != "argon2id" || partes[1] != "v=19")
            {
                return false;
            }

            try
            {
                var parametros = partes[2]
                    .Split(',')
                    .Select(x => x.Split('='))
                    .ToDictionary(x => x[0], x => int.Parse(x[1]));

                var salt = Convert.FromBase64String(partes[3]);

                var hashEsperado = Convert.FromBase64String(partes[4]);

                var hashCalculado = GerarHash(
                    senha,
                    salt,
                    parametros["t"],
                    parametros["m"],
                    parametros["p"]
                );

                return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
            }
            catch
            {
                return false;
            }
        }

        private static byte[] GerarHash(
            string senha,
            byte[] salt,
            int iterations,
            int memorySize,
            int degreeOfParallelism
        )
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(senha))
            {
                Salt = salt,
                Iterations = iterations,
                MemorySize = memorySize,
                DegreeOfParallelism = degreeOfParallelism,
            };

            return argon2.GetBytes(HashSize);
        }
    }
}
