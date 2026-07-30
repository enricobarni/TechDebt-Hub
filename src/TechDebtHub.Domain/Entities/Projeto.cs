using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechDebtHub.Domain.Entities
{
    public class Projeto
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public DateTime DataCriacao { get; private set; }
        public DateTime? DataAtualizacao { get; private set; }
        public bool Arquivado { get; private set; }

        private Projeto() { }

        public Projeto(string nome, string descricao)
        {
            AlterarNome(nome);
            AlterarDescricao(descricao);

            Id = Guid.NewGuid();
            Nome = nome.Trim();
            Descricao = descricao.Trim();
            DataCriacao = DateTime.UtcNow;
            DataAtualizacao = null;
            Arquivado = false;
        }

        public void AlterarNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                throw new ArgumentException("O nome do projeto é obrigatório", nameof(nome));
            }

            Nome = nome.Trim();
            DataAtualizacao = DateTime.UtcNow;
        }

        public void AlterarDescricao(string descricao)
        {
            if (string.IsNullOrWhiteSpace(descricao))
            {
                throw new ArgumentException(
                    "A descrição do projeto é obrigatória",
                    nameof(descricao)
                );
            }

            Descricao = descricao.Trim();
            DataAtualizacao = DateTime.UtcNow;
        }

        public void Arquivar()
        {
            Arquivado = true;
            DataAtualizacao = DateTime.UtcNow;
        }
    }
}
