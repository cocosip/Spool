using Microsoft.Extensions.Logging;

namespace Spool.Tests.Group
{
    public static class LoggerHelper
    {
        public static ILogger<T> GetLogger<T>()
        {
            return new EmptyLogger<T>();
        }
    }
}
