using System.Collections.Generic;

namespace EmojiTelegramBot.Logger
{
    ///<inheritdoc cref="ILoggerService"/>
    class LoggerService : ILoggerService
    {
        public Dictionary<string, Logger> Loggers { get; protected set; }

        public LoggerService()
        {
            Loggers = new Dictionary<string, Logger>();
        }

        public ILogger Create(string name)
        {
            lock (Loggers)
            {
                if (!Loggers.ContainsKey(name))
                {
                    Loggers.Add(name, new Logger(name));
                }

                return Loggers[name];
            }
        }
    }
}
