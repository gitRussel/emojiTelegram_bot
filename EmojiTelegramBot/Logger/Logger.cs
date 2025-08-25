namespace EmojiTelegramBot.Logger
{
    class Logger : ILogger
    {
        protected Serilog.ILogger Log { get; set; }

        public Logger(string name)
        {
            Log = Serilog.Log.ForContext("SourceContext", name);
        }

        public void Trace(string message)
        {
            Log.Verbose(message);
        }

        public void Info(string message)
        {
            Log.Information(message);
        }

        public void Warn(string message)
        {
            Log.Warning(message);
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
