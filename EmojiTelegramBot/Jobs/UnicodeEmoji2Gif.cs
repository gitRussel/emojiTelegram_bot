using EmojiTelegramBot.Logger;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace EmojiTelegramBot.Jobs
{
    /// <inheritdoc cref="IJob"/>
    public class UnicodeEmoji2Gif : IJob
    {
        private string[] args;

        private ILogger Logger { get; set; }

        public UnicodeEmoji2Gif(string[] args, ILogger logger)
        {
            this.args = args;
            Logger = logger;
        }

        public async Task<string> DoJobAsync()
        {
            string pathToGif = Path.ChangeExtension(args[0], ".gif");
            await Task.Run(() =>
            {
                float x = 0;
                float y = 0;
                int height = 100;
                int width = 100;

                using var bm = new Bitmap(height, width);
                using var gr = Graphics.FromImage(bm);

                Font drawFont = null;
                if (Application.OperatingSystem.IsLinux)
                {
                    drawFont = new Font("Noto Color Emoji", 45);
                }
                else if (Application.OperatingSystem.IsWindows)
                {
                    drawFont = new Font("Segoe UI Emoji", 45);
                }

                using var drawBrush = new SolidBrush(Color.YellowGreen);
                var drawRect = new RectangleF(x, y, width, height);
                using var rectBrush = new SolidBrush(Color.White);

                gr.FillRectangle(rectBrush, x, y, width, height);

                using var drawFormat = new StringFormat(StringFormatFlags.NoFontFallback);

                drawFormat.Alignment = StringAlignment.Center;
                gr.DrawString(args[1], drawFont, drawBrush, drawRect, drawFormat);

                try
                {
                    bm.Save(args[0], System.Drawing.Imaging.ImageFormat.Gif);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error when saving file: {ex.Message}");
                }
            });
            return pathToGif;
        }

    }
}
