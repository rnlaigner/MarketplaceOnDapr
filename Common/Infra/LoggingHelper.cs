namespace Common.Infra;

public sealed class LoggingHelper<T>
{

    public static ILogging<T> Init(bool logging, int delay)
    { 
        if(logging)
        {
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var path = Path.Combine(docPath, "json.data");
            // make sure file is created
            var file = File.Open(path.ToString(), FileMode.OpenOrCreate, FileAccess.ReadWrite);
            file.Close();
            // make sure file is truncated
            file = File.Open(path.ToString(), FileMode.Truncate, FileAccess.ReadWrite);
            file.Close();
            StreamWriter outputFile = new StreamWriter(path);
            // disable write to disk on every append
            outputFile.AutoFlush = false;
            // outputFile.WriteLine("TEST");
        
            var loggingInstance = new JsonLogging<T>(outputFile, delay);
            loggingInstance.InitLoggingTask();
            return loggingInstance;
        } else
        {
            return new DefaultNoLogging<T>();
        }
    }

    private class DefaultNoLogging<A> : ILogging<A> { }

}


