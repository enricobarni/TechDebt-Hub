using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Abstractions.Persistence;
using TechDebtHub.Application.Exceptions;
using TechDebtHub.Application.Interfaces;

namespace TechDebtHub.Application.Features.Usuarios.BuscarUsuarioAtual
{
    public sealed class BuscarUsuarioAtualHandler
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUser _currentUser;

        public BuscarUsuarioAtualHandler(IApplicationDbContext context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<BuscarUsuarioAtualResponse> HandleAsync(
            BuscarUsuarioAtualQuery query,
            CancellationToken cancellationToken
        )
        {
            var usuarioId = _currentUser.UsuarioId;

            if (usuarioId is null)
            {
                throw new UnauthorizedException("Usuário não autenticado");
            }

            var usuario = await _context
                .Usuarios.AsNoTracking()
                .Where(usuario => usuario.Id == usuarioId.Value && usuario.Ativo)
                .Select(usuario => new BuscarUsuarioAtualResponse(
                    usuario.Id,
                    usuario.Nome,
                    usuario.Email,
                    usuario.EmailConfirmado,
                    usuario.DataCriacao
                ))
                .FirstOrDefaultAsync(cancellationToken);

            if (usuario is null)
            {
                throw new UnauthorizedException("Usuário não autorizado");
            }

            return usuario;
        }
    }
}
