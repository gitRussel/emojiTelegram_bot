﻿namespace EmojiTelegramBot.Configuration
{
    /// <summary>
    /// Configuration
    /// </summary>
    public interface ICustomConfiguration
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
        /// Api token Telegram bot.
        /// </summary>
        string ApiBotToken { get; }
    }
}
