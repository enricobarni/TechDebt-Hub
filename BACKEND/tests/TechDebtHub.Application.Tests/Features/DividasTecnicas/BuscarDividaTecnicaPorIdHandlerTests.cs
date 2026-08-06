using TechDebtHub.Application.Exceptions;
using TechDebtHub.Application.Features.DividasTecnicas.BuscarDividaTecnicaPorId;
using TechDebtHub.Application.Tests.Common;
using TechDebtHub.Application.Tests.Factories;
using TechDebtHub.Domain.Enums;
using Xunit;

namespace TechDebtHub.Application.Tests.Features.DividasTecnicas;

public sealed class BuscarDividaTecnicaPorIdHandlerTests : ApplicationTestBase
{
    [Fact]
    public async Task HandleAsync_DividaExistente_DeveRetornarDividaComMapeamentoCorreto()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);

        var divida = DividaTecnicaFactory.Criar(
            projeto.Id,
            titulo: "Consulta sem paginação",
            descricao: "A consulta retorna todos os registros.",
            categoria: CategoriaDivida.Performance,
            impacto: NivelImpacto.Alto,
            urgencia: NivelUrgencia.Media,
            frequencia: NivelFrequencia.Constante,
            esforco: NivelEsforco.Medio
        );
        Context.DividasTecnicas.Add(divida);

        await Context.SaveChangesAsync();

        Context.ChangeTracker.Clear();

        var handler = new BuscarDividaTecnicaPorIdHandler(Context);
        var query = new BuscarDividaTecnicaPorIdQuery(divida.Id);

        // When
        var response = await handler.HandleAsync(query, CancellationToken.None);

        // Then
        Assert.Equal(divida.Id, response.Id);
        Assert.Equal(divida.ProjetoId, response.ProjetoId);
        Assert.Equal(divida.Titulo, response.Titulo);
        Assert.Equal(divida.Descricao, response.Descricao);
        Assert.Equal(divida.Categoria, response.Categoria);
        Assert.Equal(divida.Status, response.Status);
        Assert.False(response.Arquivada);
        Assert.Equal(divida.Impacto, response.Impacto);
        Assert.Equal(divida.Urgencia, response.Urgencia);
        Assert.Equal(divida.Frequencia, response.Frequencia);
        Assert.Equal(divida.Esforco, response.Esforco);
        Assert.Equal(divida.PontuacaoPrioridade, response.PontuacaoPrioridade);
        Assert.Equal(divida.DataCriacao, response.DataCriacao);
        Assert.Null(response.DataAtualizacao);
        Assert.Null(response.DataResolucao);
    }

    [Fact]
    public async Task HandleAsync_DividaInexistente_DeveLancarNotFoundException()
    {
        // Given
        var handler = new BuscarDividaTecnicaPorIdHandler(Context);
        var query = new BuscarDividaTecnicaPorIdQuery(Guid.NewGuid());

        // When
        Func<Task> acaoDividaInexistente = () =>
            handler.HandleAsync(query, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<NotFoundException>(acaoDividaInexistente);
        Assert.Equal("Dívida técnica não encontrada", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_DividaArquivada_DeveRetornarArquivadaVerdadeiro()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);

        var divida = DividaTecnicaFactory.Criar(projeto.Id);
        divida.Arquivar();
        Context.DividasTecnicas.Add(divida);

        await Context.SaveChangesAsync();

        Context.ChangeTracker.Clear();

        var handler = new BuscarDividaTecnicaPorIdHandler(Context);
        var query = new BuscarDividaTecnicaPorIdQuery(divida.Id);

        // When
        var response = await handler.HandleAsync(query, CancellationToken.None);

        // Then
        Assert.True(response.Arquivada);
        Assert.NotNull(response.DataAtualizacao);
    }

    [Fact]
    public async Task HandleAsync_DividaResolvida_DeveRetornarDataResolucaoPreenchida()
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

        Context.ChangeTracker.Clear();

        var handler = new BuscarDividaTecnicaPorIdHandler(Context);
        var query = new BuscarDividaTecnicaPorIdQuery(divida.Id);

        // When
        var response = await handler.HandleAsync(query, CancellationToken.None);

        // Then
        Assert.Equal(StatusDivida.Resolvida, response.Status);
        Assert.NotNull(response.DataResolucao);
        Assert.NotNull(response.DataAtualizacao);
    }
}
