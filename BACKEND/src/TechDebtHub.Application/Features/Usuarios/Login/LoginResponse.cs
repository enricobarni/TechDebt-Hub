using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechDebtHub.Application.Features.Usuarios.Login
{
    public sealed record LoginResponse(string AcessToken, string TokenType);
}
