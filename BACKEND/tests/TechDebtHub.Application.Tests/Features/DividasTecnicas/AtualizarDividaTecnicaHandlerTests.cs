using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Exceptions;
using TechDebtHub.Application.Features.DividasTecnicas.AtualizarDividaTecnica;
using TechDebtHub.Application.Tests.Common;
using TechDebtHub.Application.Tests.Factories;
using TechDebtHub.Domain.Enums;
using TechDebtHub.Domain.Exceptions;
using Xunit;

namespace TechDebtHub.Application.Tests.Features.DividasTecnicas;

public sealed class AtualizarDividaTecnicaHandlerTests : ApplicationTestBase
{
    [Fact]
    public async Task HandleAsync_DividaInexistente_DeveLancarNotFoundException()
    {
        // Given
        var handler = new AtualizarDividaTecnicaHandler(Context);
        var command = new AtualizarDividaTecnicaCommand(
            Guid.NewGuid(),
            "Consulta sem paginação",
            "A consulta retorna todos os registros.",
            CategoriaDivida.Performance,
            NivelImpacto.Alto,
            NivelUrgencia.Media,
            NivelFrequencia.Constante,
            NivelEsforco.Medio
        );

        // When
        Func<Task> acaoDividaInexistente = () =>
            handler.HandleAsync(command, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<NotFoundException>(acaoDividaInexistente);
        Assert.Equal("Dívida técnica não encontrada", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_TituloPertenceAOutraDividaDoMesmoProjeto_DeveLancarConflictException()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);

        var dividaUm = DividaTecnicaFactory.Criar(projeto.Id, titulo: "Consulta lenta");
        var dividaDois = DividaTecnicaFactory.Criar(projeto.Id, titulo: "Índice ausente");
        Context.DividasTecnicas.AddRange(dividaUm, dividaDois);

        await Context.SaveChangesAsync();

        var handler = new AtualizarDividaTecnicaHandler(Context);
        var command = new AtualizarDividaTecnicaCommand(
            dividaDois.Id,
            "consulta    lenta",
            "Nova descrição.",
            CategoriaDivida.Performance,
            NivelImpacto.Alto,
            NivelUrgencia.Media,
            NivelFrequencia.Constante,
            NivelEsforco.Medio
        );

        // When
        Func<Task> acaoTituloDuplicado = () =>
            handler.HandleAsync(command, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<ConflictException>(acaoTituloDuplicado);
        Assert.Equal(
            "Já existe outra dívida com esse título neste projeto.",
            exception.Message
        );
    }

    [Fact]
    public async Task HandleAsync_MantendoProprioTitulo_DeveAtualizarSemConflito()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);

        var divida = DividaTecnicaFactory.Criar(projeto.Id, titulo: "Consulta lenta");
        Context.DividasTecnicas.Add(divida);

        await Context.SaveChangesAsync();

        var handler = new AtualizarDividaTecnicaHandler(Context);
        var command = new AtualizarDividaTecnicaCommand(
            divida.Id,
            "consulta    lenta",
            "Nova descrição.",
            CategoriaDivida.Performance,
            NivelImpacto.Alto,
            NivelUrgencia.Media,
            NivelFrequencia.Constante,
            NivelEsforco.Medio
        );

        // When
        await handler.HandleAsync(command, CancellationToken.None);

        Context.ChangeTracker.Clear();

        // Then
        var dividaSalva = await Context
            .DividasTecnicas.AsNoTracking()
            .SingleAsync(divida => divida.Id == command.Id);

        Assert.Equal("consulta lenta", dividaSalva.Titulo);
        Assert.Equal("CONSULTA LENTA", dividaSalva.TituloNormalizado);
        Assert.Equal("Nova descrição.", dividaSalva.Descricao);
    }

