using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechDebtHub.Domain.Common;
using TechDebtHub.Domain.Enums;
using TechDebtHub.Domain.Exceptions;

namespace TechDebtHub.Domain.Entities
{
    public class DividaTecnica
    {
        public Guid Id { get; private set; }
        public Guid ProjetoId { get; private set; }
        public string Titulo { get; private set; } = string.Empty;
        public string TituloNormalizado { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public CategoriaDivida Categoria { get; private set; }
        public StatusDivida Status { get; private set; }
        public NivelImpacto Impacto { get; private set; }
        public NivelUrgencia Urgencia { get; private set; }
        public NivelFrequencia Frequencia { get; private set; }
        public NivelEsforco Esforco { get; private set; }
        public decimal PontuacaoPrioridade { get; private set; }
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
            ValidarProjeto(projetoId);

            Id = Guid.NewGuid();
            ProjetoId = projetoId;
            Status = StatusDivida.Aberta;
            DataCriacao = DateTime.UtcNow;

            SetarCampos(titulo, descricao, categoria, impacto, urgencia, frequencia, esforco);
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
            return new DividaTecnica(
                projetoId,
                titulo,
                descricao,
                categoria,
                impacto,
                urgencia,
                frequencia,
                esforco
            );
        }

        public void AtualizarDividaTecnica(
            string titulo,
            string descricao,
            CategoriaDivida categoria,
            NivelImpacto impacto,
            NivelUrgencia urgencia,
            NivelFrequencia frequencia,
            NivelEsforco esforco
        )
        {
            if (Status == StatusDivida.Resolvida || Status == StatusDivida.Arquivada)
            {
                throw new DomainException(
                    $"Não é possível alterar uma dívida técnica com o status '{Status}'"
                );
            }

            SetarCampos(titulo, descricao, categoria, impacto, urgencia, frequencia, esforco);
            DataAtualizacao = DateTime.UtcNow;
        }

        private void SetarCampos(
            string titulo,
            string descricao,
            CategoriaDivida categoria,
            NivelImpacto impacto,
            NivelUrgencia urgencia,
            NivelFrequencia frequencia,
            NivelEsforco esforco
        )
        {
            ValidarTitulo(titulo);
            ValidarDescricao(descricao);
            ValidarEnum(categoria, nameof(categoria));
            ValidarEnum(impacto, nameof(impacto));
            ValidarEnum(urgencia, nameof(urgencia));
            ValidarEnum(frequencia, nameof(frequencia));
            ValidarEnum(esforco, nameof(esforco));

            Titulo = TextNormalizer.PrepararParaExibicao(titulo);
            TituloNormalizado = TextNormalizer.NormalizarParaComparacao(titulo);
            Descricao = descricao.Trim();
            Categoria = categoria;
            Impacto = impacto;
            Urgencia = urgencia;
            Frequencia = frequencia;
            Esforco = esforco;

            PontuacaoPrioridade = CalcularPrioridade();
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
            var valorEsforco = (decimal)Esforco;

            if (valorEsforco == 0)
            {
                throw new DomainException("O nível de esforço não pode ser zero");
            }

            return (decimal)Impacto * (decimal)Urgencia * (decimal)Frequencia / (decimal)Esforco;
        }

        public void AlterarStatus(StatusDivida novoStatus)
        {
            ValidarEnum(novoStatus, nameof(novoStatus));

            if (Status == novoStatus)
            {
                throw new DomainException("A dívida técnica já está nesse status");
            }

            if (!TransicaoPermitida(Status, novoStatus))
            {
                throw new DomainException(
                    $"Não é permitido alterar o status de {Status} para {novoStatus}"
                );
            }

            if (novoStatus == StatusDivida.Resolvida)
            {
                Resolver();
                return;
            }

            Status = novoStatus;
            DataAtualizacao = DateTime.UtcNow;
        }

        private static bool TransicaoPermitida(StatusDivida statusAtual, StatusDivida novoStatus)
        {
            return statusAtual switch
            {
                StatusDivida.Aberta => novoStatus
                    is StatusDivida.EmAnalise
                        or StatusDivida.Aceita
                        or StatusDivida.Arquivada,

                StatusDivida.EmAnalise => novoStatus
                    is StatusDivida.Planejada
                        or StatusDivida.Aceita
                        or StatusDivida.Arquivada,

                StatusDivida.Planejada => novoStatus
                    is StatusDivida.EmAndamento
                        or StatusDivida.Aceita
                        or StatusDivida.Arquivada,

                StatusDivida.EmAndamento => novoStatus
                    is StatusDivida.Resolvida
                        or StatusDivida.Arquivada,

                StatusDivida.Resolvida => false,

                StatusDivida.Aceita => novoStatus == StatusDivida.Arquivada,

                StatusDivida.Arquivada => false,

                _ => false,
            };
        }

        private void Resolver()
        {
            Status = StatusDivida.Resolvida;
            DataResolucao = DateTime.UtcNow;
            DataAtualizacao = DateTime.UtcNow;
        }
    }
}
