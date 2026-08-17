using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechDebtHub.Application.Interfaces
{
    public interface IEmailConfirmationCodeHasher
    {
        string Hash(Guid usuarioId, string codigo);
    }
}
