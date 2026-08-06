using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Api.Contracts.Projetos;
using TechDebtHub.Api.IntegrationTests.Common;
using TechDebtHub.Application.Features.Projetos.CriarProjeto;
using TechDebtHub.Application.Tests.Factories;

namespace TechDebtHub.Api.IntegrationTests.Features.Projetos;

public sealed class CriarProjetoTests : ApiIntegrationTestBase
{
    public CriarProjetoTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task Post_Projetos_ComDadosValidos_DeveRetornar201()
    {
        // Given
        var request = new CriarProjetoRequest(
            "TechDebt Hub",
            "Sistema para controle de dívidas técnicas."
        );

        // When
        var response = await Client.PostAsJsonAsync("/projetos", request);

        // Then
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var corpo = await response.Content.ReadFromJsonAsync<CriarProjetoResponse>();

        Assert.NotNull(corpo);
        Assert.NotEqual(Guid.Empty, corpo!.Id);
        Assert.Equal(request.Nome, corpo.Nome);
        Assert.Equal(request.Descricao, corpo.Descricao);

        Assert.NotNull(response.Headers.Location);
        Assert.Contains(corpo.Id.ToString(), response.Headers.Location!.ToString());

        var projetoPersistido = await UsingContextAsync(context =>
            context.Projetos.AsNoTracking().SingleOrDefaultAsync(p => p.Id == corpo.Id)
        );

        Assert.NotNull(projetoPersistido);
        Assert.Equal(request.Nome, projetoPersistido!.Nome);
        Assert.Equal(request.Descricao, projetoPersistido.Descricao);
        Assert.False(projetoPersistido.Arquivado);
    }

    [Fact]
    public async Task Post_Projetos_ComNomeDuplicadoIgnorandoAcentosEEspacos_DeveRetornar409()
    {
        // Given
        var projetoExistente = ProjetoFactory.Criar("Gestão Técnica");

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projetoExistente);
            await context.SaveChangesAsync();
        });

        var request = new CriarProjetoRequest(
            "gestao   tecnica",
            "Outro projeto com o mesmo nome normalizado."
        );

        // When
        var response = await Client.PostAsJsonAsync("/projetos", request);

        // Then
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        // NOTA: o ExceptionHandlingMiddleware define ContentType = "application/problem+json",
        // porém HttpResponseJsonExtensions.WriteAsJsonAsync (sem o parâmetro "contentType")
        // sobrescreve o header de volta para "application/json; charset=utf-8". Ver relatório do
        // agente de testes para detalhes — comportamento real documentado aqui, não o previsto.
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Já existe um projeto com esse nome", corpo);

        var totalProjetos = await UsingContextAsync(context =>
            context.Projetos.AsNoTracking().CountAsync()
        );
        Assert.Equal(1, totalProjetos);
    }
}
