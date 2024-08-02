using Common.Infra;
using OrderMS.Common.Models;

namespace OrderMS.Test;

public sealed class LoggingTest
{

	[Fact]
    public void TestLogging()
    {
        ILogging logging = LoggingHelper.Init(true, 1000, "test");

        CustomerOrderModel com = new(){ customer_id = 1, next_order_id = 1 };

        for(int i = 0; i < 10; i++)
            logging.Append(com);

        Thread.Sleep(2001);

        logging.Dispose();

        Thread.Sleep(1001);

        string docPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var path = Path.Combine(docPath, "test.data");

        char[] buffer;
        using(var sr = new StreamReader(path))
        {
            buffer = new char[(int) sr.BaseStream.Length];
            sr.Read(buffer, 0, (int) sr.BaseStream.Length);
        }

        Console.WriteLine($"Buffer size: {buffer.Length}");
        Console.WriteLine(new string(buffer));

        Assert.True(buffer.Length == 360);

	}

}

