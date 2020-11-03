using System.Configuration;
using System.IO;
using System.Reflection;

namespace EmojiTelegramBot.Configuration
{
    /// <inheritdoc cref="IConfiguration"/>
    public class Configuration : IConfiguration
    {
        public int ParallelCount => int.TryParse(ConfigurationManager.AppSettings["ParallelCount"], out int count) ? count : 1;

        public string PathToGifDirectory
        {
            get
            {
                string dirPath = ConfigurationManager.AppSettings["PathToGifDirectory"].Replace('/', Path.DirectorySeparatorChar);
                if (Application.OperatingSystem.IsWindows)
                {
                    dirPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), dirPath);
                }
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                return dirPath;
            }
        }

        public string ProxyHostName => ConfigurationManager.AppSettings["ProxyHostName"];

        public int ProxyPort => int.TryParse(ConfigurationManager.AppSettings["ProxyPort"], out int port) ? port : 1080;

        public string ApiBotToken => ConfigurationManager.AppSettings["ApiBotToken"];
    }
}
