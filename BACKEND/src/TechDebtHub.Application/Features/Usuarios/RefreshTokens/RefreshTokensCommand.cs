using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechDebtHub.Application.Features.Usuarios.RefreshTokens
{
    public sealed record RefreshTokensCommand(string RefreshToken);
}
