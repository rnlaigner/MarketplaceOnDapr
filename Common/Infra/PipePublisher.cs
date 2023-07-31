using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Pipes;

namespace Common.Infra
{
	public class PipePublisher
	{
        private readonly Process pipeClient;
        private readonly StreamWriter sw;

        private PipePublisher(Process pipeClient, StreamWriter sw) {
            this.pipeClient = pipeClient;
            this.sw = sw;
        }

        public static BlockingCollection<string> WaitHandle = new BlockingCollection<string>();

        public static PipePublisher BuildPipePublisher()
		{
            Process pipeClient = new Process();
            pipeClient.StartInfo.FileName = "pipeClient.exe";
            AnonymousPipeServerStream pipeServer = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);

            pipeClient.StartInfo.Arguments = pipeServer.GetClientHandleAsString();
            pipeClient.StartInfo.UseShellExecute = false;
            pipeClient.Start();

            pipeServer.DisposeLocalCopyOfClientHandle();

            StreamWriter sw = new StreamWriter(pipeServer);
            sw.AutoFlush = true;

            return new PipePublisher(pipeClient, sw);
        }

        public void Write(string message)
        {
            this.sw.WriteLine(message);
        }

        public void Dispose() {
            // this.pipeClient.WaitForExit();
            this.pipeClient.Close();
        }

	}
}

