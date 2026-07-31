namespace TechDebtHub.Application.Features.Projetos.AtualizarProjeto
{
    public sealed record AtualizarProjetoCommand(
        Guid Id,
        string Nome,
        string Descricao
    );
}