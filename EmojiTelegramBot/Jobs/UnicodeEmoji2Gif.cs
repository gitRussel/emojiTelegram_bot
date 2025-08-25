using EmojiTelegramBot.Logger;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace EmojiTelegramBot.Jobs
{
    /// <inheritdoc cref="IJob"/>
    public class UnicodeEmoji2Gif : IJob
    {
        private readonly string _outputPath;
        private readonly string _emoji;
        private readonly long _chatId;
        private readonly ILogger _logger;

        public UnicodeEmoji2Gif(string outputPath, string emoji, long chatId, ILogger logger)
        {
            _outputPath = outputPath;
            _emoji = emoji;
            _chatId = chatId;
            _logger = logger;
        }

        public async Task<JobResult> DoJobAsync()
        {
            string pathToGif = Path.ChangeExtension(_outputPath, ".gif");
            var result = new JobResult(pathToGif, _chatId);

            _logger.Info($"Starting Unicode emoji to GIF conversion: {_emoji} -> {pathToGif}");

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
                gr.DrawString(_emoji, drawFont, drawBrush, drawRect, drawFormat);

                try
                {
                    bm.Save(_outputPath, ImageFormat.Gif);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error when saving file: {ex.Message}");
                }
            });
            return result;
        }
    }
}
