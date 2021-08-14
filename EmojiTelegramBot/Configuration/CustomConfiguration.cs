namespace EmojiTelegramBot.Configuration
{
	/// <inheritdoc cref="ICustomConfiguration"/>
	public class CustomConfiguration : ICustomConfiguration
	{
		public int ParallelCount { get; set; }

		public string PathToGifDirectory { get; set; }

		public string ApiBotToken { get; set; }
	}
}
