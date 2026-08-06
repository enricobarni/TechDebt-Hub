using System.Net;
using System.Net.Http.Json;
using TechDebtHub.Api.IntegrationTests.Common;
using TechDebtHub.Application.Common.Models;
using TechDebtHub.Application.Features.Projetos.ListarProjetos;
using TechDebtHub.Application.Tests.Factories;

namespace TechDebtHub.Api.IntegrationTests.Features.Projetos;

public sealed class ListarProjetosTests : ApiIntegrationTestBase
{
    public ListarProjetosTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task Get_Projetos_SemFiltros_DeveRetornar200ComListaVazia()
    {
        // Given - banco vazio (resetado antes de cada teste pela ApiIntegrationTestBase)

        // When
        var response = await Client.GetAsync("/projetos");

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var corpo = await response.Content.ReadFromJsonAsync<PagedResult<ProjetoResumoResponse>>();

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
    public async Task Get_Projetos_ComPaginacaoInvalida_DeveRetornar400(string queryString)
    {
        // Given - query string inválida informada como parâmetro do teste

        // When
        var response = await Client.GetAsync($"/projetos?{queryString}");

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_Projetos_ComFiltroArquivado_DeveRetornarApenasProjetosArquivados()
    {
        // Given
        var projetoAtivo = ProjetoFactory.Criar("Projeto Ativo");
        var projetoArquivado = ProjetoFactory.Criar("Projeto Arquivado");
        projetoArquivado.Arquivar();

        await UsingContextAsync(async context =>
        {
            context.Projetos.AddRange(projetoAtivo, projetoArquivado);
            await context.SaveChangesAsync();
        });

        // When
        var response = await Client.GetAsync("/projetos?arquivado=true");

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var corpo = await response.Content.ReadFromJsonAsync<PagedResult<ProjetoResumoResponse>>();

        Assert.NotNull(corpo);
        Assert.Single(corpo!.Itens);
        Assert.Equal(projetoArquivado.Id, corpo.Itens[0].Id);
        Assert.True(corpo.Itens[0].Arquivado);
    }

    [Fact]
    public async Task Get_Projetos_ComFiltroBusca_DeveRetornarApenasProjetosCorrespondentes()
    {
        // Given
        var projetoTechDebt = ProjetoFactory.Criar("TechDebt Hub");
        var projetoFinanceiro = ProjetoFactory.Criar("Sistema Financeiro");

        await UsingContextAsync(async context =>
        {
            context.Projetos.AddRange(projetoTechDebt, projetoFinanceiro);
            await context.SaveChangesAsync();
        });

        // When
        var response = await Client.GetAsync("/projetos?busca=TechDebt");

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var corpo = await response.Content.ReadFromJsonAsync<PagedResult<ProjetoResumoResponse>>();

        Assert.NotNull(corpo);
        Assert.Single(corpo!.Itens);
        Assert.Equal(projetoTechDebt.Id, corpo.Itens[0].Id);
    }

    [Fact]
    public async Task Get_Projetos_ComPaginacao_DeveRetornarItensDaPaginaSolicitada()
    {
        // Given - 3 projetos, página de tamanho 2
        var projeto1 = ProjetoFactory.Criar("Projeto 1");
        var projeto2 = ProjetoFactory.Criar("Projeto 2");
        var projeto3 = ProjetoFactory.Criar("Projeto 3");

        await UsingContextAsync(async context =>
        {
            context.Projetos.AddRange(projeto1, projeto2, projeto3);
            await context.SaveChangesAsync();
        });

        // When
        var response = await Client.GetAsync("/projetos?pagina=2&tamanhoPagina=2");

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var corpo = await response.Content.ReadFromJsonAsync<PagedResult<ProjetoResumoResponse>>();

        Assert.NotNull(corpo);
        Assert.Equal(2, corpo!.Pagina);
        Assert.Equal(2, corpo.TamanhoPagina);
        Assert.Equal(3, corpo.TotalItens);
        Assert.Equal(2, corpo.TotalPaginas);
        Assert.Single(corpo.Itens);
    }
}
