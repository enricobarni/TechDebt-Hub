using System.Net;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Api.IntegrationTests.Common;
using TechDebtHub.Application.Tests.Factories;

namespace TechDebtHub.Api.IntegrationTests.Features.Projetos;

public sealed class ArquivarProjetoTests : ApiIntegrationTestBase
{
    public ArquivarProjetoTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task Delete_Projetos_ComProjetoSemDividasAtivas_DeveRetornar204EArquivar()
    {
        // Given
        var projeto = ProjetoFactory.Criar();

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            await context.SaveChangesAsync();
        });

        // When
        var response = await Client.DeleteAsync($"/projetos/{projeto.Id}");

        // Then
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var projetoPersistido = await UsingContextAsync(context =>
            context.Projetos.AsNoTracking().SingleAsync(p => p.Id == projeto.Id)
        );

        Assert.True(projetoPersistido.Arquivado);
        Assert.NotNull(projetoPersistido.DataAtualizacao);
    }

    [Fact]
    public async Task Delete_Projetos_ComIdInexistente_DeveRetornar404()
    {
        // Given
        var idInexistente = Guid.NewGuid();

        // When
        var response = await Client.DeleteAsync($"/projetos/{idInexistente}");

        // Then
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Projetos_ComDividaTecnicaAtiva_DeveRetornar422ENaoArquivar()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        var dividaAtiva = DividaTecnicaFactory.Criar(projeto.Id);

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            context.DividasTecnicas.Add(dividaAtiva);
            await context.SaveChangesAsync();
        });

        // When
        var response = await Client.DeleteAsync($"/projetos/{projeto.Id}");

        // Then
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains(
            "Não é possível arquivar um projeto que possui dívidas técnicas ativas",
            corpo
        );

        var projetoPersistido = await UsingContextAsync(context =>
            context.Projetos.AsNoTracking().SingleAsync(p => p.Id == projeto.Id)
        );

        Assert.False(projetoPersistido.Arquivado);
    }
}
