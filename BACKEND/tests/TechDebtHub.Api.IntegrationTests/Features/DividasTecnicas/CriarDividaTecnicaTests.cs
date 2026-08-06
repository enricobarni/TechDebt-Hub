using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Api.Contracts.DividasTecnicas;
using TechDebtHub.Api.IntegrationTests.Common;
using TechDebtHub.Application.Features.DividasTecnicas.CriarDividaTecnica;
using TechDebtHub.Application.Tests.Factories;
using TechDebtHub.Domain.Enums;

namespace TechDebtHub.Api.IntegrationTests.Features.DividasTecnicas;

public sealed class CriarDividaTecnicaTests : ApiIntegrationTestBase
{
    public CriarDividaTecnicaTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task Post_DividasTecnicas_ComDadosValidos_DeveRetornar201EPersistirComPontuacaoCalculada()
    {
        // Given
        var projeto = ProjetoFactory.Criar();

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            await context.SaveChangesAsync();
        });

        var request = new CriarDividaTecnicaRequest(
            "Consulta sem paginação",
            "A consulta retorna todos os registros de uma vez.",
            CategoriaDivida.Performance,
            NivelImpacto.Alto,
            NivelUrgencia.Alta,
            NivelFrequencia.Frequente,
            NivelEsforco.Medio
        );

        // When
        var response = await Client.PostAsJsonAsync($"/projetos/{projeto.Id}/dividas", request);

        // Then
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var corpo = await response.Content.ReadFromJsonAsync<CriarDividaTecnicaResponse>();

        Assert.NotNull(corpo);
        Assert.NotEqual(Guid.Empty, corpo!.Id);
        Assert.Equal(projeto.Id, corpo.ProjetoId);
        Assert.Equal(request.Titulo, corpo.Titulo);
        Assert.Equal(request.Descricao, corpo.Descricao);
        Assert.Equal(request.Categoria, corpo.Categoria);
        Assert.Equal(StatusDivida.Aberta, corpo.Status);
        Assert.False(corpo.Arquivada);
        Assert.Equal(request.Impacto, corpo.Impacto);
        Assert.Equal(request.Urgencia, corpo.Urgencia);
        Assert.Equal(request.Frequencia, corpo.Frequencia);
        Assert.Equal(request.Esforco, corpo.Esforco);

        // PontuacaoPrioridade = Impacto * Urgencia * Frequencia / Esforco = 3 * 3 * 3 / 2 = 13.5
        Assert.Equal(13.5m, corpo.PontuacaoPrioridade);

        Assert.NotNull(response.Headers.Location);
        Assert.Contains(corpo.Id.ToString(), response.Headers.Location!.ToString());

        var dividaPersistida = await UsingContextAsync(context =>
            context.DividasTecnicas.AsNoTracking().SingleAsync(d => d.Id == corpo.Id)
        );

        Assert.Equal(projeto.Id, dividaPersistida.ProjetoId);
        Assert.Equal(request.Titulo, dividaPersistida.Titulo);
        Assert.Equal(13.5m, dividaPersistida.PontuacaoPrioridade);
        Assert.False(dividaPersistida.Arquivada);
    }

    [Fact]
    public async Task Post_DividasTecnicas_ComProjetoInexistente_DeveRetornar404()
    {
        // Given
        var projetoInexistente = Guid.NewGuid();

        var request = new CriarDividaTecnicaRequest(
            "Consulta sem paginação",
            "A consulta retorna todos os registros de uma vez.",
            CategoriaDivida.Performance,
            NivelImpacto.Alto,
            NivelUrgencia.Alta,
            NivelFrequencia.Frequente,
            NivelEsforco.Medio
        );

        // When
        var response = await Client.PostAsJsonAsync(
            $"/projetos/{projetoInexistente}/dividas",
            request
        );

        // Then
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Projeto não encontrado", corpo);
    }

    [Fact]
    public async Task Post_DividasTecnicas_ComProjetoArquivado_DeveRetornar422()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        projeto.Arquivar();

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            await context.SaveChangesAsync();
        });

        var request = new CriarDividaTecnicaRequest(
            "Consulta sem paginação",
            "A consulta retorna todos os registros de uma vez.",
            CategoriaDivida.Performance,
            NivelImpacto.Alto,
            NivelUrgencia.Alta,
            NivelFrequencia.Frequente,
            NivelEsforco.Medio
        );

        // When
        var response = await Client.PostAsJsonAsync($"/projetos/{projeto.Id}/dividas", request);

        // Then
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Não é possível criar uma dívida em um projeto arquivado", corpo);
    }

    [Fact]
    public async Task Post_DividasTecnicas_ComTituloDuplicadoNoMesmoProjeto_DeveRetornar409()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        var dividaExistente = DividaTecnicaFactory.Criar(projeto.Id, "Consulta sem paginação");

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            context.DividasTecnicas.Add(dividaExistente);
            await context.SaveChangesAsync();
        });

        var request = new CriarDividaTecnicaRequest(
            "consulta   sem   paginação",
            "Outra descrição para a mesma dívida.",
            CategoriaDivida.Performance,
            NivelImpacto.Alto,
            NivelUrgencia.Alta,
            NivelFrequencia.Frequente,
            NivelEsforco.Medio
        );

        // When
        var response = await Client.PostAsJsonAsync($"/projetos/{projeto.Id}/dividas", request);

        // Then
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Já existe uma dívida com esse título neste projeto", corpo);

        var totalDividas = await UsingContextAsync(context =>
            context.DividasTecnicas.AsNoTracking().CountAsync(d => d.ProjetoId == projeto.Id)
        );
        Assert.Equal(1, totalDividas);
    }
}
