using log4net;

namespace XtFocusOnBaseIntegAPI
{
    public sealed class Logger : Ilogger
    {
        #region ===[ Private Members ]=============================================================

        //   private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Logger));
        private static readonly Lazy<Logger> _loggerInstance = new Lazy<Logger>(() => new Logger());

        private const string ExceptionName = "Exception";
        private const string InnerExceptionName = "Inner Exception";
        private const string ExceptionMessageWithoutInnerException = "{0}{1}: {2}Message: {3}{4}StackTrace: {5}.";
        private const string ExceptionMessageWithInnerException = "{0}{1}{2}";


        #endregion
        public static Logger Instance
        {
            get { return _loggerInstance.Value; }
        }

        public void LogInfo(string filename, string message)
        {
            var appender = (log4net.Appender.RollingFileAppender)_logger.Logger.Repository.GetAppenders().FirstOrDefault(a => a.Name.Equals("RollingFileAppender"));

            if (appender != null)
            {
                appender.File = $"logs/Event_{filename}.log";
                appender.ActivateOptions();
            }

            _logger.Info(message);
        }

        public void LogError(string filename, string message, Exception ex)
        {
            var appender = (log4net.Appender.RollingFileAppender)_logger.Logger.Repository.GetAppenders().FirstOrDefault(a => a.Name.Equals("RollingFileAppender"));

            if (appender != null)
            {
                appender.File = $"logs/Error_{filename}_{DateTime.Now.ToString("ddMMyyyy")}.log";
                appender.ActivateOptions();
            }
            _logger.Error(message, ex);
        }
    }

}
