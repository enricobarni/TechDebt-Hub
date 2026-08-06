using Microsoft.Extensions.DependencyInjection;
using TechDebtHub.Infrastructure.Persistence;

namespace TechDebtHub.Api.IntegrationTests.Common;

/// <summary>
/// Classe base para testes de integração HTTP ponta a ponta. Cada classe de teste que herdar
/// desta base compartilha a mesma <see cref="CustomWebApplicationFactory"/> (via
/// <see cref="IClassFixture{TFixture}"/>, evitando o custo de subir o host a cada teste), mas
/// tem o banco de dados resetado antes de cada teste individual (<see cref="InitializeAsync"/>),
/// garantindo isolamento entre eles.
/// </summary>
public abstract class ApiIntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    // Reutiliza as mesmas factories de entidades de domínio já usadas em
    // TechDebtHub.Application.Tests (ex.: ProjetoFactory, DividaTecnicaFactory) em vez de
    // duplicar a lógica de criação de dados de teste aqui. Optou-se por uma ProjectReference ao
    // projeto TechDebtHub.Application.Tests (ver .csproj) por ser mais simples e evitar
    // divergência entre os dois projetos de teste; a alternativa (duplicar/adaptar helpers)
    // aumentaria o risco de as factories ficarem dessincronizadas com o tempo.
    protected readonly CustomWebApplicationFactory Factory;

    protected HttpClient Client { get; }

    protected ApiIntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        // A CustomWebApplicationFactory (host + conexão SQLite) é compartilhada por toda a
        // classe de teste via IClassFixture e só é descartada quando a classe inteira termina,
        // então não há nada para limpar aqui a cada teste individual.
        return Task.CompletedTask;
    }

    /// <summary>
    /// Cria um escopo de DI, resolve o <see cref="ApplicationDbContext"/> real usado pela
    /// aplicação sob teste e executa <paramref name="action"/> contra ele — útil para popular
    /// (ou inspecionar) dados diretamente no banco sem depender de chamadas HTTP redundantes.
    /// </summary>
    protected async Task<TResult> UsingContextAsync<TResult>(
        Func<ApplicationDbContext, Task<TResult>> action
    )
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await action(context);
    }

    /// <summary>
    /// Sobrecarga sem retorno de <see cref="UsingContextAsync{TResult}"/>, para ações que apenas
    /// alteram o estado do banco (ex.: inserir dados de setup) sem precisar de um valor de volta.
    /// </summary>
    protected async Task UsingContextAsync(Func<ApplicationDbContext, Task> action)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await action(context);
    }
}
