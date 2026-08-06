using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Exceptions;
using TechDebtHub.Application.Features.DividasTecnicas.ArquivarDividaTecnica;
using TechDebtHub.Application.Tests.Common;
using TechDebtHub.Application.Tests.Factories;
using TechDebtHub.Domain.Enums;
using TechDebtHub.Domain.Exceptions;
using Xunit;

namespace TechDebtHub.Application.Tests.Features.DividasTecnicas;

public sealed class ArquivarDividaTecnicaHandlerTests : ApplicationTestBase
{
    [Fact]
    public async Task HandleAsync_DividaInexistente_DeveLancarNotFoundException()
    {
        // Given
        var handler = new ArquivarDividaTecnicaHandler(Context);
        var command = new ArquivarDividaTecnicaCommand(Guid.NewGuid());

        // When
        Func<Task> acaoDividaInexistente = () =>
            handler.HandleAsync(command, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<NotFoundException>(acaoDividaInexistente);
        Assert.Equal("Dívida técnica não encontrada", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_DividaJaArquivada_DeveLancarDomainException()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);

        var divida = DividaTecnicaFactory.Criar(projeto.Id);
        divida.Arquivar();
        Context.DividasTecnicas.Add(divida);

        await Context.SaveChangesAsync();

        var handler = new ArquivarDividaTecnicaHandler(Context);
        var command = new ArquivarDividaTecnicaCommand(divida.Id);

        // When
        Func<Task> acaoDividaJaArquivada = () =>
            handler.HandleAsync(command, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<DomainException>(acaoDividaJaArquivada);
        Assert.Equal("A dívida técnica já está arquivada", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_DividaResolvida_DeveLancarDomainException()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);

        var divida = DividaTecnicaFactory.Criar(projeto.Id);
        divida.AlterarStatus(StatusDivida.EmAnalise);
        divida.AlterarStatus(StatusDivida.Planejada);
        divida.AlterarStatus(StatusDivida.EmAndamento);
        divida.AlterarStatus(StatusDivida.Resolvida);
        Context.DividasTecnicas.Add(divida);

        await Context.SaveChangesAsync();

        var handler = new ArquivarDividaTecnicaHandler(Context);
        var command = new ArquivarDividaTecnicaCommand(divida.Id);

        // When
        Func<Task> acaoDividaResolvida = () =>
            handler.HandleAsync(command, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<DomainException>(acaoDividaResolvida);
        Assert.Equal(
            "Não é possível arquivar uma dívida técnica resolvida",
            exception.Message
        );
    }

    [Fact]
    public async Task HandleAsync_DividaComStatusNormal_DeveArquivarComSucesso()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);

        var divida = DividaTecnicaFactory.Criar(projeto.Id);
        Context.DividasTecnicas.Add(divida);

        await Context.SaveChangesAsync();

        var handler = new ArquivarDividaTecnicaHandler(Context);
        var command = new ArquivarDividaTecnicaCommand(divida.Id);

        // When
        await handler.HandleAsync(command, CancellationToken.None);

        Context.ChangeTracker.Clear();

        // Then
        var dividaSalva = await Context
            .DividasTecnicas.AsNoTracking()
            .SingleAsync(divida => divida.Id == command.Id);

        Assert.True(dividaSalva.Arquivada);
        Assert.NotNull(dividaSalva.DataAtualizacao);
    }
}
