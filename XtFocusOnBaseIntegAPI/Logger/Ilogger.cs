namespace XtFocusOnBaseIntegAPI
{
    public interface Ilogger
    {
        void LogInfo(string filename, string message);
        void LogError(string filename, string message, Exception ex);
    }
}
