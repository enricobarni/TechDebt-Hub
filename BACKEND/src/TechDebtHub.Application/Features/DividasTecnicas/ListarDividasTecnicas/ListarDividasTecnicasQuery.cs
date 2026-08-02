using TechDebtHub.Domain.Enums;

namespace TechDebtHub.Application.Features.DividasTecnicas.ListarDividasTecnicas
{
    public sealed record ListarDividasTecnicasQuery(
        Guid ProjetoId,
        StatusDivida? Status,
        CategoriaDivida? Categoria,
        bool? Arquivada,
        string? Busca,
        int Pagina,
        int TamanhoPagina
    );
}
