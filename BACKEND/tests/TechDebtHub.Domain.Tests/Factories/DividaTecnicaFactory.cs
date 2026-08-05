using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechDebtHub.Domain.Entities;
using TechDebtHub.Domain.Enums;

namespace TechDebtHub.Domain.Tests.Factories
{
    public static class DividaTecnicaFactory
    {
        public static DividaTecnica Criar(
            Guid? projetoId = null,
            string titulo = "Consulta sem paginação",
            string descricao = "A consulta retorna todos os registros.",
            CategoriaDivida categoria = CategoriaDivida.Performance,
            NivelImpacto impacto = NivelImpacto.Alto,
            NivelUrgencia urgencia = NivelUrgencia.Media,
            NivelFrequencia frequencia = NivelFrequencia.Constante,
            NivelEsforco esforco = NivelEsforco.Medio
        )
        {
            return DividaTecnica.Criar(
                projetoId ?? Guid.NewGuid(),
                titulo,
                descricao,
                categoria,
                impacto,
                urgencia,
                frequencia,
                esforco
            );
        }
    }
}
