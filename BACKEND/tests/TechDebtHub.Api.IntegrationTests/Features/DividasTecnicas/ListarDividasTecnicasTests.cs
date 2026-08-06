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
    public async Task Get_DividasTecnicas_ComFiltroBusca_ETituloEmMaiusculas_DeveRetornarApenasDividasCorrespondentes()
    {
        // Given - bug conhecido e documentado abaixo: o handler normaliza o termo de busca para
        // maiúsculas sem acentos (TextNormalizer.NormalizarParaComparacao) mas compara esse termo
        // contra "divida.Titulo" (que preserva a caixa original digitada pelo usuário, apenas com
        // espaços colapsados via PrepararParaExibicao), e não contra "divida.TituloNormalizado"
        // (que é o campo já normalizado para comparação). Como o provedor Sqlite do EF Core
        // traduz string.Contains para "instr()", que é case-sensitive, a busca só encontra
        // resultados quando o trecho do título já estiver em maiúsculas — não é assim que um
        // usuário digitaria um título normalmente. Ver também o teste
        // "..._ComTituloEmCaixaNatural_NaoEncontraApesarDoTituloConterOTermo" abaixo, que
        // demonstra o caso comum (título em caixa natural) não encontrando o resultado esperado.
        var projeto = ProjetoFactory.Criar();

        var dividaConsulta = DividaTecnicaFactory.Criar(projeto.Id, "PAGINACAO ausente na consulta");
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

        var corpo = await response.Content.ReadFromJsonAsync<
            PagedResult<ListarDividasTecnicasResponse>
        >();

        Assert.NotNull(corpo);
        Assert.Single(corpo!.Itens);
        Assert.Equal(dividaConsulta.Id, corpo.Itens[0].Id);
    }

    [Fact]
    public async Task Get_DividasTecnicas_ComFiltroBusca_ComTituloEmCaixaNatural_NaoEncontraApesarDoTituloConterOTermo()
    {
        // Given - documenta o mesmo bug de case-sensitivity descrito no teste acima: um título
        // digitado do jeito que um usuário normalmente digitaria (caixa natural, não toda em
        // maiúsculas) NÃO é encontrado pela busca mesmo contendo literalmente o termo buscado,
        // porque o termo de busca é normalizado para maiúsculas mas o título não é. Este teste
        // fixa o comportamento REAL da API hoje (nenhum resultado), não o comportamento desejável.
        var projeto = ProjetoFactory.Criar();

        var dividaConsulta = DividaTecnicaFactory.Criar(projeto.Id, "Consulta sem paginacao");

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            context.DividasTecnicas.Add(dividaConsulta);
            await context.SaveChangesAsync();
        });

        // When
        var response = await Client.GetAsync($"/projetos/{projeto.Id}/dividas?busca=paginacao");

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var corpo = await response.Content.ReadFromJsonAsync<
            PagedResult<ListarDividasTecnicasResponse>
        >();

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
