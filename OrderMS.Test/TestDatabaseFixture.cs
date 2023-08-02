using OrderMS.Infra;

namespace OrderMS.Test;

// https://learn.microsoft.com/en-us/ef/core/testing/testing-with-the-database
public class TestDatabaseFixture
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=password";

    public OrderDbContext CreateContext() => new(ConnectionString);

    private static readonly object _lock = new();
    private static bool _databaseInitialized;

    public TestDatabaseFixture()
    {
        lock (_lock)
        {
            if (!_databaseInitialized)
            {
                using (var context = CreateContext())
                {
                    context.Database.EnsureCreated();
                    _databaseInitialized = true;
                }
            }
        }
    }
}
