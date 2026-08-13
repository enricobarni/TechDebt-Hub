using System.Net;
using System.Net.Http.Json;
using TechDebtHub.Api.IntegrationTests.Common;
using TechDebtHub.Application.Common.Models;
using TechDebtHub.Application.Features.DividasTecnicas.ListarDividasTecnicas;
using TechDebtHub.Application.Tests.Factories;
using TechDebtHub.Domain.Enums;

namespace TechDebtHub.Api.IntegrationTests.Features.DividasTecnicas;

public sealed class ListarDividasTecnicasTests : ApiIntegrationTestBase
{
    public ListarDividasTecnicasTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task Get_DividasTecnicas_ComProjetoSemDividas_DeveRetornar200ComListaVazia()
    {
        // Given
        var projeto = ProjetoFactory.Criar();

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            await context.SaveChangesAsync();
        });

        // When
        var response = await Client.GetAsync($"/projetos/{projeto.Id}/dividas");

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var corpo = await response.Content.ReadFromJsonAsync<
            PagedResult<ListarDividasTecnicasResponse>
        >();

        Assert.NotNull(corpo);
        Assert.Empty(corpo!.Itens);
        Assert.Equal(0, corpo.TotalItens);
        Assert.Equal(0, corpo.TotalPaginas);
    }

    [Theory]
    [InlineData("pagina=0")]
    [InlineData("pagina=-1")]
    [InlineData("tamanhoPagina=0")]
    [InlineData("tamanhoPagina=101")]
    public async Task Get_DividasTecnicas_ComPaginacaoInvalida_DeveRetornar400(string queryString)
    {
        // Given - a query de paginação é validada antes mesmo do projeto ser buscado, então um
        // projeto inexistente é suficiente para isolar o comportamento de paginação inválida.
        var projetoId = Guid.NewGuid();

        // When
        var response = await Client.GetAsync($"/projetos/{projetoId}/dividas?{queryString}");

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_DividasTecnicas_ComProjetoInexistente_DeveRetornar404()
    {
        // Given
        var projetoInexistente = Guid.NewGuid();

        // When
        var response = await Client.GetAsync($"/projetos/{projetoInexistente}/dividas");

        // Then
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Projeto não encontrado", corpo);
    }

    [Fact]
    public async Task Get_DividasTecnicas_SemFiltros_DeveExcluirArquivadasEResolvidasPorPadrao()
    {
        // Given
        var projeto = ProjetoFactory.Criar();

        var dividaAberta = DividaTecnicaFactory.Criar(projeto.Id, "Divida aberta");

        var dividaArquivada = DividaTecnicaFactory.Criar(projeto.Id, "Divida arquivada");
        dividaArquivada.Arquivar();

        var dividaResolvida = DividaTecnicaFactory.Criar(projeto.Id, "Divida resolvida");
        dividaResolvida.AlterarStatus(StatusDivida.EmAnalise);
        dividaResolvida.AlterarStatus(StatusDivida.Planejada);
        dividaResolvida.AlterarStatus(StatusDivida.EmAndamento);
        dividaResolvida.AlterarStatus(StatusDivida.Resolvida);

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            context.DividasTecnicas.AddRange(dividaAberta, dividaArquivada, dividaResolvida);
            await context.SaveChangesAsync();
        });

        // When
        var response = await Client.GetAsync($"/projetos/{projeto.Id}/dividas");

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var corpo = await response.Content.ReadFromJsonAsync<
            PagedResult<ListarDividasTecnicasResponse>
        >();

        Assert.NotNull(corpo);
        Assert.Single(corpo!.Itens);
        Assert.Equal(dividaAberta.Id, corpo.Itens[0].Id);
    }

    [Fact]
    public async Task Get_DividasTecnicas_ComFiltroStatus_DeveRetornarApenasDividasComStatusInformado()
    {
        // Given
        var projeto = ProjetoFactory.Criar();

        var dividaAberta = DividaTecnicaFactory.Criar(projeto.Id, "Divida aberta");

        var dividaEmAnalise = DividaTecnicaFactory.Criar(projeto.Id, "Divida em analise");
        dividaEmAnalise.AlterarStatus(StatusDivida.EmAnalise);

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            context.DividasTecnicas.AddRange(dividaAberta, dividaEmAnalise);
            await context.SaveChangesAsync();
        });

        // When
        var response = await Client.GetAsync(
            $"/projetos/{projeto.Id}/dividas?status={(int)StatusDivida.EmAnalise}"
        );

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var corpo = await response.Content.ReadFromJsonAsync<
            PagedResult<ListarDividasTecnicasResponse>
        >();

        Assert.NotNull(corpo);
        Assert.Single(corpo!.Itens);
        Assert.Equal(dividaEmAnalise.Id, corpo.Itens[0].Id);
    }

    [Fact]
    public async Task Get_DividasTecnicas_ComFiltroCategoria_DeveRetornarApenasDividasDaCategoriaInformada()
    {
        // Given
        var projeto = ProjetoFactory.Criar();

        var dividaPerformance = DividaTecnicaFactory.Criar(
            projeto.Id,
            "Divida de performance",
            categoria: CategoriaDivida.Performance
        );

        var dividaSeguranca = DividaTecnicaFactory.Criar(
            projeto.Id,
            "Divida de seguranca",
            categoria: CategoriaDivida.Seguranca
        );

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            context.DividasTecnicas.AddRange(dividaPerformance, dividaSeguranca);
            await context.SaveChangesAsync();
        });

        // When
        var response = await Client.GetAsync(
            $"/projetos/{projeto.Id}/dividas?categoria={(int)CategoriaDivida.Seguranca}"
        );

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var corpo = await response.Content.ReadFromJsonAsync<
            PagedResult<ListarDividasTecnicasResponse>
        >();

        Assert.NotNull(corpo);
        Assert.Single(corpo!.Itens);
        Assert.Equal(dividaSeguranca.Id, corpo.Itens[0].Id);
    }

    [Fact]
    public async Task Get_DividasTecnicas_ComFiltroArquivada_DeveRetornarApenasDividasArquivadas()
    {
        // Given
        var projeto = ProjetoFactory.Criar();

        var dividaAtiva = DividaTecnicaFactory.Criar(projeto.Id, "Divida ativa");

        var dividaArquivada = DividaTecnicaFactory.Criar(projeto.Id, "Divida arquivada");
        dividaArquivada.Arquivar();

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            context.DividasTecnicas.AddRange(dividaAtiva, dividaArquivada);
            await context.SaveChangesAsync();
        });

        // When
        var response = await Client.GetAsync($"/projetos/{projeto.Id}/dividas?arquivada=true");

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var corpo = await response.Content.ReadFromJsonAsync<
            PagedResult<ListarDividasTecnicasResponse>
        >();

        Assert.NotNull(corpo);
        Assert.Single(corpo!.Itens);
        Assert.Equal(dividaArquivada.Id, corpo.Itens[0].Id);
        Assert.True(corpo.Itens[0].Arquivada);
    }

    [Fact]
    public async Task Get_DividasTecnicas_ComFiltroBusca_DeveSerCaseInsensitiveEIgnorarAcentos()
    {
        // Given - termo buscado em minúsculas e sem acento; título salvo em caixa natural, com
        // acento. Antes do fix (comparação contra Titulo em vez de TituloNormalizado), essa
        // combinação não retornava nenhum resultado.
        var projeto = ProjetoFactory.Criar();

        var dividaConsulta = DividaTecnicaFactory.Criar(projeto.Id, "Consulta sem paginação");
        var dividaDeploy = DividaTecnicaFactory.Criar(projeto.Id, "Deploy manual sem pipeline");

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            context.DividasTecnicas.AddRange(dividaConsulta, dividaDeploy);
            await context.SaveChangesAsync();
        });

        // When
        var response = await Client.GetAsync($"/projetos/{projeto.Id}/dividas?busca=paginacao");

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var corpo =
            await response.Content.ReadFromJsonAsync.PagedResult<ListarDividasTecnicasResponse>();

        Assert.NotNull(corpo);
        Assert.Single(corpo!.Itens);
        Assert.Equal(dividaConsulta.Id, corpo.Itens[0].Id);
    }

    [Fact]
    public async Task Get_DividasTecnicas_ComFiltroBusca_SemCorrespondencia_DeveRetornarListaVazia()
    {
        // Given
        var projeto = ProjetoFactory.Criar();

        var dividaConsulta = DividaTecnicaFactory.Criar(projeto.Id, "Consulta sem paginação");

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            context.DividasTecnicas.Add(dividaConsulta);
            await context.SaveChangesAsync();
        });

        // When
        var response = await Client.GetAsync($"/projetos/{projeto.Id}/dividas?busca=deploy");

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var corpo =
            await response.Content.ReadFromJsonAsync.PagedResult<ListarDividasTecnicasResponse>();

        Assert.NotNull(corpo);
        Assert.Empty(corpo!.Itens);
    }

    [Fact]
    public async Task Get_DividasTecnicas_ComPaginacao_DeveRetornarItensDaPaginaSolicitada()
    {
        // Given - 3 dívidas ativas, página de tamanho 2
        var projeto = ProjetoFactory.Criar();

        var divida1 = DividaTecnicaFactory.Criar(projeto.Id, "Divida 1");
        var divida2 = DividaTecnicaFactory.Criar(projeto.Id, "Divida 2");
        var divida3 = DividaTecnicaFactory.Criar(projeto.Id, "Divida 3");

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            context.DividasTecnicas.AddRange(divida1, divida2, divida3);
            await context.SaveChangesAsync();
        });

        // When
        var response = await Client.GetAsync(
            $"/projetos/{projeto.Id}/dividas?pagina=2&tamanhoPagina=2"
        );

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var corpo = await response.Content.ReadFromJsonAsync<
            PagedResult<ListarDividasTecnicasResponse>
        >();

        Assert.NotNull(corpo);
        Assert.Equal(2, corpo!.Pagina);
        Assert.Equal(2, corpo.TamanhoPagina);
        Assert.Equal(3, corpo.TotalItens);
        Assert.Equal(2, corpo.TotalPaginas);
        Assert.Single(corpo.Itens);
    }
}
