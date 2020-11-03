namespace EmojiTelegramBot.Logger
{
    public interface ILogger
    {
        void Trace(string name);

        void Info(string name);

        void Warn(string name);

        void Error(string name);

        void Fatal(string name);
    }
}
