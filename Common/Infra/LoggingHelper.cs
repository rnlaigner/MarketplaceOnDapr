namespace Common.Infra;

public sealed class LoggingHelper
{
    public static ILogging Init(bool logging, int delay)
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
            var loggingInstance = new JsonLogging(outputFile, delay);
            loggingInstance.InitLoggingTask();
            return loggingInstance;
        }
        else
        {
            return DEFAULT_NO_LOGGING;
        }
    }

    private class DefaultNoLogging : ILogging { }

    private static readonly DefaultNoLogging DEFAULT_NO_LOGGING = new(); 
}


