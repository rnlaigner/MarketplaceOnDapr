using System.Collections.Concurrent;
using System.Text.Json;

namespace Common.Infra;

public sealed class JsonLogging : ILogging
{
    private readonly ConcurrentQueue<object> recordsToLog;
    private readonly Thread loggingTask;
    private readonly StreamWriter outputFile;
    private readonly int delay;
    private readonly CancellationTokenSource cts;

    public JsonLogging(StreamWriter outputFile, int delay)
    {
        this.outputFile = outputFile;
        this.recordsToLog = new ConcurrentQueue<object>();
        this.loggingTask = new Thread(LoggingTask);
        this.delay = delay;
        this.cts = new CancellationTokenSource();
    }

    public void InitLoggingTask()
    {
        this.loggingTask.Start();
    }

    private void LoggingTask()
    {
        Console.WriteLine($"Logging thread ({Environment.CurrentManagedThreadId}) spawned.");
        do {
            Thread.Sleep(this.delay);
            Console.WriteLine($"Logging thread ({Environment.CurrentManagedThreadId}) task started.");
            int numRecords = this.recordsToLog.Count;
            while(numRecords > 0)
            {
                if(this.recordsToLog.TryDequeue(out var item)){
                    string itemStr = JsonSerializer.Serialize(item);
                    this.outputFile.WriteLine(itemStr);
                    numRecords--;
                }
            }
            Console.WriteLine($"Logging thread ({Environment.CurrentManagedThreadId}) task finished.");
        } while (!this.cts.IsCancellationRequested);
        this.outputFile.Close();
    }

    public void Append(object item)
    {
        this.recordsToLog.Enqueue(item);
    }

    public void Clear()
    {
        this.recordsToLog.Clear();
    }

    public void Dispose()
    {
        this.cts.Cancel();
        this.recordsToLog.Clear();
    }
}

