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
        private readonly IEmailConfirmationCodeGenerator _codeGenerator;
        private readonly IEmailConfirmationCodeHasher _codeHasher;

        public CadastrarUsuarioHandler(
            IApplicationDbContext context,
            IPasswordHasher passwordHasher,
            IEmailConfirmationCodeGenerator codeGenerator,
            IEmailConfirmationCodeHasher codeHasher
        )
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _codeGenerator = codeGenerator;
            _codeHasher = codeHasher;
        }

        public async Task<CadastrarUsuarioResponse> HandleAsync(
            CadastrarUsuarioCommand command,
            CancellationToken cancellationToken
        )
        {
            SenhaValidator.Validar(command.Senha);

            var senhaHash = _passwordHasher.Hash(command.Senha);

            var usuario = new Usuario(command.Nome, command.Email, senhaHash);

            var (codigo, dataExpiracao, maximoTentativas) = _codeGenerator.Generate();

            var codigoHash = _codeHasher.Hash(usuario.Id, codigo);

            var codigoConfirmacao = new CodigoConfirmacaoEmail(
                usuario.Id,
                codigoHash,
                dataExpiracao,
                maximoTentativas
            );

            var emailExiste = await _context.Usuarios.AnyAsync(
                u => u.EmailNormalizado == usuario.EmailNormalizado,
                cancellationToken
            );

            if (emailExiste)
            {
                throw new ConflictException("Já existe um usuário cadastrado com esse e-mail");
            }

            _context.Usuarios.Add(usuario);
            _context.CodigoConfirmacaoEmails.Add(codigoConfirmacao);

            await _context.SaveChangesAsync(cancellationToken);

            return new CadastrarUsuarioResponse(usuario.Id, usuario.Nome, usuario.Email);
        }
    }
}
