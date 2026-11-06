using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Zuhid.Template.Tests;

// In-memory Sqlite kept alive by holding the connection open; one per test.
public sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;
    public TemplateContext Context { get; }

    public TestDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        Context = new TemplateContext(new DbContextOptionsBuilder<TemplateContext>().UseSqlite(_connection).Options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
