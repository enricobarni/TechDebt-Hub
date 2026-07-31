namespace TechDebtHub.Application.Common.Models
{
    public sealed record PagedResult<T>(
        IReadOnlyList<T> Itens,
        int Pagina,
        int TamanhoPagina,
        int TotalItens,
        int TotalPaginas
    );
}
