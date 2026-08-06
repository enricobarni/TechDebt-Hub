using TechDebtHub.Application.Features.Projetos.ListarProjetos;
using TechDebtHub.Application.Tests.Common;
using TechDebtHub.Application.Tests.Factories;
using Xunit;

namespace TechDebtHub.Application.Tests.Features.Projetos;

public sealed class ListarProjetosHandlerTests : ApplicationTestBase
{
    [Fact]
    public async Task HandleAsync_PaginaMenorQueUm_DeveLancarArgumentException()
    {
        // Given
        var handler = new ListarProjetosHandler(Context);
        var query = new ListarProjetosQuery(Busca: null, Arquivado: null, Pagina: 0, TamanhoPagina: 10);

        // When
        Func<Task> acaoPaginaInvalida = () => handler.HandleAsync(query, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<ArgumentException>(acaoPaginaInvalida);
        Assert.Equal("Apágina deve ser maior ou igual a 1", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task HandleAsync_TamanhoPaginaForaDoIntervalo_DeveLancarArgumentException(
        int tamanhoPagina
    )
    {
        // Given
        var handler = new ListarProjetosHandler(Context);
        var query = new ListarProjetosQuery(
            Busca: null,
            Arquivado: null,
            Pagina: 1,
            TamanhoPagina: tamanhoPagina
        );

        // When
        Func<Task> acaoTamanhoInvalido = () => handler.HandleAsync(query, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<ArgumentException>(acaoTamanhoInvalido);
        Assert.Equal("O tamanho da página deve estar entre 1 e 100", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_FiltroPadrao_DeveRetornarApenasProjetosNaoArquivados()
    {
        // Given
        var projetoAtivo = ProjetoFactory.Criar("Projeto Ativo");
        var projetoArquivado = ProjetoFactory.Criar("Projeto Arquivado");
        projetoArquivado.Arquivar();

        Context.Projetos.AddRange(projetoAtivo, projetoArquivado);
        await Context.SaveChangesAsync();

        var handler = new ListarProjetosHandler(Context);
        var query = new ListarProjetosQuery(Busca: null, Arquivado: null, Pagina: 1, TamanhoPagina: 10);

        // When
        var resultado = await handler.HandleAsync(query, CancellationToken.None);

        // Then
        var item = Assert.Single(resultado.Itens);
        Assert.Equal(projetoAtivo.Id, item.Id);
        Assert.Equal(1, resultado.TotalItens);
    }

    [Fact]
    public async Task HandleAsync_FiltroArquivadoVerdadeiro_DeveRetornarApenasProjetosArquivados()
    {
        // Given
        var projetoAtivo = ProjetoFactory.Criar("Projeto Ativo");
        var projetoArquivado = ProjetoFactory.Criar("Projeto Arquivado");
        projetoArquivado.Arquivar();

        Context.Projetos.AddRange(projetoAtivo, projetoArquivado);
        await Context.SaveChangesAsync();

        var handler = new ListarProjetosHandler(Context);
        var query = new ListarProjetosQuery(
            Busca: null,
            Arquivado: true,
            Pagina: 1,
            TamanhoPagina: 10
        );

        // When
        var resultado = await handler.HandleAsync(query, CancellationToken.None);

        // Then
        var item = Assert.Single(resultado.Itens);
        Assert.Equal(projetoArquivado.Id, item.Id);
        Assert.True(item.Arquivado);
    }

    [Fact]
    public async Task HandleAsync_FiltroArquivadoFalso_DeveRetornarApenasProjetosNaoArquivados()
    {
        // Given
        var projetoAtivo = ProjetoFactory.Criar("Projeto Ativo");
        var projetoArquivado = ProjetoFactory.Criar("Projeto Arquivado");
        projetoArquivado.Arquivar();

        Context.Projetos.AddRange(projetoAtivo, projetoArquivado);
        await Context.SaveChangesAsync();

        var handler = new ListarProjetosHandler(Context);
        var query = new ListarProjetosQuery(
            Busca: null,
            Arquivado: false,
            Pagina: 1,
            TamanhoPagina: 10
        );

        // When
        var resultado = await handler.HandleAsync(query, CancellationToken.None);

        // Then
        var item = Assert.Single(resultado.Itens);
        Assert.Equal(projetoAtivo.Id, item.Id);
        Assert.False(item.Arquivado);
    }

    [Fact]
    public async Task HandleAsync_BuscaPorNomeComCorrespondenciaExata_DeveRetornarProjeto()
    {
        // Given
        var projeto = ProjetoFactory.Criar("TechDebt Hub");
        var outroProjeto = ProjetoFactory.Criar("Sistema Financeiro");

        Context.Projetos.AddRange(projeto, outroProjeto);
        await Context.SaveChangesAsync();

        var handler = new ListarProjetosHandler(Context);
        var query = new ListarProjetosQuery(
            Busca: "TechDebt",
            Arquivado: null,
            Pagina: 1,
            TamanhoPagina: 10
        );

        // When
        var resultado = await handler.HandleAsync(query, CancellationToken.None);

        // Then
        var item = Assert.Single(resultado.Itens);
        Assert.Equal(projeto.Id, item.Id);
    }

    [Fact]
    public async Task HandleAsync_BuscaComCasingDiferente_NaoDeveEncontrarProjeto()
    {
        // O filtro `Nome.Contains(busca)` é traduzido pelo provider Sqlite usando
        // comparação binária (case-sensitive), diferente do comportamento
        // case-insensitive do SQL Server. Este teste documenta esse comportamento.

        // Given
        var projeto = ProjetoFactory.Criar("TechDebt Hub");
        Context.Projetos.Add(projeto);
        await Context.SaveChangesAsync();

        var handler = new ListarProjetosHandler(Context);
        var query = new ListarProjetosQuery(
            Busca: "techdebt",
            Arquivado: null,
            Pagina: 1,
            TamanhoPagina: 10
        );

        // When
        var resultado = await handler.HandleAsync(query, CancellationToken.None);

        // Then
        Assert.Equal(0, resultado.TotalItens);
    }

    [Fact]
    public async Task HandleAsync_Paginacao_DeveRetornarItensOrdenadosPorDataDeAtualizacaoDescendente()
    {
        // Given
        var projetoUm = ProjetoFactory.Criar("Projeto Um");
        var projetoDois = ProjetoFactory.Criar("Projeto Dois");
        var projetoTres = ProjetoFactory.Criar("Projeto Tres");

        Context.Projetos.AddRange(projetoUm, projetoDois, projetoTres);
        await Context.SaveChangesAsync();

        // Atualiza o projeto Um por último para que ele fique em primeiro na ordenação
        projetoUm.AtualizarProjeto("Projeto Um", "Descrição atualizada.");
        await Context.SaveChangesAsync();

        var handler = new ListarProjetosHandler(Context);

        var queryPaginaUm = new ListarProjetosQuery(
            Busca: null,
            Arquivado: null,
            Pagina: 1,
            TamanhoPagina: 2
        );

        // When
        var resultadoPaginaUm = await handler.HandleAsync(queryPaginaUm, CancellationToken.None);

        // Then
        Assert.Equal(3, resultadoPaginaUm.TotalItens);
        Assert.Equal(2, resultadoPaginaUm.TotalPaginas);
        Assert.Equal(2, resultadoPaginaUm.Itens.Count);
        Assert.Equal(projetoUm.Id, resultadoPaginaUm.Itens[0].Id);

        var queryPaginaDois = new ListarProjetosQuery(
            Busca: null,
            Arquivado: null,
            Pagina: 2,
            TamanhoPagina: 2
        );

        // When
        var resultadoPaginaDois = await handler.HandleAsync(
            queryPaginaDois,
            CancellationToken.None
        );

        // Then
        Assert.Single(resultadoPaginaDois.Itens);
        Assert.Equal(2, resultadoPaginaDois.Pagina);
    }
}
