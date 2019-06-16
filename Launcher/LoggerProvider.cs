using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;

namespace Launcher
{
    public class LoggerProvider
    {
        public static ILog GetLogger<T>()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            return LogManager.GetLogger(typeof(T));
        }
    }
}
