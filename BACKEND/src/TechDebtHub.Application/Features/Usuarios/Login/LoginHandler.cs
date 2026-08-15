using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Abstractions.Persistence;
using TechDebtHub.Application.Exceptions;
using TechDebtHub.Application.Interfaces;
using TechDebtHub.Domain.Entities;

namespace TechDebtHub.Application.Features.Usuarios.Login
{
    public sealed class LoginHandler
    {
        private const string CredenciaisInvalidas = "Credenciais inválidas";

        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly ITokenHasher _tokenHasher;

        public LoginHandler(
            IApplicationDbContext context,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator,
            IRefreshTokenGenerator refreshTokenGenerator,
            ITokenHasher tokenHasher
        )
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _refreshTokenGenerator = refreshTokenGenerator;
            _tokenHasher = tokenHasher;
        }

        public async Task<LoginResponse> HandleAsync(
            LoginCommand command,
            CancellationToken cancellationToken
        )
        {
            var emailNormalizado = command.Email.Trim().ToUpperInvariant();

            var usuario = await _context.Usuarios.SingleOrDefaultAsync(
                u => u.EmailNormalizado == emailNormalizado,
                cancellationToken
            );

            if (usuario is null)
            {
                throw new UnauthorizedException(CredenciaisInvalidas);
            }

            var senhaValida = _passwordHasher.Verify(command.Senha, usuario.SenhaHash);

            if (!senhaValida || !usuario.Ativo)
            {
                throw new UnauthorizedException(CredenciaisInvalidas);
            }

            var acessToken = _jwtTokenGenerator.Generate(usuario.Id);

            var (refreshTokenValue, refreshTokenExpiration) = _refreshTokenGenerator.Generate();

            var refreshTokenHash = _tokenHasher.Hash(refreshTokenValue);

            var refreshTokenEntity = new RefreshToken(
                usuario.Id,
                refreshTokenHash,
                refreshTokenExpiration
            );

            _context.RefreshTokens.Add(refreshTokenEntity);

            await _context.SaveChangesAsync(cancellationToken);

            return new LoginResponse(
                acessToken,
                refreshTokenValue,
                "Bearer",
                refreshTokenExpiration
            );
        }
    }
}
