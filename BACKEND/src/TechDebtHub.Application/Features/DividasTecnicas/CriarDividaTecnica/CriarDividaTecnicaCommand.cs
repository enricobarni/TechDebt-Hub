using TechDebtHub.Domain.Enums;

namespace TechDebtHub.Application.Features.DividasTecnicas.CriarDividaTecnica
{
    public sealed record CriarDividaTecnicaCommand(
        Guid ProjetoId,
        string Titulo,
        string Descricao,
        CategoriaDivida Categoria,
        NivelImpacto Impacto,
        NivelUrgencia Urgencia,
        NivelFrequencia Frequencia,
        NivelEsforco Esforco
    );
}
