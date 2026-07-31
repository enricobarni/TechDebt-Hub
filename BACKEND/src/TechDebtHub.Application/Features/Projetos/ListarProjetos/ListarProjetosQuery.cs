namespace TechDebtHub.Application.Features.Projetos.ListarProjetos
{
    public sealed record ListarProjetosQuery(
        string? Busca,
        bool? Arquivado,
        int Pagina,
        int TamanhoPagina
    );
}
