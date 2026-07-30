using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechDebtHub.Domain.Enums;

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
        public NivelEsforco Esforco { get; private set; }
        public NivelFrequencia Frequencia { get; private set; }
        public NivelUrgencia Urgencia { get; private set; }
        public decimal PontuacaoPrioridade { get; private set; }
        public Guid ProjetoId { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public DateTime? DataAtualizacao { get; private set; }
        public DateTime? DataResolucao { get; private set; }

        private DividaTecnica() { }

        public DividaTecnica(
            Guid projetoId,
            string titulo,
            string descricao,
            CategoriaDivida categoria,
            NivelImpacto impacto,
            NivelEsforco esforco,
            NivelFrequencia frequencia,
            NivelUrgencia urgencia
        )
        {
            if (projetoId == Guid.Empty)
            {
                throw new ArgumentException("O projeto é obrigatório", nameof(projetoId));
            }

            Id = Guid.NewGuid();
            ProjetoId = projetoId;

            AlterarTitulo(titulo);
            AlterarDescricao(descricao);

            Categoria = categoria;
            Impacto = impacto;
            Esforco = esforco;
            Frequencia = frequencia;
            Urgencia = urgencia;

            Status = StatusDivida.Aberta;
            DataCriacao = DateTime.UtcNow;

            RecalcularPrioridade();
        }

        public void AlterarTitulo(string titulo)
        {
            if (string.IsNullOrWhiteSpace(titulo))
            {
                throw new ArgumentException(
                    "O título da divida técnica é obrigatório",
                    nameof(titulo)
                );
            }

            Titulo = titulo.Trim();
            DataAtualizacao = DateTime.UtcNow;
        }

        public void AlterarDescricao(string descricao)
        {
            if (string.IsNullOrWhiteSpace(descricao))
            {
                throw new ArgumentException("A descrição da divida técnica é obrigatória");
            }

            Descricao = descricao.Trim();
            DataAtualizacao = DateTime.UtcNow;
        }

        private void RecalcularPrioridade()
        {
            PontuacaoPrioridade =
                (decimal)Impacto * (decimal)Urgencia * (decimal)Frequencia / (decimal)Esforco;
        }

        public void Resolver()
        {
            if (Status == StatusDivida.Resolvida)
            {
                throw new ArgumentException("A divida técnica ja foi resolvida");
            }

            Status = StatusDivida.Resolvida;
            DataResolucao = DateTime.UtcNow;
            DataAtualizacao = DateTime.UtcNow;
        }
    }
}
