namespace EmojiTelegramBot.Logger
{
    class Logger : ILogger
    {
        protected NLog.Logger Log { get; set; }

        public Logger(string name)
        {
            Log = NLog.LogManager.GetLogger(name);
        }

        public void Trace(string message)
        {
            Log.Trace(message);
        }

        public void Info(string message)
        {
            Log.Info(message);
        }

        public void Warn(string message)
        {
            Log.Warn(message);
        }

        public void Error(string message)
        {
            Log.Error(message);
        }

        public void Fatal(string message)
        {
            Log.Fatal(message);
        }
    }
}
