using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechDebtHub.Application.Abstractions.Persistence;
using TechDebtHub.Infrastructure.Persistence;

namespace TechDebtHub.Api.IntegrationTests.Common;

/// <summary>
/// Sobe a aplicação inteira (pipeline HTTP real) em memória usando <see cref="TestServer"/>,
/// substituindo o provedor SQL Server por SQLite in-memory para permitir testes de integração
/// ponta a ponta sem depender de um banco de dados externo.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public CustomWebApplicationFactory()
    {
        // O Program.cs (top-level statements) chama AddInfrastructure(builder.Configuration) de
        // forma síncrona, ANTES de "builder.Build()" ser executado. O mecanismo do
        // WebApplicationFactory que permite injetar configuração/serviços adicionais
        // (ConfigureWebHost -> ConfigureAppConfiguration/ConfigureServices) só é aplicado no
        // momento em que "builder.Build()" é interceptado via reflexão — ou seja, DEPOIS que
        // AddInfrastructure já rodou e já teria lançado InvalidOperationException por causa da
        // connection string vazia. Testado na prática: usar builder.ConfigureAppConfiguration
        // aqui não funciona para este cenário específico (Program.cs com top-level statements),
        // pois chega tarde demais.
        //
        // A única forma de fornecer um valor "dummy" a tempo é através de uma variável de
        // ambiente: os provedores de configuração padrão do ASP.NET Core (incluindo variáveis de
        // ambiente) são carregados já dentro de WebApplication.CreateBuilder(args), na primeira
        // linha do Program.cs — portanto antes de AddInfrastructure ser chamado.
        Environment.SetEnvironmentVariable("ConnectionStrings__SqlServer", "Data Source=dummy");
    }

    public async Task InitializeAsync()
    {
        // A conexão SQLite in-memory só existe enquanto estiver aberta. Ela precisa ser aberta
        // antes de qualquer requisição ser feita, pois é a mesma conexão reutilizada por todo o
        // ciclo de vida da factory (todas as requisições HTTP compartilham o mesmo banco).
        await _connection.OpenAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            RemoverServicosDoApplicationDbContext(services);

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));

            services.AddScoped<IApplicationDbContext>(sp =>
                sp.GetRequiredService<ApplicationDbContext>()
            );
        });
    }

    /// <summary>
    /// Remove todos os descritores relacionados ao ApplicationDbContext registrados pelo
    /// AddInfrastructure original (com UseSqlServer). Remover apenas o descritor exato de
    /// "DbContextOptions&lt;ApplicationDbContext&gt;" NÃO é suficiente: desde as versões mais
    /// recentes do EF Core, AddDbContext também registra internamente configurações do tipo
    /// "IDbContextOptionsConfiguration&lt;TContext&gt;" (uma por chamada de AddDbContext para o
    /// mesmo TContext), e todas elas são combinadas/aplicadas ao construir as DbContextOptions
    /// finais. Se a configuração antiga (UseSqlServer) não for removida, tanto o provedor
    /// SqlServer quanto o Sqlite acabam configurados simultaneamente na mesma DbContextOptions,
    /// o que o EF Core rejeita com "Services for database providers ... have been registered".
    /// Por isso removemos, de forma genérica, qualquer descritor cujo ServiceType feche
    /// (genericamente) sobre ApplicationDbContext, além dos tipos não genéricos óbvios.
    /// </summary>
    private static void RemoverServicosDoApplicationDbContext(IServiceCollection services)
    {
        var descritoresRelacionados = services
            .Where(d =>
                d.ServiceType == typeof(ApplicationDbContext)
                || d.ServiceType == typeof(IApplicationDbContext)
                || (
                    d.ServiceType.IsGenericType
                    && d.ServiceType.GetGenericArguments().Contains(typeof(ApplicationDbContext))
                )
            )
            .ToList();

        foreach (var descritor in descritoresRelacionados)
        {
            services.Remove(descritor);
        }
    }

    /// <summary>
    /// Recria o schema do banco (drop + create), garantindo que cada teste comece com o banco
    /// vazio sem precisar reconstruir toda a factory/host (que tem custo de bootstrap alto).
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        // WebApplicationFactory<TEntryPoint> já implementa IAsyncDisposable com um método
        // "ValueTask DisposeAsync()" próprio, então IAsyncLifetime.DisposeAsync (que exige
        // "Task DisposeAsync()") precisa ser implementado explicitamente para não colidir com o
        // membro público herdado.
        await _connection.DisposeAsync();
        await base.DisposeAsync();
    }
}
