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

        private static void ValidarEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("O e-mail é obrigatório");
            }

            if (email.Length > 254)
            {
                throw new ArgumentException("O e-mail deve possuir no máximo 254 caracteres");
            }

            try
            {
                var endereco = new MailAddress(email);

                if (
                    !string.Equals(
                        endereco.Address,
                        email.Trim(),
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    throw new ArgumentException("O e-mail informado é inválido");
                }

                var dominio = endereco.Host;

                if (!dominio.Contains('.'))
                {
                    throw new ArgumentException("O e-mail informado é inválido");
                }
            }
            catch (FormatException)
            {
                throw new ArgumentException("O e-mail informado é inválido");
            }
        }

        private static void ValidarSenha(string senha)
        {
            int tamanhoMinimoSenha = 8;
            int tamanhoMaximoSenha = 128;

            if (string.IsNullOrWhiteSpace(senha))
            {
                throw new ArgumentException("A senha é obrigatória");
            }

            if (senha.Length < tamanhoMinimoSenha)
            {
                throw new ArgumentException("A senha deve possuir pelo menos 8 caracteres");
            }

            if (senha.Length > tamanhoMaximoSenha)
            {
                throw new ArgumentException("A senha deve possuir no máximo 128 caracteres");
            }

            if (!Regex.IsMatch(senha, @"[a-z]"))
            {
                throw new ArgumentException("A senha deve possuir pelo menos uma letra minúscula");
            }

            if (!Regex.IsMatch(senha, @"[A-Z]"))
            {
                throw new ArgumentException("A senha deve possuir pelo menos uma letra maiúscula");
            }

            if (!Regex.IsMatch(senha, @"[0-9]"))
            {
                throw new ArgumentException("A senha deve possuir pelo menos um número");
            }

            if (!Regex.IsMatch(senha, @"[^a-zA-Z0-9]"))
            {
                throw new ArgumentException(
                    "A senha deve possuir pelo menos um caractere especial"
                );
            }
        }
    }
}
