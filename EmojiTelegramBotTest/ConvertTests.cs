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
        Webp2Gif webp2Gif;
        string[] args;
        string pathToDataDirectory;
        string pathToTgsFile;
        string pathToGif;
        string pathToWebpFile;
        Mock<ILogger> loggerMock;

        [SetUp]
        public void Setup()
        {
            Mock<ILoggerService> loggerServiceMock = new Mock<ILoggerService>();
            loggerMock = new Mock<ILogger>();
            loggerServiceMock.Setup(x => x.Create("Test")).Returns(loggerMock.Object);

            pathToDataDirectory = Path.GetFullPath(@"..\..\..\Data\");
            pathToTgsFile = Directory.GetFiles(pathToDataDirectory, "*.tgs")[0];
            
            // Create a test WebP file for testing
            pathToWebpFile = Path.Combine(pathToDataDirectory, "test.webp");
            if (!File.Exists(pathToWebpFile))
            {
                // Create a minimal WebP file (just a placeholder for testing)
                File.WriteAllBytes(pathToWebpFile, new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50 });
            }
        }

        [TearDown]
        public void Cleanup()
        {
            // Clean up any test files created during tests
            if (File.Exists(pathToGif))
            {
                File.Delete(pathToGif);
            }
        }

        [Test]
        public async Task TgsConvertTest()
        {
            pathToGif = Path.ChangeExtension(pathToTgsFile, ".gif");
            tgs2Gif = new Tgs2Gif(pathToTgsFile, 0L, loggerMock.Object);

            await tgs2Gif.DoJobAsync();

            string[] gifFiles = Directory.GetFiles(pathToDataDirectory, "*.gif");

            Assert.IsTrue(gifFiles.Any(x => x == pathToGif), $"Expected gif at {pathToGif}, found: {string.Join(", ", gifFiles)}");

            File.Delete(pathToGif);
        }

        [Test]
        public async Task UnicodeEmojiConvertTest()
        {
            pathToGif = $"{pathToDataDirectory}{"\u00a9".GetHashCode()}.gif";
            unicodeEmoji2Gif = new UnicodeEmoji2Gif(pathToGif, "\u00a9", 0L, loggerMock.Object);
            await unicodeEmoji2Gif.DoJobAsync();

            string[] gifFiles = Directory.GetFiles(pathToDataDirectory, "*.gif");

            Assert.IsTrue(gifFiles.Any(x => x == pathToGif));
            File.Delete(pathToGif);
        }

        [Test]
        public async Task Webp2Gif_DoJobAsync_ShouldReturnCorrectJobResult()
        {
            // Arrange
            long expectedChatId = 12345;
            args = new string[] { pathToWebpFile, expectedChatId.ToString() };
            webp2Gif = new Webp2Gif(args);

            // Act
            var result = await webp2Gif.DoJobAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(Path.ChangeExtension(pathToWebpFile, ".gif"), result.PathToGif);
            Assert.AreEqual(expectedChatId, result.ChatId);
        }

        [Test]
        public async Task Webp2Gif_DoJobAsync_ShouldMoveFileToGifExtension()
        {
            // Arrange
            args = new string[] { pathToWebpFile, "0" };
            webp2Gif = new Webp2Gif(args);
            string expectedGifPath = Path.ChangeExtension(pathToWebpFile, ".gif");

            // Ensure the WebP file exists
            Assert.IsTrue(File.Exists(pathToWebpFile));

            // Ensure destination does not exist
            if (File.Exists(expectedGifPath))
            {
                File.Delete(expectedGifPath);
            }

            // Act
            var result = await webp2Gif.DoJobAsync();

            // Assert
            Assert.IsTrue(File.Exists(expectedGifPath));
            Assert.AreEqual(expectedGifPath, result.PathToGif);
            
            // Clean up - move the file back to .webp extension for other tests
            if (File.Exists(expectedGifPath))
            {
                File.Move(expectedGifPath, pathToWebpFile, true);
            }
        }

        [Test]
        public async Task Webp2Gif_DoJobAsync_ShouldNotOverwriteExistingGif()
        {
            // Arrange
            args = new string[] { pathToWebpFile, "0" };
            webp2Gif = new Webp2Gif(args);
            string expectedGifPath = Path.ChangeExtension(pathToWebpFile, ".gif");
            
            // Create an existing GIF file
            File.WriteAllText(expectedGifPath, "existing gif content");
            string originalContent = File.ReadAllText(expectedGifPath);

            // Act
            var result = await webp2Gif.DoJobAsync();

            // Assert
            Assert.IsTrue(File.Exists(expectedGifPath));
            Assert.AreEqual(originalContent, File.ReadAllText(expectedGifPath));
            Assert.AreEqual(expectedGifPath, result.PathToGif);
            
            // Clean up
            File.Delete(expectedGifPath);
        }

        [Test]
        public void Webp2Gif_Constructor_ShouldSetArgs()
        {
            // Arrange & Act
            string[] testArgs = { "test.webp", "123" };
            var webp2Gif = new Webp2Gif(testArgs);

            // Assert
            // Note: Since args is private, we can only test through the DoJobAsync method
            // This test verifies the constructor doesn't throw an exception
            Assert.IsNotNull(webp2Gif);
        }
    }
}