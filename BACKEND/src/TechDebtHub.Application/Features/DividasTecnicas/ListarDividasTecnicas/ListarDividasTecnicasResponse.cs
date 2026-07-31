using TechDebtHub.Domain.Enums;

namespace TechDebtHub.Application.Features.DividasTecnicas.ListarDividasTecnicas
{
    public sealed record ListarDividasTecnicasResponse(
        Guid Id,
        string Titulo,
        CategoriaDivida Categoria,
        StatusDivida Status,
        NivelImpacto Impacto,
        NivelUrgencia Urgencia,
        NivelFrequencia Frequencia,
        NivelEsforco Esforco,
        decimal PontuacaoPrioridade,
        DateTime DataCriacao,
        DateTime? DataAtualizacao
    );
}
