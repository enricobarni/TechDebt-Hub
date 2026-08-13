using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechDebtHub.Application.Features.Usuarios.CadastrarUsuario
{
    public sealed record CadastrarUsuarioResponse(Guid Id, string Nome, string Email);
}
