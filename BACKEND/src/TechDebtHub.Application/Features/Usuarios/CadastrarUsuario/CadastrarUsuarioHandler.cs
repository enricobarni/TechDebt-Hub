using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Abstractions.Persistence;
using TechDebtHub.Application.Exceptions;
using TechDebtHub.Application.Interfaces;
using TechDebtHub.Domain.Common;
using TechDebtHub.Domain.Entities;

namespace TechDebtHub.Application.Features.Usuarios.CadastrarUsuario
{
    public sealed class CadastrarUsuarioHandler
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public CadastrarUsuarioHandler(
            IApplicationDbContext context,
            IPasswordHasher passwordHasher
        )
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<CadastrarUsuarioResponse> HandleAsync(
            CadastrarUsuarioCommand command,
            CancellationToken cancellationToken
        )
        {
            SenhaValidator.Validar(command.Senha);

            var senhaHash = _passwordHasher.Hash(command.Senha);

            var usuario = new Usuario(command.Nome, command.Email, senhaHash);

            var emailExiste = await _context.Usuarios.AnyAsync(
                u => u.EmailNormalizado == usuario.EmailNormalizado,
                cancellationToken
            );

            if (emailExiste)
            {
                throw new ConflictException("Já existe um usuário cadastrado com esse e-mail");
            }

            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync(cancellationToken);

            return new CadastrarUsuarioResponse(usuario.Id, usuario.Nome, usuario.Email);
        }
    }
}
