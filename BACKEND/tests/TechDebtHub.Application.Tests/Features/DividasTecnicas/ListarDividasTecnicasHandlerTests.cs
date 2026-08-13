using TechDebtHub.Application.Exceptions;
using TechDebtHub.Application.Features.DividasTecnicas.ListarDividasTecnicas;
using TechDebtHub.Application.Tests.Common;
using TechDebtHub.Application.Tests.Factories;
using TechDebtHub.Domain.Enums;
using Xunit;

namespace TechDebtHub.Application.Tests.Features.DividasTecnicas;

public sealed class ListarDividasTecnicasHandlerTests : ApplicationTestBase
{
    [Fact]
    public async Task HandleAsync_PaginaMenorQueUm_DeveLancarArgumentException()
    {
        // Given
        var handler = new ListarDividasTecnicasHandler(Context);
        var query = new ListarDividasTecnicasQuery(
            ProjetoId: Guid.NewGuid(),
            Status: null,
            Categoria: null,
            Arquivada: null,
            Busca: null,
            Pagina: 0,
            TamanhoPagina: 10
        );

        // When
        Func<Task> acaoPaginaInvalida = () => handler.HandleAsync(query, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<ArgumentException>(acaoPaginaInvalida);
        Assert.Equal("A página deve ser maior ou igual a 1", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task HandleAsync_TamanhoPaginaForaDoIntervalo_DeveLancarArgumentException(
        int tamanhoPagina
    )
    {
        // Given
        var handler = new ListarDividasTecnicasHandler(Context);
        var query = new ListarDividasTecnicasQuery(
            ProjetoId: Guid.NewGuid(),
            Status: null,
            Categoria: null,
            Arquivada: null,
            Busca: null,
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
    public async Task HandleAsync_StatusForaDoEnum_DeveLancarArgumentException()
    {
        // Given
        var handler = new ListarDividasTecnicasHandler(Context);
        var query = new ListarDividasTecnicasQuery(
            ProjetoId: Guid.NewGuid(),
            Status: (StatusDivida)999,
            Categoria: null,
            Arquivada: null,
            Busca: null,
            Pagina: 1,
            TamanhoPagina: 10
        );

        // When
        Func<Task> acaoStatusInvalido = () => handler.HandleAsync(query, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<ArgumentException>(acaoStatusInvalido);
        Assert.Equal("O status é inválido", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_CategoriaForaDoEnum_DeveLancarArgumentException()
    {
        // Given
        var handler = new ListarDividasTecnicasHandler(Context);
        var query = new ListarDividasTecnicasQuery(
            ProjetoId: Guid.NewGuid(),
            Status: null,
            Categoria: (CategoriaDivida)999,
            Arquivada: null,
            Busca: null,
            Pagina: 1,
            TamanhoPagina: 10
        );

        // When
        Func<Task> acaoCategoriaInvalida = () =>
            handler.HandleAsync(query, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<ArgumentException>(acaoCategoriaInvalida);
        Assert.Equal("A categoria informada é inválida", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_ProjetoInexistente_DeveLancarNotFoundException()
    {
        // Given
        var handler = new ListarDividasTecnicasHandler(Context);
        var query = new ListarDividasTecnicasQuery(
            ProjetoId: Guid.NewGuid(),
            Status: null,
            Categoria: null,
            Arquivada: null,
            Busca: null,
            Pagina: 1,
            TamanhoPagina: 10
        );

        // When
        Func<Task> acaoProjetoInexistente = () =>
            handler.HandleAsync(query, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<NotFoundException>(acaoProjetoInexistente);
        Assert.Equal("Projeto não encontrado", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_FiltroPadrao_DeveRetornarApenasDividasNaoArquivadasENaoResolvidas()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);

        var dividaAtiva = DividaTecnicaFactory.Criar(projeto.Id, titulo: "Dívida ativa");

        var dividaArquivada = DividaTecnicaFactory.Criar(projeto.Id, titulo: "Dívida arquivada");
        dividaArquivada.Arquivar();

        var dividaResolvida = DividaTecnicaFactory.Criar(projeto.Id, titulo: "Dívida resolvida");
        dividaResolvida.AlterarStatus(StatusDivida.EmAnalise);
        dividaResolvida.AlterarStatus(StatusDivida.Planejada);
        dividaResolvida.AlterarStatus(StatusDivida.EmAndamento);
        dividaResolvida.AlterarStatus(StatusDivida.Resolvida);

        Context.DividasTecnicas.AddRange(dividaAtiva, dividaArquivada, dividaResolvida);
        await Context.SaveChangesAsync();

        var handler = new ListarDividasTecnicasHandler(Context);
        var query = new ListarDividasTecnicasQuery(
            ProjetoId: projeto.Id,
            Status: null,
            Categoria: null,
            Arquivada: null,
            Busca: null,
            Pagina: 1,
            TamanhoPagina: 10
        );

        // When
        var resultado = await handler.HandleAsync(query, CancellationToken.None);

        // Then
        var item = Assert.Single(resultado.Itens);
        Assert.Equal(dividaAtiva.Id, item.Id);
        Assert.Equal(1, resultado.TotalItens);
    }

    [Fact]
    public async Task HandleAsync_FiltroStatusExplicito_DeveRetornarApenasDividasComStatusInformado()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);

        var dividaAberta = DividaTecnicaFactory.Criar(projeto.Id, titulo: "Dívida aberta");

        var dividaEmAnalise = DividaTecnicaFactory.Criar(projeto.Id, titulo: "Dívida em análise");
        dividaEmAnalise.AlterarStatus(StatusDivida.EmAnalise);

        Context.DividasTecnicas.AddRange(dividaAberta, dividaEmAnalise);
        await Context.SaveChangesAsync();

        var handler = new ListarDividasTecnicasHandler(Context);
        var query = new ListarDividasTecnicasQuery(
            ProjetoId: projeto.Id,
            Status: StatusDivida.EmAnalise,
            Categoria: null,
            Arquivada: null,
            Busca: null,
            Pagina: 1,
            TamanhoPagina: 10
        );

        // When
        var resultado = await handler.HandleAsync(query, CancellationToken.None);

        // Then
        var item = Assert.Single(resultado.Itens);
        Assert.Equal(dividaEmAnalise.Id, item.Id);
        Assert.Equal(StatusDivida.EmAnalise, item.Status);
    }

    [Fact]
    public async Task HandleAsync_FiltroCategoriaExplicito_DeveRetornarApenasDividasComCategoriaInformada()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);

        var dividaPerformance = DividaTecnicaFactory.Criar(
            projeto.Id,
            titulo: "Dívida de performance",
            categoria: CategoriaDivida.Performance
        );

        var dividaSeguranca = DividaTecnicaFactory.Criar(
            projeto.Id,
            titulo: "Dívida de segurança",
            categoria: CategoriaDivida.Seguranca
        );

        Context.DividasTecnicas.AddRange(dividaPerformance, dividaSeguranca);
        await Context.SaveChangesAsync();

        var handler = new ListarDividasTecnicasHandler(Context);
        var query = new ListarDividasTecnicasQuery(
            ProjetoId: projeto.Id,
            Status: null,
            Categoria: CategoriaDivida.Seguranca,
            Arquivada: null,
            Busca: null,
            Pagina: 1,
            TamanhoPagina: 10
        );

        // When
        var resultado = await handler.HandleAsync(query, CancellationToken.None);

        // Then
        var item = Assert.Single(resultado.Itens);
        Assert.Equal(dividaSeguranca.Id, item.Id);
        Assert.Equal(CategoriaDivida.Seguranca, item.Categoria);
    }

    [Fact]
    public async Task HandleAsync_FiltroBusca_DeveRetornarApenasDividasComTituloCorrespondente()
    {
        // O filtro normaliza o termo de busca (maiúsculas, sem acentos) e o compara
        // com `Titulo`, que preserva a formatação original. Por isso o título é
        // cadastrado já em maiúsculas e sem acentos para garantir a correspondência
        // independente do provider de banco utilizado.

        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);

        var dividaCorrespondente = DividaTecnicaFactory.Criar(
            projeto.Id,
            titulo: "Performance Crítica"
        );
        var dividaNaoCorrespondente = DividaTecnicaFactory.Criar(
            projeto.Id,
            titulo: "Segurança do Sistema"
        );

        Context.DividasTecnicas.AddRange(dividaCorrespondente, dividaNaoCorrespondente);
        await Context.SaveChangesAsync();

        var handler = new ListarDividasTecnicasHandler(Context);
        var query = new ListarDividasTecnicasQuery(
            ProjetoId: projeto.Id,
            Status: null,
            Categoria: null,
            Arquivada: null,
            Busca: "performance",
            Pagina: 1,
            TamanhoPagina: 10
        );

        // When
        var resultado = await handler.HandleAsync(query, CancellationToken.None);

        // Then
        var item = Assert.Single(resultado.Itens);
        Assert.Equal(dividaCorrespondente.Id, item.Id);
    }

    [Fact]
    public async Task HandleAsync_Paginacao_DeveOrdenarPorPontuacaoPrioridadeDescendenteERetornarTotalPaginas()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);

        var dividaBaixaPrioridade = DividaTecnicaFactory.Criar(
            projeto.Id,
            titulo: "Prioridade baixa",
            impacto: NivelImpacto.Baixo,
            urgencia: NivelUrgencia.Baixa,
            frequencia: NivelFrequencia.Rara,
            esforco: NivelEsforco.MuitoAlto
        );

        var dividaMediaPrioridade = DividaTecnicaFactory.Criar(
            projeto.Id,
            titulo: "Prioridade média",
            impacto: NivelImpacto.Alto,
            urgencia: NivelUrgencia.Media,
            frequencia: NivelFrequencia.Constante,
            esforco: NivelEsforco.Medio
        );

        var dividaAltaPrioridade = DividaTecnicaFactory.Criar(
            projeto.Id,
            titulo: "Prioridade alta",
            impacto: NivelImpacto.Critico,
            urgencia: NivelUrgencia.Imediata,
            frequencia: NivelFrequencia.Constante,
            esforco: NivelEsforco.Baixo
        );

        Context.DividasTecnicas.AddRange(
            dividaBaixaPrioridade,
            dividaMediaPrioridade,
            dividaAltaPrioridade
        );
        await Context.SaveChangesAsync();

        var handler = new ListarDividasTecnicasHandler(Context);

        var queryPaginaUm = new ListarDividasTecnicasQuery(
            ProjetoId: projeto.Id,
            Status: null,
            Categoria: null,
            Arquivada: null,
            Busca: null,
            Pagina: 1,
            TamanhoPagina: 2
        );

        // When
        var resultadoPaginaUm = await handler.HandleAsync(queryPaginaUm, CancellationToken.None);

        // Then
        Assert.Equal(3, resultadoPaginaUm.TotalItens);
        Assert.Equal(2, resultadoPaginaUm.TotalPaginas);
        Assert.Equal(2, resultadoPaginaUm.Itens.Count);
        Assert.Equal(dividaAltaPrioridade.Id, resultadoPaginaUm.Itens[0].Id);
        Assert.Equal(dividaMediaPrioridade.Id, resultadoPaginaUm.Itens[1].Id);

        var queryPaginaDois = new ListarDividasTecnicasQuery(
            ProjetoId: projeto.Id,
            Status: null,
            Categoria: null,
            Arquivada: null,
            Busca: null,
            Pagina: 2,
            TamanhoPagina: 2
        );

        // When
        var resultadoPaginaDois = await handler.HandleAsync(
            queryPaginaDois,
            CancellationToken.None
        );

        // Then
        var item = Assert.Single(resultadoPaginaDois.Itens);
        Assert.Equal(dividaBaixaPrioridade.Id, item.Id);
        Assert.Equal(2, resultadoPaginaDois.Pagina);
    }
}
