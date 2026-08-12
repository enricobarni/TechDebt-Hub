using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechDebtHub.Application.Interfaces
{
    public interface IPasswordHasher
    {
        string Hash(string senha);

        bool Verify(string senha, string hash);
    }
}
