using System.IO;
using System.Threading.Tasks;
using EmojiTelegramBot.Logger;

namespace EmojiTelegramBot.Jobs
{
	/// <inheritdoc cref="IJob"/>
	public class Webp2Gif : IJob
	{
		private string[] args;
		private ILogger _logger;

		public Webp2Gif(string[] args)
		{
			this.args = args;
			// Create a logger instance for this job
			_logger = new Logger.Logger("Webp2Gif");
		}

		public async Task<JobResult> DoJobAsync()
		{
			string pathToGif = Path.ChangeExtension(args[0], ".gif");
			var result = new JobResult(pathToGif, long.Parse(args[1]));

			_logger.Info($"Starting WebP to GIF conversion: {args[0]} -> {pathToGif}");

			await Task.Run(() =>
			{
				if (!File.Exists(pathToGif))
				{
					File.Move(args[0], pathToGif);
				}
			});

			return result;
		}
	}
}
