using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Exceptions;
using TechDebtHub.Application.Features.Projetos.AtualizarProjeto;
using TechDebtHub.Application.Tests.Common;
using TechDebtHub.Application.Tests.Factories;
using TechDebtHub.Domain.Exceptions;
using Xunit;

namespace TechDebtHub.Application.Tests.Features.Projetos;

public sealed class AtualizarProjetoHandlerTests : ApplicationTestBase
{
    [Fact]
    public async Task HandleAsync_ComDadosValidos_DeveAtualizarProjetoEPersistirAlteracoes()
    {
        // Given
        var projeto = ProjetoFactory.Criar("Nome inicial", "Descrição inicial.");
        Context.Projetos.Add(projeto);
        await Context.SaveChangesAsync();

        var handler = new AtualizarProjetoHandler(Context);
        var command = new AtualizarProjetoCommand(
            projeto.Id,
            "Nome atualizado",
            "Descrição atualizada."
        );

        // When
        var response = await handler.HandleAsync(command, CancellationToken.None);

        Context.ChangeTracker.Clear();

        // Then
        var projetoSalvo = await Context
            .Projetos.AsNoTracking()
            .SingleAsync(p => p.Id == command.Id);

        Assert.Equal(command.Id, response.Id);
        Assert.Equal("Nome atualizado", projetoSalvo.Nome);
        Assert.Equal("NOME ATUALIZADO", projetoSalvo.NomeNormalizado);
        Assert.Equal("Descrição atualizada.", projetoSalvo.Descricao);
        Assert.NotNull(projetoSalvo.DataAtualizacao);
    }

    [Fact]
    public async Task HandleAsync_ProjetoInexistente_DeveLancarNotFoundException()
    {
        // Given
        var handler = new AtualizarProjetoHandler(Context);
        var command = new AtualizarProjetoCommand(
            Guid.NewGuid(),
            "Nome atualizado",
            "Descrição atualizada."
        );

        // When
        Func<Task> acaoInexistente = () => handler.HandleAsync(command, CancellationToken.None);

        // Then
        await Assert.ThrowsAsync<NotFoundException>(acaoInexistente);
    }

    [Fact]
    public async Task HandleAsync_NomePertenceAOutroProjeto_DeveLancarConflictException()
    {
        // Given
        var projetoUm = ProjetoFactory.Criar("TechDebt Hub");
        var projetoDois = ProjetoFactory.Criar("Sistema Financeiro");

        Context.Projetos.AddRange(projetoUm, projetoDois);
        await Context.SaveChangesAsync();

        var handler = new AtualizarProjetoHandler(Context);

        var command = new AtualizarProjetoCommand(
            projetoDois.Id,
            "techdebt   hub",
            "Descrição atualizada."
        );

        // When
        Func<Task> acaoConflitante = () => handler.HandleAsync(command, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<ConflictException>(acaoConflitante);
        Assert.Equal("Já existe um projeto com esse nome", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_MantendoProprioNome_DeveAtualizarProjetoSemConflito()
    {
        // Given
        var projeto = ProjetoFactory.Criar("TechDebt Hub", "Descrição inicial.");
        Context.Projetos.Add(projeto);
        await Context.SaveChangesAsync();

        var handler = new AtualizarProjetoHandler(Context);

        var command = new AtualizarProjetoCommand(projeto.Id, "techdebt   hub", "Nova descrição.");

        // When
        await handler.HandleAsync(command, CancellationToken.None);

        Context.ChangeTracker.Clear();

        // Then
        var projetoSalvo = await Context
            .Projetos.AsNoTracking()
            .SingleAsync(p => p.Id == command.Id);

        Assert.Equal("techdebt hub", projetoSalvo.Nome);
        Assert.Equal("TECHDEBT HUB", projetoSalvo.NomeNormalizado);
        Assert.Equal("Nova descrição.", projetoSalvo.Descricao);
    }

    [Fact]
    public async Task HandleAsync_ProjetoArquivado_DeveLancarDomainException()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        projeto.Arquivar();

        Context.Projetos.Add(projeto);
        await Context.SaveChangesAsync();

        var handler = new AtualizarProjetoHandler(Context);
        var command = new AtualizarProjetoCommand(projeto.Id, "Novo nome", "Nova descrição.");

        // When
        Func<Task> acaoEmProjetoArquivado = () =>
            handler.HandleAsync(command, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<DomainException>(acaoEmProjetoArquivado);
        Assert.Equal("Não é possível alterar um projeto arquivado", exception.Message);
    }
}
