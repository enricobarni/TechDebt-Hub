using System.Net;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Api.IntegrationTests.Common;
using TechDebtHub.Application.Tests.Factories;
using TechDebtHub.Domain.Enums;

namespace TechDebtHub.Api.IntegrationTests.Features.DividasTecnicas;

/// <summary>
/// O endpoint de arquivar dívida técnica está declarado no controller como
/// <c>[HttpDelete("/dividas{id:guid}")]</c> — repare que falta a barra "/" entre o segmento
/// literal "dividas" e o parâmetro "{id:guid}", ao contrário de todos os outros endpoints deste
/// controller (ex.: <c>[HttpGet("/dividas/{id:guid}")]</c>, com a barra). Isso faz com que o
/// template de rota real seja um ÚNICO segmento "dividas{id:guid}", em vez de dois segmentos
/// "dividas" e "{id:guid}".
///
/// Na prática, isso significa que:
/// - Uma chamada para a URL que qualquer cliente razoável construiria, "/dividas/{id}" (COM
///   barra, no mesmo padrão dos demais endpoints), NÃO é roteada para este endpoint DELETE.
///   Curiosamente, o resultado observado empiricamente não é 404: como "/dividas/{id:guid}" (COM
///   barra) É um template de rota válido para os endpoints GET e PUT deste mesmo controller
///   (ex.: <c>[HttpGet("/dividas/{id:guid}")]</c>), o ASP.NET Core reconhece que a URL casa com
///   um recurso existente, só que não para o verbo DELETE — e devolve 405 (Method Not Allowed)
///   em vez de 404. O handler de arquivar nunca chega a ser executado.
/// - A única forma de de fato alcançar o handler é reproduzindo o bug literalmente, concatenando
///   o id logo após "dividas" sem barra nenhuma (ex.: "/dividas3fa85f64-...").
///
/// Os testes abaixo comprovam empiricamente esse comportamento. O bug NÃO foi corrigido em
/// src/ (fora do escopo desta tarefa de testes) — os testes documentam o comportamento real da
/// API hoje.
/// </summary>
public sealed class ArquivarDividaTecnicaTests : ApiIntegrationTestBase
{
    public ArquivarDividaTecnicaTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task Delete_DividasTecnicas_ComUrlNoPadraoDosDemaisEndpoints_RetornaMethodNotAllowedPorBugDeRotaAoInvesDeNoContent()
    {
        // Given - dívida existente, arquivável
        var projeto = ProjetoFactory.Criar();
        var divida = DividaTecnicaFactory.Criar(projeto.Id);

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            context.DividasTecnicas.Add(divida);
            await context.SaveChangesAsync();
        });

        // When - URL construída no mesmo padrão dos demais endpoints ("/dividas/{id}", com barra)
        var response = await Client.DeleteAsync($"/dividas/{divida.Id}");

        // Then - bug de rota conhecido, confirmado empiricamente: "/dividas/{id:guid}" (com
        // barra) já é um template de rota válido para GET e PUT neste controller, então o
        // ASP.NET Core reconhece a URL mas rejeita o verbo DELETE com 405 (Method Not Allowed),
        // sem executar o handler de arquivar.
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);

        var dividaPersistida = await UsingContextAsync(context =>
            context.DividasTecnicas.AsNoTracking().SingleAsync(d => d.Id == divida.Id)
        );

        Assert.False(dividaPersistida.Arquivada);
    }

    [Fact]
    public async Task Delete_DividasTecnicas_ComUrlConcatenadaConformeORouteTemplateReal_DeveRetornar204EArquivar()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        var divida = DividaTecnicaFactory.Criar(projeto.Id);

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            context.DividasTecnicas.Add(divida);
            await context.SaveChangesAsync();
        });

        // When - reproduz literalmente o template de rota real "/dividas{id:guid}" (sem barra)
        var response = await Client.DeleteAsync($"/dividas{divida.Id}");

        // Then - com a URL malformada correspondente ao bug, a rota casa e o handler executa
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var dividaPersistida = await UsingContextAsync(context =>
            context.DividasTecnicas.AsNoTracking().SingleAsync(d => d.Id == divida.Id)
        );

        Assert.True(dividaPersistida.Arquivada);
        Assert.NotNull(dividaPersistida.DataAtualizacao);
    }

    [Fact]
    public async Task Delete_DividasTecnicas_ComIdInexistenteEUrlConcatenada_DeveRetornar404()
    {
        // Given
        var idInexistente = Guid.NewGuid();

        // When - só é possível alcançar o handler (e a exceção NotFoundException dele) através
        // da URL concatenada, conforme o template de rota real documentado nesta classe.
        var response = await Client.DeleteAsync($"/dividas{idInexistente}");

        // Then
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Dívida técnica não encontrada", corpo);
    }

    [Fact]
    public async Task Delete_DividasTecnicas_ComDividaJaArquivadaEUrlConcatenada_DeveRetornar422()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        var divida = DividaTecnicaFactory.Criar(projeto.Id);
        divida.Arquivar();

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            context.DividasTecnicas.Add(divida);
            await context.SaveChangesAsync();
        });

        // When
        var response = await Client.DeleteAsync($"/dividas{divida.Id}");

        // Then
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("A dívida técnica já está arquivada", corpo);
    }

    [Fact]
    public async Task Delete_DividasTecnicas_ComDividaResolvidaEUrlConcatenada_DeveRetornar422()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        var divida = DividaTecnicaFactory.Criar(projeto.Id);
        divida.AlterarStatus(StatusDivida.EmAnalise);
        divida.AlterarStatus(StatusDivida.Planejada);
        divida.AlterarStatus(StatusDivida.EmAndamento);
        divida.AlterarStatus(StatusDivida.Resolvida);

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            context.DividasTecnicas.Add(divida);
            await context.SaveChangesAsync();
        });

        // When
        var response = await Client.DeleteAsync($"/dividas{divida.Id}");

        // Then
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Não é possível arquivar uma dívida técnica resolvida", corpo);
    }
}
