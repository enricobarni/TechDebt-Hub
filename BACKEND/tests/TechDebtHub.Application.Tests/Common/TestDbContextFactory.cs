using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Infrastructure.Persistence;

namespace TechDebtHub.Application.Tests.Common;

public sealed class TestDbContextFactory : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public ApplicationDbContext Context { get; }

    private TestDbContextFactory(
        SqliteConnection connection,
        DbContextOptions<ApplicationDbContext> options,
        ApplicationDbContext context
    )
    {
        _connection = connection;
        _options = options;
        Context = context;
    }

    public static async Task<TestDbContextFactory> CriarAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;

        var context = new ApplicationDbContext(options);

        await context.Database.EnsureCreatedAsync();

        return new TestDbContextFactory(connection, options, context);
    }

    public ApplicationDbContext CriarNovoContexto()
    {
        return new ApplicationDbContext(_options);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
