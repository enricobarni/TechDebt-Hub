using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechDebtHub.Application.Features.Usuarios.RefreshTokens
{
    public sealed record RefreshTokensResponse(
        string AccessToken,
        string RefreshToken,
        string TokenType,
        DateTime RefreshTokenExpiresAt
    );
}
