using System;
using System.Globalization;
using System.Text;
using TechDebtHub.Domain.Exceptions;
using TechDebtHub.Domain.Common;

namespace TechDebtHub.Domain.Entities
{
    public class Projeto
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string NomeNormalizado { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public DateTime DataCriacao { get; private set; }
        public DateTime? DataAtualizacao { get; private set; }
        public bool Arquivado { get; private set; }

        private Projeto() { }

        private Projeto(string nome, string descricao)
        {
            Id = Guid.NewGuid();
            DataCriacao = DateTime.UtcNow;
            DataAtualizacao = null;
            Arquivado = false;

            SetarCampos(nome, descricao);
        }

        public static Projeto Criar(string nome, string descricao)
        {
            return new Projeto(nome, descricao);
        }

        public void AtualizarProjeto(string nome, string descricao)
        {
            GarantirQueNaoEstaArquivado();

            SetarCampos(nome, descricao);
            DataAtualizacao = DateTime.UtcNow;
        }

        public void Arquivar()
        {
            if (Arquivado)
            {
                throw new DomainException("O projeto já está arquivado.");
            }

            Arquivado = true;
            DataAtualizacao = DateTime.UtcNow;
        }

        private void SetarCampos(string nome, string descricao)
        {
            ValidarNome(nome);
            ValidarDescricao(descricao);

            Nome = TextNormalizer.PrepararParaExibicao(nome);
            NomeNormalizado = TextNormalizer.NormalizarParaComparacao(nome);
            Descricao = descricao.Trim();
        }

        private void GarantirQueNaoEstaArquivado()
        {
            if (Arquivado)
            {
                throw new DomainException("Não é possível alterar um projeto arquivado.");
            }
        }

        private static void ValidarNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                throw new DomainException("O nome do projeto é obrigatório.");
            }

            if (nome.Trim().Length > 100)
            {
                throw new DomainException(
                    "O nome do projeto deve possuir no máximo 100 caracteres."
                );
            }
        }

        private static void ValidarDescricao(string descricao)
        {
            if (string.IsNullOrWhiteSpace(descricao))
            {
                throw new DomainException("A descrição do projeto é obrigatória.");
            }

            if (descricao.Trim().Length > 1000)
            {
                throw new DomainException(
                    "A descrição do projeto deve possuir no máximo 1000 caracteres."
                );
            }
        }
    }
}
