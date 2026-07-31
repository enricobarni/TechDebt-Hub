using System;
using System.Globalization;
using System.Text;
using TechDebtHub.Domain.Exceptions;

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

            Nome = PrepararNomeParaExibicao(nome);
            NomeNormalizado = NormalizarNome(nome);
            Descricao = descricao.Trim();
        }

        private void GarantirQueNaoEstaArquivado()
        {
            if (Arquivado)
            {
                throw new DomainException("Não é possível alterar um projeto arquivado.");
            }
        }

        private static string PrepararNomeParaExibicao(string nome)
        {
            var partes = nome.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(' ', partes);
        }

        private static string NormalizarNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                throw new DomainException("O nome do projeto é obrigatório.");
            }

            var nomeDecomposto = nome.Trim().Normalize(NormalizationForm.FormKD);
            var resultado = new StringBuilder(nomeDecomposto.Length);
            var ultimoCaractereFoiEspaco = false;

            foreach (var caractere in nomeDecomposto)
            {
                var categoria = CharUnicodeInfo.GetUnicodeCategory(caractere);

                var ehMarcaDeAcentuacao =
                    categoria
                    is UnicodeCategory.NonSpacingMark
                        or UnicodeCategory.SpacingCombiningMark
                        or UnicodeCategory.EnclosingMark;

                if (ehMarcaDeAcentuacao)
                {
                    continue;
                }

                if (char.IsWhiteSpace(caractere))
                {
                    if (resultado.Length > 0 && !ultimoCaractereFoiEspaco)
                    {
                        resultado.Append(' ');
                        ultimoCaractereFoiEspaco = true;
                    }

                    continue;
                }

                resultado.Append(char.ToUpperInvariant(caractere));
                ultimoCaractereFoiEspaco = false;
            }

            return resultado.ToString().Trim().Normalize(NormalizationForm.FormC);
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
