using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Abstractions.Persistence;
using TechDebtHub.Application.Exceptions;
using TechDebtHub.Application.Interfaces;
using TechDebtHub.Domain.Entities;

namespace TechDebtHub.Application.Features.Usuarios.RefreshTokens
{
    public sealed class RefreshTokensHandler
    {
        private const string RefreshTokenInvalido = "Refresh token inválido";

        private readonly IApplicationDbContext _context;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly ITokenHasher _tokenHasher;

        public RefreshTokensHandler(
            IApplicationDbContext context,
            IJwtTokenGenerator jwtTokenGenerator,
            IRefreshTokenGenerator refreshTokenGenerator,
            ITokenHasher tokenHasher
        )
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
            _refreshTokenGenerator = refreshTokenGenerator;
            _tokenHasher = tokenHasher;
        }

        public async Task<RefreshTokensResponse> HandleAsync(
            RefreshTokensCommand command,
            CancellationToken cancellationToken
        )
        {
            var tokenHash = _tokenHasher.Hash(command.RefreshToken);

            var refreshToken = await _context.RefreshTokens.SingleOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken
            );

            if (refreshToken is null || !refreshToken.EstaAtivo)
            {
                throw new UnauthorizedException(RefreshTokenInvalido);
            }

            var usuario = await _context.Usuarios.SingleOrDefaultAsync(
                usuario => usuario.Id == refreshToken.UsuarioId,
                cancellationToken
            );

            if (usuario is null || !usuario.Ativo)
            {
                throw new UnauthorizedException(RefreshTokenInvalido);
            }

            refreshToken.Revogar();

            var accessToken = _jwtTokenGenerator.Generate(usuario.Id);

            var (novoRefreshTokenValue, novoRefreshTokenExpiration) =
                _refreshTokenGenerator.Generate();

            var novoRefreshTokenHash = _tokenHasher.Hash(novoRefreshTokenValue);

            var novoRefreshToken = new RefreshToken(
                usuario.Id,
                novoRefreshTokenHash,
                novoRefreshTokenExpiration
            );

            _context.RefreshTokens.Add(novoRefreshToken);

            await _context.SaveChangesAsync(cancellationToken);

            return new RefreshTokensResponse(
                accessToken,
                novoRefreshTokenValue,
                "Bearer",
                novoRefreshTokenExpiration
            );
        }
    }
}
