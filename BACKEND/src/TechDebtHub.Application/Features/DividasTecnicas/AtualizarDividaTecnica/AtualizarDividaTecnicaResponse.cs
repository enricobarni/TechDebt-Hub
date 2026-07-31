using TechDebtHub.Domain.Enums;

namespace TechDebtHub.Application.Features.DividasTecnicas.AtualizarDividaTecnica
{
    public sealed record AtualizarDividaTecnicaResponse(
        Guid Id,
        Guid ProjetoId,
        string Titulo,
        string Descricao,
        CategoriaDivida Categoria,
        StatusDivida Status,
        NivelImpacto Impacto,
        NivelUrgencia Urgencia,
        NivelFrequencia Frequencia,
        NivelEsforco Esforco,
        decimal PontuacaoPrioridade,
        DateTime? DataAtualizacao
    );
}
