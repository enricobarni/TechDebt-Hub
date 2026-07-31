namespace TechDebtHub.Application.Features.Projetos.AtualizarProjeto
{
    public sealed record AtualizarProjetoResponse(
        Guid Id,
        string Nome,
        string Descricao,
        DateTime? DataAtualizacao
    );
}
