using MysticMind.PostgresEmbed;
using OrderMS.Infra;
using Common.Utils;

namespace OrderMS.Test;

// https://learn.microsoft.com/en-us/ef/core/testing/testing-with-the-database
public class TestDatabaseFixture
{
    private const string ConnectionString = "Server=localhost;Port=5432;Database=postgres;User Id=postgres;Password=test";

    public OrderDbContext GetContext()
    {
        return context;
    }

    private static readonly OrderDbContext context;

    private static readonly PgServer server;

    static TestDatabaseFixture(){
        // create the server instance
        var instanceId = Utils.GetGuid("OrderDb");
        server = new PgServer("15.3.0", port: 5432, instanceId: instanceId);
        // start the server
        server.Start();
        context = new(ConnectionString);
        context.Database.EnsureCreated();
    }

    public TestDatabaseFixture()
    {
        
    }

    public void Dispose()
    {
        if (server != null)
        {
            server.Stop();
        }
    }

}
