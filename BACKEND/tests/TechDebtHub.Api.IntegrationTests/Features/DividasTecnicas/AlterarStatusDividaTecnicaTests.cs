using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Api.Contracts.DividasTecnicas;
using TechDebtHub.Api.IntegrationTests.Common;
using TechDebtHub.Application.Features.DividasTecnicas.AlterarStatusDividaTecnica;
using TechDebtHub.Application.Tests.Factories;
using TechDebtHub.Domain.Enums;

namespace TechDebtHub.Api.IntegrationTests.Features.DividasTecnicas;

public sealed class AlterarStatusDividaTecnicaTests : ApiIntegrationTestBase
{
    public AlterarStatusDividaTecnicaTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task Patch_DividasTecnicas_ComTransicaoValida_DeveRetornar200EPreencherDataAtualizacao()
    {
        // Given - transição válida a partir do status inicial (Aberta -> EmAnalise)
        var projeto = ProjetoFactory.Criar();
        var divida = DividaTecnicaFactory.Criar(projeto.Id);

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            context.DividasTecnicas.Add(divida);
            await context.SaveChangesAsync();
        });

        var request = new AlterarStatusDividaTecnicaRequest(StatusDivida.EmAnalise);

        // When
        var response = await Client.PatchAsJsonAsync($"/dividas/{divida.Id}/status", request);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var corpo = await response.Content.ReadFromJsonAsync<AlterarStatusDividaTecnicaResponse>();

        Assert.NotNull(corpo);
        Assert.Equal(divida.Id, corpo!.Id);
        Assert.Equal(StatusDivida.EmAnalise, corpo.Status);
        Assert.NotNull(corpo.DataAtualizacao);
        Assert.Null(corpo.DataResolucao);

        var dividaPersistida = await UsingContextAsync(context =>
            context.DividasTecnicas.AsNoTracking().SingleAsync(d => d.Id == divida.Id)
        );

        Assert.Equal(StatusDivida.EmAnalise, dividaPersistida.Status);
        Assert.NotNull(dividaPersistida.DataAtualizacao);
    }

    [Fact]
    public async Task Patch_DividasTecnicas_ComTransicaoParaResolvidaAPartirDeEmAndamento_DeveRetornar200EPreencherDataResolucao()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        var divida = DividaTecnicaFactory.Criar(projeto.Id);
        divida.AlterarStatus(StatusDivida.EmAnalise);
        divida.AlterarStatus(StatusDivida.Planejada);
        divida.AlterarStatus(StatusDivida.EmAndamento);

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            context.DividasTecnicas.Add(divida);
            await context.SaveChangesAsync();
        });

        var request = new AlterarStatusDividaTecnicaRequest(StatusDivida.Resolvida);

        // When
        var response = await Client.PatchAsJsonAsync($"/dividas/{divida.Id}/status", request);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var corpo = await response.Content.ReadFromJsonAsync<AlterarStatusDividaTecnicaResponse>();

        Assert.NotNull(corpo);
        Assert.Equal(StatusDivida.Resolvida, corpo!.Status);
        Assert.NotNull(corpo.DataAtualizacao);
        Assert.NotNull(corpo.DataResolucao);

        var dividaPersistida = await UsingContextAsync(context =>
            context.DividasTecnicas.AsNoTracking().SingleAsync(d => d.Id == divida.Id)
        );

        Assert.Equal(StatusDivida.Resolvida, dividaPersistida.Status);
        Assert.NotNull(dividaPersistida.DataResolucao);
    }

    [Fact]
    public async Task Patch_DividasTecnicas_ComIdInexistente_DeveRetornar404()
    {
        // Given
        var idInexistente = Guid.NewGuid();
        var request = new AlterarStatusDividaTecnicaRequest(StatusDivida.EmAnalise);

        // When
        var response = await Client.PatchAsJsonAsync($"/dividas/{idInexistente}/status", request);

        // Then
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Dívida técnica não encontrada", corpo);
    }

    [Fact]
    public async Task Patch_DividasTecnicas_ComDividaArquivada_DeveRetornar422()
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

        var request = new AlterarStatusDividaTecnicaRequest(StatusDivida.EmAnalise);

        // When
        var response = await Client.PatchAsJsonAsync($"/dividas/{divida.Id}/status", request);

        // Then
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("A dívida técnica já está arquivada", corpo);
    }

    [Fact]
    public async Task Patch_DividasTecnicas_ComTransicaoInvalida_DeveRetornar422()
    {
        // Given - a partir de "Aberta" não é permitido ir direto para "Resolvida"
        var projeto = ProjetoFactory.Criar();
        var divida = DividaTecnicaFactory.Criar(projeto.Id);

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            context.DividasTecnicas.Add(divida);
            await context.SaveChangesAsync();
        });

        var request = new AlterarStatusDividaTecnicaRequest(StatusDivida.Resolvida);

        // When
        var response = await Client.PatchAsJsonAsync($"/dividas/{divida.Id}/status", request);

        // Then
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Não é permitido alterar o status de Aberta para Resolvida", corpo);
    }
}
