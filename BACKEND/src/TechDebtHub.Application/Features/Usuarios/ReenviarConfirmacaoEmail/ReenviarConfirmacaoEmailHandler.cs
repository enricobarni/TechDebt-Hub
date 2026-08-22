using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Abstractions.Persistence;
using TechDebtHub.Application.Interfaces;
using TechDebtHub.Domain.Entities;

namespace TechDebtHub.Application.Features.Usuarios.ReenviarConfirmacaoEmail
{
    public sealed class ReenviarConfirmacaoEmailHandler
    {
        private const string MensagemResposta =
            "Se houver uma conta pendente de confirmação para este e-mail, um novo código será enviado";
        private readonly IApplicationDbContext _context;
        private readonly IEmailConfirmationCodeGenerator _codeGenerator;
        private readonly IEmailConfirmationCodeHasher _codeHasher;
        private readonly IEmailSender _emailSender;

        public ReenviarConfirmacaoEmailHandler(
            IApplicationDbContext context,
            IEmailConfirmationCodeGenerator codeGenerator,
            IEmailConfirmationCodeHasher codeHasher,
            IEmailSender emailSender
        )
        {
            _context = context;
            _codeGenerator = codeGenerator;
            _codeHasher = codeHasher;
            _emailSender = emailSender;
        }

        public async Task<ReenviarConfirmacaoEmailResponse> HandleAsync(
            ReenviarConfirmacaoEmailCommand command,
            CancellationToken cancellationToken
        )
        {
            var emailNormalizado = command.Email.Trim().ToUpperInvariant();

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(
                u => u.EmailNormalizado == emailNormalizado,
                cancellationToken
            );

            if (usuario is null || usuario.EmailConfirmado)
            {
                return new ReenviarConfirmacaoEmailResponse(MensagemResposta);
            }

            var codigoAnterior = await _context
                .CodigoConfirmacaoEmails.Where(c => c.UsuarioId == usuario.Id)
                .OrderByDescending(c => c.DataCriacao)
                .FirstOrDefaultAsync(cancellationToken);

            if (
                codigoAnterior is not null
                && !codigoAnterior.FoiRevogado
                && !codigoAnterior.FoiUtilizado
            )
            {
                codigoAnterior.Revogar();
            }

            var (codigo, dataExpiracao, maximoTentativas) = _codeGenerator.Generate();

            var codigoHash = _codeHasher.Hash(usuario.Id, codigo);

            var novaConfirmacao = new CodigoConfirmacaoEmail(
                usuario.Id,
                codigoHash,
                dataExpiracao,
                maximoTentativas
            );

            _context.CodigoConfirmacaoEmails.Add(novaConfirmacao);
            await _context.SaveChangesAsync(cancellationToken);

            var nome = WebUtility.HtmlEncode(usuario.Nome);

            var html = $"""
                    <h1>Confirme seu e-mail</h1>

                    <p>Olá, {nome}</p>
                    <p>Seu novo código de verificação é:</p>

                    <h2>{codigo}</h2>

                    <p>Esse código expira em breve.</p>
                    <p>Se você não solicitou esse código, ignore este e-mail.</p>
                """;

            await _emailSender.SendAsync(
                usuario.Email,
                "Novo código de confirmação — TechDebtHub",
                html,
                cancellationToken
            );

            return new ReenviarConfirmacaoEmailResponse(MensagemResposta);
        }
    }
}
