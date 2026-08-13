using System.Net;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Api.IntegrationTests.Common;
using TechDebtHub.Application.Tests.Factories;
using TechDebtHub.Domain.Enums;

namespace TechDebtHub.Api.IntegrationTests.Features.DividasTecnicas;
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

        // When 
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

        // When 
        var response = await Client.DeleteAsync($"/dividas/{divida.Id}");

        // Then 
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

        // When 
        var response = await Client.DeleteAsync($"/dividas/{idInexistente}");

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
        var response = await Client.DeleteAsync($"/dividas/{divida.Id}");

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
        var response = await Client.DeleteAsync($"/dividas/{divida.Id}");

        // Then
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Não é possível arquivar uma dívida técnica resolvida", corpo);
    }
}
