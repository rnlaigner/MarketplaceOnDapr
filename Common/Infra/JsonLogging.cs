using System.Collections.Concurrent;
using System.Text.Json;

namespace Common.Infra;

public sealed class JsonLogging<T> : ILogging<T>, IDisposable
{
    private readonly ConcurrentQueue<T> recordsToLog;
    private readonly Thread loggingTask;
    private readonly StreamWriter outputFile;
    private readonly int delay;

    public JsonLogging(StreamWriter outputFile, int delay)
    {
        this.outputFile = outputFile;
        this.recordsToLog = new ConcurrentQueue<T>();
        this.loggingTask = new Thread(LoggingTask);
        this.delay = delay;
    }

    public void InitLoggingTask()
    {
        this.loggingTask.Start();
    }

    private void LoggingTask()
    {
        Console.WriteLine("Logging Thread Spawned.");
        do {
            Thread.Sleep(this.delay);
            Console.WriteLine("Logging task started.");
            int numRecords = this.recordsToLog.Count;
            while(numRecords > 0)
            {
                if(this.recordsToLog.TryDequeue(out var item)){
                    string itemStr = JsonSerializer.Serialize(item);
                    this.outputFile.WriteLine(itemStr);
                    numRecords--;
                }
            }
            Console.WriteLine("Logging task finished.");
        } while (true);
    }

    public void Append(T item)
    {
        this.recordsToLog.Enqueue(item);
    }

    public void Clear()
    {
        this.recordsToLog.Clear();
    }

    public void Dispose()
    {
        this.loggingTask.Interrupt();
        this.outputFile.Close();
    }
}

