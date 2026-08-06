using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Exceptions;
using TechDebtHub.Application.Features.DividasTecnicas.AlterarStatusDividaTecnica;
using TechDebtHub.Application.Tests.Common;
using TechDebtHub.Application.Tests.Factories;
using TechDebtHub.Domain.Enums;
using TechDebtHub.Domain.Exceptions;
using Xunit;

namespace TechDebtHub.Application.Tests.Features.DividasTecnicas;

public sealed class AlterarStatusDividaTecnicaHandlerTests : ApplicationTestBase
{
    [Fact]
    public async Task HandleAsync_DividaInexistente_DeveLancarNotFoundException()
    {
        // Given
        var handler = new AlterarStatusDividaTecnicaHandler(Context);
        var command = new AlterarStatusDividaTecnicaCommand(Guid.NewGuid(), StatusDivida.EmAnalise);

        // When
        Func<Task> acaoDividaInexistente = () =>
            handler.HandleAsync(command, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<NotFoundException>(acaoDividaInexistente);
        Assert.Equal("Dívida técnica não encontrada", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_DividaArquivada_DeveLancarDomainException()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);

        var divida = DividaTecnicaFactory.Criar(projeto.Id);
        divida.Arquivar();
        Context.DividasTecnicas.Add(divida);

        await Context.SaveChangesAsync();

        var handler = new AlterarStatusDividaTecnicaHandler(Context);
        var command = new AlterarStatusDividaTecnicaCommand(divida.Id, StatusDivida.EmAnalise);

        // When
        Func<Task> acaoDividaArquivada = () =>
            handler.HandleAsync(command, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<DomainException>(acaoDividaArquivada);
        Assert.Equal("A dívida técnica já está arquivada", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_NovoStatusIgualAoAtual_DeveLancarDomainException()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);

        var divida = DividaTecnicaFactory.Criar(projeto.Id);
        Context.DividasTecnicas.Add(divida);

        await Context.SaveChangesAsync();

        var handler = new AlterarStatusDividaTecnicaHandler(Context);
        var command = new AlterarStatusDividaTecnicaCommand(divida.Id, StatusDivida.Aberta);

        // When
        Func<Task> acaoStatusIgual = () => handler.HandleAsync(command, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<DomainException>(acaoStatusIgual);
        Assert.Equal("A dívida técnica já está nesse status", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_TransicaoNaoPermitida_DeveLancarDomainException()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);

        var divida = DividaTecnicaFactory.Criar(projeto.Id);
        Context.DividasTecnicas.Add(divida);

        await Context.SaveChangesAsync();

        var handler = new AlterarStatusDividaTecnicaHandler(Context);
        var command = new AlterarStatusDividaTecnicaCommand(divida.Id, StatusDivida.EmAndamento);

        // When
        Func<Task> acaoTransicaoInvalida = () =>
            handler.HandleAsync(command, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<DomainException>(acaoTransicaoInvalida);
        Assert.Equal(
            "Não é permitido alterar o status de Aberta para EmAndamento",
            exception.Message
        );
    }

    [Fact]
    public async Task HandleAsync_TransicaoValida_DeveAlterarStatusEPreencherDataAtualizacao()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);

        var divida = DividaTecnicaFactory.Criar(projeto.Id);
        Context.DividasTecnicas.Add(divida);

        await Context.SaveChangesAsync();

        var handler = new AlterarStatusDividaTecnicaHandler(Context);
        var command = new AlterarStatusDividaTecnicaCommand(divida.Id, StatusDivida.EmAnalise);

        // When
        var response = await handler.HandleAsync(command, CancellationToken.None);

        Context.ChangeTracker.Clear();

        // Then
        var dividaSalva = await Context
            .DividasTecnicas.AsNoTracking()
            .SingleAsync(divida => divida.Id == command.Id);

        Assert.Equal(StatusDivida.EmAnalise, response.Status);
        Assert.Equal(StatusDivida.EmAnalise, dividaSalva.Status);
        Assert.NotNull(dividaSalva.DataAtualizacao);
        Assert.Null(dividaSalva.DataResolucao);
    }

    [Fact]
    public async Task HandleAsync_TransicaoParaResolvida_DevePreencherDataResolucaoEDataAtualizacao()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);

        var divida = DividaTecnicaFactory.Criar(projeto.Id);
        divida.AlterarStatus(StatusDivida.EmAnalise);
        divida.AlterarStatus(StatusDivida.Planejada);
        divida.AlterarStatus(StatusDivida.EmAndamento);
        Context.DividasTecnicas.Add(divida);

        await Context.SaveChangesAsync();

        var handler = new AlterarStatusDividaTecnicaHandler(Context);
        var command = new AlterarStatusDividaTecnicaCommand(divida.Id, StatusDivida.Resolvida);

        // When
        var response = await handler.HandleAsync(command, CancellationToken.None);

        Context.ChangeTracker.Clear();

        // Then
        var dividaSalva = await Context
            .DividasTecnicas.AsNoTracking()
            .SingleAsync(divida => divida.Id == command.Id);

        Assert.Equal(StatusDivida.Resolvida, response.Status);
        Assert.NotNull(response.DataAtualizacao);
        Assert.NotNull(response.DataResolucao);
        Assert.Equal(StatusDivida.Resolvida, dividaSalva.Status);
        Assert.NotNull(dividaSalva.DataAtualizacao);
        Assert.NotNull(dividaSalva.DataResolucao);
    }
}
