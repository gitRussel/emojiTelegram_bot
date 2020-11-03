namespace EmojiTelegramBot.Logger
{
    public interface ILoggerService
    {
        /// <summary>
        /// Create logger
        /// </summary>
        ILogger Create(string name);
    }
}
