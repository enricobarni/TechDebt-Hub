using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechDebtHub.Application.Features.Usuarios.Login
{
    public sealed record LoginResponse(
        string AccessToken,
        string RefreshToken,
        string TokenType,
        DateTime RefreshTokenExpiresAt
    );
}