    [Fact]
    public async Task HandleAsync_DividaArquivada_DeveLancarDomainException()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);

        var divida = DividaTecnicaFactory.Criar(projeto.Id);
        divida.Arquivar();
        Context.DividasTecnicas.Add(divida);

        await Context.SaveChangesAsync();

        var handler = new AtualizarDividaTecnicaHandler(Context);
        var command = new AtualizarDividaTecnicaCommand(
            divida.Id,
            "Novo título",
            "Nova descrição.",
            CategoriaDivida.Performance,
            NivelImpacto.Alto,
            NivelUrgencia.Media,
            NivelFrequencia.Constante,
            NivelEsforco.Medio
        );

        // When
        Func<Task> acaoDividaArquivada = () =>
            handler.HandleAsync(command, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<DomainException>(acaoDividaArquivada);
        Assert.Equal("A dívida técnica já está arquivada", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_DividaResolvida_DeveLancarDomainException()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);

        var divida = DividaTecnicaFactory.Criar(projeto.Id);
        divida.AlterarStatus(StatusDivida.EmAnalise);
        divida.AlterarStatus(StatusDivida.Planejada);
        divida.AlterarStatus(StatusDivida.EmAndamento);
        divida.AlterarStatus(StatusDivida.Resolvida);
        Context.DividasTecnicas.Add(divida);

        await Context.SaveChangesAsync();

        var handler = new AtualizarDividaTecnicaHandler(Context);
        var command = new AtualizarDividaTecnicaCommand(
            divida.Id,
            "Novo título",
            "Nova descrição.",
            CategoriaDivida.Performance,
            NivelImpacto.Alto,
            NivelUrgencia.Media,
            NivelFrequencia.Constante,
            NivelEsforco.Medio
        );

        // When
        Func<Task> acaoDividaResolvida = () =>
            handler.HandleAsync(command, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<DomainException>(acaoDividaResolvida);
        Assert.Equal(
            "Não é possível alterar uma dívida técnica já resolvida",
            exception.Message
        );
    }

    [Fact]
    public async Task HandleAsync_ComDadosValidos_DeveAtualizarTodosOsCamposEPersistirAlteracoes()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);

        var divida = DividaTecnicaFactory.Criar(projeto.Id);
        Context.DividasTecnicas.Add(divida);

        await Context.SaveChangesAsync();

        var handler = new AtualizarDividaTecnicaHandler(Context);
        var command = new AtualizarDividaTecnicaCommand(
            divida.Id,
            "Título atualizado",
            "Descrição atualizada.",
            CategoriaDivida.Seguranca,
            NivelImpacto.Critico,
            NivelUrgencia.Imediata,
            NivelFrequencia.Rara,
            NivelEsforco.MuitoAlto
        );

        // When
        var response = await handler.HandleAsync(command, CancellationToken.None);

        Context.ChangeTracker.Clear();

        // Then
        var dividaSalva = await Context
            .DividasTecnicas.AsNoTracking()
            .SingleAsync(divida => divida.Id == command.Id);

        Assert.Equal(command.Id, response.Id);
        Assert.Equal("Título atualizado", dividaSalva.Titulo);
        Assert.Equal("TITULO ATUALIZADO", dividaSalva.TituloNormalizado);
        Assert.Equal("Descrição atualizada.", dividaSalva.Descricao);
        Assert.Equal(CategoriaDivida.Seguranca, dividaSalva.Categoria);
        Assert.Equal(NivelImpacto.Critico, dividaSalva.Impacto);
        Assert.Equal(NivelUrgencia.Imediata, dividaSalva.Urgencia);
        Assert.Equal(NivelFrequencia.Rara, dividaSalva.Frequencia);
        Assert.Equal(NivelEsforco.MuitoAlto, dividaSalva.Esforco);

        // NivelImpacto.Critico (4) * NivelUrgencia.Imediata (4) * NivelFrequencia.Rara (1) / NivelEsforco.MuitoAlto (4)
        Assert.Equal(4, dividaSalva.PontuacaoPrioridade);
        Assert.NotNull(dividaSalva.DataAtualizacao);
    }
}
