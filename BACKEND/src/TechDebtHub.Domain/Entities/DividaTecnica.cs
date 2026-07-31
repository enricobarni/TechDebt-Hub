using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechDebtHub.Domain.Enums;
using TechDebtHub.Domain.Exceptions;

namespace TechDebtHub.Domain.Entities
{
    public class DividaTecnica
    {
        public Guid Id { get; private set; }
        public string Titulo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public CategoriaDivida Categoria { get; private set; }
        public StatusDivida Status { get; private set; }
        public NivelImpacto Impacto { get; private set; }
        public NivelUrgencia Urgencia { get; private set; }
        public NivelFrequencia Frequencia { get; private set; }
        public NivelEsforco Esforco { get; private set; }
        public decimal PontuacaoPrioridade { get; private set; }
        public Guid ProjetoId { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public DateTime? DataAtualizacao { get; private set; }
        public DateTime? DataResolucao { get; private set; }

        private DividaTecnica() { }

        private DividaTecnica(
            Guid projetoId,
            string titulo,
            string descricao,
            CategoriaDivida categoria,
            NivelImpacto impacto,
            NivelUrgencia urgencia,
            NivelFrequencia frequencia,
            NivelEsforco esforco
        )
        {
            Id = Guid.NewGuid();
            ProjetoId = projetoId;
            Titulo = titulo;
            Descricao = descricao;
            Categoria = categoria;
            Impacto = impacto;
            Urgencia = urgencia;
            Frequencia = frequencia;
            Esforco = esforco;
            Status = StatusDivida.Aberta;
            PontuacaoPrioridade = CalcularPrioridade();
            DataCriacao = DateTime.UtcNow;
        }

        public static DividaTecnica Criar(
            Guid projetoId,
            string titulo,
            string descricao,
            CategoriaDivida categoria,
            NivelImpacto impacto,
            NivelUrgencia urgencia,
            NivelFrequencia frequencia,
            NivelEsforco esforco
        )
        {
            ValidarProjeto(projetoId);
            ValidarTitulo(titulo);
            ValidarDescricao(descricao);
            ValidarEnum(categoria, nameof(categoria));
            ValidarEnum(impacto, nameof(impacto));
            ValidarEnum(urgencia, nameof(urgencia));
            ValidarEnum(frequencia, nameof(frequencia));
            ValidarEnum(esforco, nameof(esforco));

            return new DividaTecnica(
                projetoId,
                titulo.Trim(),
                descricao.Trim(),
                categoria,
                impacto,
                urgencia,
                frequencia,
                esforco
            );
        }

        public void AtualizarDividaTecnica(string titulo, string descricao)
        {
            ValidarEAtribuirCampos(titulo, descricao);
            DataAtualizacao = DateTime.UtcNow;
        }

        private void ValidarEAtribuirCampos(string titulo, string descricao)
        {
            ValidarTitulo(titulo);
            ValidarDescricao(descricao);

            Titulo = titulo.Trim();
            Descricao = descricao.Trim();
        }

        private static void ValidarProjeto(Guid projetoId)
        {
            if (projetoId == Guid.Empty)
            {
                throw new DomainException("O projeto é obrigatório");
            }
        }

        private static void ValidarTitulo(string titulo)
        {
            if (string.IsNullOrWhiteSpace(titulo))
            {
                throw new DomainException("O título da dívida técnica é obrigatório");
            }
            if (titulo.Trim().Length > 120)
            {
                throw new DomainException("O título deve possuir no máximo 120 caracteres.");
            }
        }

        private static void ValidarDescricao(string descricao)
        {
            if (string.IsNullOrWhiteSpace(descricao))
            {
                throw new DomainException("A descrição da Dívida Técnica é obrigatória");
            }
            if (descricao.Trim().Length > 2000)
            {
                throw new DomainException("A descrição deve possuir no máximo 2000 caracteres.");
            }
        }

        private static void ValidarEnum<TEnum>(TEnum valor, string nomeParametro)
            where TEnum : struct, Enum
        {
            if (!Enum.IsDefined(valor))
            {
                throw new DomainException($"O valor informado para {nomeParametro} é inválido.");
            }
        }

        private decimal CalcularPrioridade()
        {
            return PontuacaoPrioridade =
                (decimal)Impacto * (decimal)Urgencia * (decimal)Frequencia / (decimal)Esforco;
        }
    }
}
