using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Abstractions.Persistence;
using TechDebtHub.Application.Exceptions;
using TechDebtHub.Application.Interfaces;

namespace TechDebtHub.Application.Features.Usuarios.ConfirmarEmail
{
    public sealed class ConfirmarEmailHandler
    {
        private readonly IApplicationDbContext _context;
        private readonly IEmailConfirmationCodeHasher _codeHasher;

        public ConfirmarEmailHandler(
            IApplicationDbContext context,
            IEmailConfirmationCodeHasher codeHasher
        )
        {
            _context = context;
            _codeHasher = codeHasher;
        }

        public async Task<ConfirmarEmailResponse> HandleAsync(
            ConfirmarEmailCommand command,
            CancellationToken cancellationToken
        )
        {
            var emailNormalizado = command.Email.Trim().ToUpperInvariant();

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(
                u => u.EmailNormalizado == emailNormalizado,
                cancellationToken
            );

            if (usuario is null)
            {
                throw new NotFoundException("Usuário não encontrado");
            }

            if (usuario.EmailConfirmado)
            {
                throw new ConflictException("O e-mail deste usuário já foi confirmado");
            }

            var confirmacao = await _context
                .CodigoConfirmacaoEmails.Where(c => c.UsuarioId == usuario.Id)
                .OrderByDescending(c => c.DataCriacao)
                .FirstOrDefaultAsync(cancellationToken);

            if (confirmacao is null)
            {
                throw new NotFoundException(
                    "Não existe um código de confirmação para este usuário"
                );
            }

            if (!confirmacao.EstaAtivo)
            {
                throw new ConflictException("O código de confirmação não está mais ativo");
            }

            var codigoHash = _codeHasher.Hash(usuario.Id, command.Codigo);

            var hashEsperado = Convert.FromHexString(confirmacao.CodigoHash);

            var hashRecebido = Convert.FromHexString(codigoHash);

            var codigoValido = CryptographicOperations.FixedTimeEquals(hashEsperado, hashRecebido);

            if (!codigoValido)
            {
                confirmacao.RegistrarTentativaFalha();

                await _context.SaveChangesAsync(cancellationToken);

                throw new ConflictException("Código de confirmação inválido");
            }

            usuario.ConfirmarEmail();

            confirmacao.MarcarComoUtilizado();

            await _context.SaveChangesAsync(cancellationToken);

            return new ConfirmarEmailResponse("E-mail confirmado com sucesso");
        }
    }
}
