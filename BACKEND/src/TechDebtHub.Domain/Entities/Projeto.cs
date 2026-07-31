using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TechDebtHub.Domain.Exceptions;

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
            ValidarEAtribuirCampos(nome, descricao);

            Id = Guid.NewGuid();
            Nome = nome.Trim();
            Descricao = descricao.Trim();
            DataCriacao = DateTime.UtcNow;
            DataAtualizacao = null;
            Arquivado = false;
        }

        public void AtualizarProjeto(string nome, string descricao)
        {
            ValidarEAtribuirCampos(nome, descricao);
            DataAtualizacao = DateTime.UtcNow;
        }

        private void ValidarEAtribuirCampos(string nome, string descricao)
        {
            ValidarNome(nome);
            ValidarDescricao(descricao);

            Nome = nome.Trim();
            Descricao = descricao.Trim();
        }

        private static void ValidarNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                throw new DomainException("O nome do projeto é obrigatório");
            }
        }

        private static void ValidarDescricao(string descricao)
        {
            if (string.IsNullOrWhiteSpace(descricao))
            {
                throw new DomainException("A descrição do projeto é obrigatória");
            }
        }

        public void Arquivar()
        {
            if (Arquivado)
            {
                throw new DomainException("Projeto já está arquivado");
            }

            Arquivado = true;
            DataAtualizacao = DateTime.UtcNow;
        }
    }
}
