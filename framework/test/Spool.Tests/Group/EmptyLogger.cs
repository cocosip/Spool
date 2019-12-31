using Microsoft.Extensions.Logging;
using System;

namespace Spool.Tests.Group
{
    public class EmptyLogger<T> : ILogger<T>
    {
        public EmptyLogger()
        {
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {

        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return default(IDisposable);
        }

    }


}
