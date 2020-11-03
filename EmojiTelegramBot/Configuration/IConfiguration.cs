namespace EmojiTelegramBot.Configuration
{
    /// <summary>
    /// Configuration
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// Number of messages processed in parallel.
        /// </summary>
        int ParallelCount { get; }

        /// <summary>
        /// Path to the directory with cached gifs.
        /// </summary>
        string PathToGifDirectory { get; }

        /// <summary>
        /// Proxies to bypass locks.
        /// </summary>
        string ProxyHostName { get; }

        /// <summary>
        /// Proxy port.
        /// </summary>
        int ProxyPort { get; }

        /// <summary>
        /// Api token Telegram bot.
        /// </summary>
        string ApiBotToken { get; }
    }
}
