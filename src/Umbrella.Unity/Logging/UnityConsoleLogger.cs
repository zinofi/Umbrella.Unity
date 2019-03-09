using Microsoft.Extensions.Logging;
using System;
using System.Text;

namespace Umbrella.Unity.Logging
{
    public class UnityConsoleLogger : Microsoft.Extensions.Logging.ILogger
    {
        private readonly string m_CategoryName;

        public static LogLevel LogLevel { get; set; } = LogLevel.None;

        public UnityConsoleLogger(string categoryName)
        {
            m_CategoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state)
            => null;

        public bool IsEnabled(LogLevel logLevel)
            => logLevel >= LogLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            if (formatter != null)
            {
                string message = formatter(state, exception);

                StringBuilder sbMessage = new StringBuilder(message);

                if (!string.IsNullOrWhiteSpace(exception?.Message))
                {
                    sbMessage
                        .AppendLine()
                        .AppendLine($"{exception.GetType().FullName}: {exception.Message}");
                }

                LogStackTrace(exception, sbMessage);

                var innerException = exception?.InnerException;

                while (innerException != null)
                {
                    if (!string.IsNullOrWhiteSpace(innerException.Message))
                    {
                        sbMessage
                            .AppendLine()
                            .AppendLine($"{innerException.GetType().FullName}: {innerException.Message}");
                    }

                    LogStackTrace(innerException, sbMessage);

                    innerException = innerException.InnerException;
                }

                string messageToLog = sbMessage.ToString();

                switch (logLevel)
                {
                    case LogLevel.Trace:
                    case LogLevel.Debug:
                    case LogLevel.Information:
                        UnityEngine.Debug.Log(messageToLog);
                        break;
                    case LogLevel.Warning:
                        UnityEngine.Debug.LogWarning(messageToLog);
                        break;
                    case LogLevel.Error:
                    case LogLevel.Critical:
                        UnityEngine.Debug.LogError(messageToLog);
                        break;
                }
            }
        }

        private static void LogStackTrace(Exception exception, StringBuilder sbMessage)
        {
            if (!string.IsNullOrWhiteSpace(exception?.StackTrace))
            {
                sbMessage
                    .AppendLine()
                    .AppendLine($"********** Stack Trace: {exception.GetType().FullName} **********")
                    .AppendLine()
                    .AppendLine(exception?.StackTrace)
                    .AppendLine()
                    .AppendLine("******************************************************************");
            }
        }
    }
}