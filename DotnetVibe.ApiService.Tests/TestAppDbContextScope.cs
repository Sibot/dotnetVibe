using DotnetVibe.ApiService.Data;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DotnetVibe.ApiService.Tests;

internal sealed class TestAppDbContextScope : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public AppDbContext Db { get; }

    private TestAppDbContextScope(SqliteConnection connection, AppDbContext db)
    {
        _connection = connection;
        Db = db;
    }

    public static async Task<TestAppDbContextScope> CreateAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqliteConnection($"Data Source=test-{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
        await connection.OpenAsync(cancellationToken);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync(cancellationToken);
        return new TestAppDbContextScope(connection, db);
    }

    public async ValueTask DisposeAsync()
    {
        await Db.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
