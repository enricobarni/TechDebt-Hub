using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using TechDebtHub.Domain.Common;
using TechDebtHub.Domain.Exceptions;

namespace TechDebtHub.Domain.Entities
{
    public class Usuario
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; } = null!;
        public string Email { get; private set; } = null!;
        public bool EmailConfirmado { get; private set; }
        public string EmailNormalizado { get; private set; } = null!;
        public string SenhaHash { get; private set; } = null!;
        public bool Ativo { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public DateTime? DataAtualizacao { get; private set; }
        public DateTime? DataConfirmacaoEmail { get; private set; }

        private Usuario() { }

        public Usuario(string nome, string email, string SenhaHash)
        {
            Id = Guid.NewGuid();

            DefinirNome(nome);
            DefinirEmail(email);
            DefinirSenhaHash(SenhaHash);

            Ativo = true;
            EmailConfirmado = false;
            DataCriacao = DateTime.UtcNow;
        }

        public void AtualizarNome(string nome)
        {
            ValidarAtivo();

            DefinirNome(nome);

            DataAtualizacao = DateTime.UtcNow;
        }

        public void AtualizarEmail(string email)
        {
            ValidarAtivo();

            DefinirEmail(email);

            EmailConfirmado = false;
            DataConfirmacaoEmail = null;

            DataAtualizacao = DateTime.UtcNow;
        }

        public void AtualizarSenhaHash(string SenhaHash)
        {
            ValidarAtivo();

            DefinirSenhaHash(SenhaHash);

            DataAtualizacao = DateTime.UtcNow;
        }

        public void ConfirmarEmail()
        {
            if (EmailConfirmado)
            {
                throw new DomainException("O e-mail do usuário já está Confirmado");
            }

            EmailConfirmado = true;
            DataConfirmacaoEmail = DateTime.UtcNow;
            DataAtualizacao = DateTime.UtcNow;
        }

        public void Desativar()
        {
            if (!Ativo)
            {
                throw new DomainException("O usuário já está desativado");
            }

            Ativo = false;
            DataAtualizacao = DateTime.UtcNow;
        }

        private void DefinirNome(string nome)
        {
            int limiteDeCaracter = 120;
            var nomePreparado = TextNormalizer.PrepararParaExibicao(nome);

            if (string.IsNullOrEmpty(nome))
            {
                throw new DomainException("O nome do usuário é obrigatório");
            }
            if (nomePreparado.Length > limiteDeCaracter)
            {
                throw new DomainException(
                    "O nome de usuário deve possuir no máximo 120 caracteres"
                );
            }

            Nome = nomePreparado;
        }

        private void DefinirEmail(string email)
        {
            int limiteDeCaracter = 254;

            if (string.IsNullOrEmpty(email))
            {
                throw new DomainException("O e-mail é obrigatório");
            }

            var emailPreparado = email.Trim();

            if (emailPreparado.Length > limiteDeCaracter)
            {
                throw new DomainException("O e-mail deve possuir no máximo 254 caracteres");
            }

            ValidarFormatoEmail(emailPreparado);

            Email = emailPreparado;

            EmailNormalizado = emailPreparado.ToUpperInvariant();
        }

        private static void ValidarFormatoEmail(string emailPreparado)
        {
            try
            {
                var endereco = new MailAddress(emailPreparado);

                var enderecoBate = string.Equals(
                    endereco.Address,
                    emailPreparado,
                    StringComparison.OrdinalIgnoreCase
                );

                if (!enderecoBate || !endereco.Host.Contains('.'))
                {
                    throw new DomainException("O e-mail informado é inválido");
                }
            }
            catch (FormatException)
            {
                throw new DomainException("O e-mail informado é inválido");
            }
        }

        private void DefinirSenhaHash(string senhaHash)
        {
            if (string.IsNullOrEmpty(senhaHash))
            {
                throw new DomainException("O hash da senha é obrigatório");
            }

            SenhaHash = senhaHash;
        }

        private void ValidarAtivo()
        {
            if (!Ativo)
            {
                throw new DomainException("Não é possível alterar um usuário desativado");
            }
        }
    }
}
