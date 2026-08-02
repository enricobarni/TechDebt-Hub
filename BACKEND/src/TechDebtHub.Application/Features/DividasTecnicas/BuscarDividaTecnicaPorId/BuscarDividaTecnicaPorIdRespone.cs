using TechDebtHub.Domain.Enums;

namespace TechDebtHub.Application.Features.DividasTecnicas.BuscarDividaTecnicaPorId
{
    public sealed record BuscarDividaTecnicaPorIdResponse(
        Guid Id,
        Guid ProjetoId,
        string Titulo,
        string Descricao,
        CategoriaDivida Categoria,
        StatusDivida Status,
        bool Arquivada,
        NivelImpacto Impacto,
        NivelUrgencia Urgencia,
        NivelFrequencia Frequencia,
        NivelEsforco Esforco,
        decimal PontuacaoPrioridade,
        DateTime DataCriacao,
        DateTime? DataAtualizacao,
        DateTime? DataResolucao
    );
}
