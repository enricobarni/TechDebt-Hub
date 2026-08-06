using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Exceptions;
using TechDebtHub.Application.Features.Projetos.ArquivarProjeto;
using TechDebtHub.Application.Tests.Common;
using TechDebtHub.Application.Tests.Factories;
using TechDebtHub.Domain.Enums;
using TechDebtHub.Domain.Exceptions;
using Xunit;

namespace TechDebtHub.Application.Tests.Features.Projetos;

public sealed class ArquivarProjetoHandlerTests : ApplicationTestBase
{
    [Fact]
    public async Task HandleAsync_ProjetoInexistente_DeveLancarNotFoundException()
    {
        // Given
        var handler = new ArquivarProjetoHandler(Context);
        var command = new ArquivarProjetoCommand(Guid.NewGuid());

        // When
        Func<Task> acaoInexistente = () => handler.HandleAsync(command, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<NotFoundException>(acaoInexistente);
        Assert.Equal("Projeto não encontrado", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_ProjetoComDividaTecnicaAtiva_DeveLancarDomainException()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);

        var dividaAtiva = DividaTecnicaFactory.Criar(projeto.Id);
        Context.DividasTecnicas.Add(dividaAtiva);

        await Context.SaveChangesAsync();

        var handler = new ArquivarProjetoHandler(Context);
        var command = new ArquivarProjetoCommand(projeto.Id);

        // When
        Func<Task> acaoComDividaAtiva = () => handler.HandleAsync(command, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<DomainException>(acaoComDividaAtiva);
        Assert.Equal(
            "Não é possível arquivar um projeto que possui dívidas técnicas ativas",
            exception.Message
        );

        Context.ChangeTracker.Clear();

        var projetoSalvo = await Context
            .Projetos.AsNoTracking()
            .SingleAsync(p => p.Id == projeto.Id);

        Assert.False(projetoSalvo.Arquivado);
    }

    [Fact]
    public async Task HandleAsync_ProjetoSemDividas_DeveArquivarComSucesso()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);
        await Context.SaveChangesAsync();

        var handler = new ArquivarProjetoHandler(Context);
        var command = new ArquivarProjetoCommand(projeto.Id);

        // When
        await handler.HandleAsync(command, CancellationToken.None);

        Context.ChangeTracker.Clear();

        // Then
        var projetoSalvo = await Context
            .Projetos.AsNoTracking()
            .SingleAsync(p => p.Id == projeto.Id);

        Assert.True(projetoSalvo.Arquivado);
        Assert.NotNull(projetoSalvo.DataAtualizacao);
    }

    [Fact]
    public async Task HandleAsync_ProjetoComDividasArquivadasOuResolvidas_DeveArquivarComSucesso()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);

        var dividaArquivada = DividaTecnicaFactory.Criar(
            projeto.Id,
            titulo: "Dívida já arquivada"
        );
        dividaArquivada.Arquivar();

        var dividaResolvida = DividaTecnicaFactory.Criar(projeto.Id, titulo: "Dívida resolvida");
        dividaResolvida.AlterarStatus(StatusDivida.EmAnalise);
        dividaResolvida.AlterarStatus(StatusDivida.Planejada);
        dividaResolvida.AlterarStatus(StatusDivida.EmAndamento);
        dividaResolvida.AlterarStatus(StatusDivida.Resolvida);

        Context.DividasTecnicas.AddRange(dividaArquivada, dividaResolvida);
        await Context.SaveChangesAsync();

        var handler = new ArquivarProjetoHandler(Context);
        var command = new ArquivarProjetoCommand(projeto.Id);

        // When
        await handler.HandleAsync(command, CancellationToken.None);

        Context.ChangeTracker.Clear();

        // Then
        var projetoSalvo = await Context
            .Projetos.AsNoTracking()
            .SingleAsync(p => p.Id == projeto.Id);

        Assert.True(projetoSalvo.Arquivado);
        Assert.NotNull(projetoSalvo.DataAtualizacao);
    }
}
