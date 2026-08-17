using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechDebtHub.Application.Features.Usuarios.BuscarUsuarioAtual
{
    public sealed record BuscarUsuarioAtualResponse(
        Guid Id,
        string Nome,
        string Email,
        bool EmailConfirmado,
        DateTime DataCriacao
    );
}
