namespace TechDebtHub.Application.Features.Projetos.ListarProjetos
{
    public sealed record ProjetoResumoResponse(
        Guid Id,
        string Nome,
        string Descricao,
        DateTime DataCriacao,
        DateTime? DataAtualizacao,
        bool Arquivado
    );
}