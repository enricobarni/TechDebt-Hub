using TechDebtHub.Domain.Enums;

namespace TechDebtHub.Application.Features.DividasTecnicas.CriarDividaTecnica
{
    public sealed record CriarDividaTecnicaResponse(
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
        DateTime DataCriacao
    );
}
