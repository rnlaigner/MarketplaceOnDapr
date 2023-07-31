using Common.Infra;

namespace OrderMS.Test
{
    // https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-use-anonymous-pipes-for-local-interprocess-communication
    public class TestPipe
    {

        [Fact]
        public void Test()
        {
            PipePublisher publisher = PipePublisher.BuildPipePublisher();
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            var t = new Thread(new Runner(publisher, token).Run);
            t.IsBackground = true;
            t.Start();

        }

        private class Runner
        {
            private PipePublisher publisher;
            private CancellationToken token;

            public Runner(PipePublisher publisher, CancellationToken token)
            {
                this.publisher = publisher;
                this.token = token;
            }

            public void Run()
            {
                while (!token.IsCancellationRequested)
                {
                    var str = PipePublisher.WaitHandle.Take();
                    publisher.Write(str);
                }
            }
        }
    }
}