using TechDebtHub.Domain.Enums;

namespace TechDebtHub.Api.Contracts.DividasTecnicas
{
    public sealed record AtualizarDividaTecnicaRequest(
        string Titulo,
        string Descricao,
        CategoriaDivida Categoria,
        NivelImpacto Impacto,
        NivelUrgencia Urgencia,
        NivelFrequencia Frequencia,
        NivelEsforco Esforco
    );
}
