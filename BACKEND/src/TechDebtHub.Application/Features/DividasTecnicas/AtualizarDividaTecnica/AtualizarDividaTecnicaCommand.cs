using TechDebtHub.Domain.Enums;

namespace TechDebtHub.Application.Features.DividasTecnicas.AtualizarDividaTecnica
{
    public sealed record AtualizarDividaTecnicaCommand(
        Guid Id,
        string Titulo,
        string Descricao,
        CategoriaDivida Categoria,
        NivelImpacto Impacto,
        NivelUrgencia Urgencia,
        NivelFrequencia Frequencia,
        NivelEsforco Esforco
    );
}
