using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechDebtHub.Api.Contracts.Usuarios
{
    public sealed record CadastrarUsuarioRequest(string Nome, string Email, string Senha);
}
