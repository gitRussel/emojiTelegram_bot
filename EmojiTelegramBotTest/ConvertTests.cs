using EmojiTelegramBot.Jobs;
using EmojiTelegramBot.Logger;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EmojiTelegramBotTest
{
    public class ConvertTests
    {
        Tgs2Gif tgs2Gif;
        UnicodeEmoji2Gif unicodeEmoji2Gif;
        string[] args;
        string pathToDataDirectory;
        string pathToTgsFile;
        string pathToGif;
        Mock<ILogger> loggerMock;

        [SetUp]
        public void Setup()
        {
            Mock<ILoggerService> loggerServiceMock = new Mock<ILoggerService>();
            loggerMock = new Mock<ILogger>();
            loggerServiceMock.Setup(x => x.Create("Test")).Returns(loggerMock.Object);

            pathToDataDirectory = Path.GetFullPath(@"..\..\..\Data\");
            pathToTgsFile = Directory.GetFiles(pathToDataDirectory, "*.tgs")[0];

        }

        [Test]
        public async Task TgsConvertTest()
        {
            pathToGif = Path.ChangeExtension(pathToTgsFile, ".gif");
            args = new string[] { pathToTgsFile, pathToGif };
            tgs2Gif = new Tgs2Gif(args, loggerMock.Object);

            await tgs2Gif.DoJobAsync();

            string[] gifFiles = Directory.GetFiles(pathToDataDirectory, "*.gif");

            Assert.IsTrue(gifFiles.Any(x => x == pathToGif));

            File.Delete(pathToGif);
        }



        [Test]
        public async Task UnicodeEmojiConvertTest()
        {
            pathToGif = $"{pathToDataDirectory}{"\u00a9".GetHashCode()}.gif";
            args = new string[] { pathToGif, "\u00a9" };
            unicodeEmoji2Gif = new UnicodeEmoji2Gif(args, loggerMock.Object);
            await unicodeEmoji2Gif.DoJobAsync();

            string[] gifFiles = Directory.GetFiles(pathToDataDirectory, "*.gif");

            Assert.IsTrue(gifFiles.Any(x => x == pathToGif));
            File.Delete(pathToGif);
        }
    }
}