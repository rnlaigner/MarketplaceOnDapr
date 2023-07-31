using Microsoft.EntityFrameworkCore;

namespace OrderMS.Test
{
    // https://dba.stackexchange.com/questions/5236/is-there-a-way-to-access-temporary-tables-of-other-sessions-in-postgres
    public class TempTableTest : IClassFixture<TestDatabaseFixture>
    {

        public TempTableTest(TestDatabaseFixture fixture) => Fixture = fixture;

        public TestDatabaseFixture Fixture { get; }

        [Fact]
        public void TestTempTable()
        {
            try
            {
                using var context = Fixture.CreateContext();
                context.Database.ExecuteSqlRaw("CREATE TEMPORARY TABLE tx_products ( tid real, product_id real )"); //, ts timestamp with time zone ) ON COMMIT PRESERVE ROWS");
                var command = string.Format( "INSERT INTO tx_products (tid,product_id,ts) VALUES ({0},{1})", 1,1 );
                context.Database.ExecuteSqlRaw(command);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: {0}", e.Message);
            }
         
        }
    }
}

