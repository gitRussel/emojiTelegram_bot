using System.IO;
using System.Threading.Tasks;

namespace EmojiTelegramBot.Jobs
{
	/// <inheritdoc cref="IJob"/>
	public class Webp2Gif : IJob
	{
		private string[] args;

		public Webp2Gif(string[] args)
		{
			this.args = args;
		}

		public async Task<JobResult> DoJobAsync()
		{
			string pathToGif = Path.ChangeExtension(args[0], ".gif");
			var result = new JobResult(pathToGif, long.Parse(args[1]));

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
