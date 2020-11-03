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

        public async Task<string> DoJobAsync()
        {
            string pathToGif = Path.ChangeExtension(args[0], ".gif");
            await Task.Run(() =>
            {
                File.Move(args[0], pathToGif);
            });
            return pathToGif;
        }
    }
}
