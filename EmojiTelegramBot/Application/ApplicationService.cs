using EmojiTelegramBot.Configuration;
using EmojiTelegramBot.Jobs;
using EmojiTelegramBot.Logger;
using MihaZupan;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;

namespace EmojiTelegramBot.Application
{
	///<inheritdoc cref="IApplicationService"/>
	public class ApplicationService : IApplicationService
	{
		private ITelegramBotClient botClient;
		private ChannelsQueuePubSub queue;
		private Chat chatId;
		private IConfiguration config;
		private ILogger logger;
		private ILoggerService loggerSvc;
		private readonly object chatLock = new object();

		public ApplicationService(
			ILoggerService loggerService,
			IConfiguration configuration)
		{
			loggerSvc = loggerService;
			logger = loggerSvc.Create("Application");
			config = configuration;
			try
			{
				var proxy = new HttpToSocks5Proxy(config.ProxyHostName, config.ProxyPort);
				botClient = new TelegramBotClient(config.ApiBotToken, proxy) { Timeout = TimeSpan.FromSeconds(20) };
				queue = new ChannelsQueuePubSub(config.ParallelCount, logger);
			}
			catch (Exception ex)
			{
				logger.Error($"Error with bot registration: {ex.Message}");
			}

		}

		private Chat GetChat()
		{
			lock (chatLock)
			{
				return chatId;
			}
		}

		public async Task Run(string[] args)
		{
			User me = await botClient.GetMeAsync();

			logger.Info($"Started bot with id {me.Id}, named {me.FirstName}.");

			botClient.OnMessage += BotOnMessageReciving;
			botClient.StartReceiving();

			queue.RegisterHandler<Tgs2Gif>(async j =>
			{
				string convertingResult = await j.DoJobAsync();
				if (convertingResult == "-1")
				{
					await SendWarnMessage("Convert is failed.");
				}
				else
				{
					await UploadGifFileAsync(convertingResult);
				}
			});
			queue.RegisterHandler<UnicodeEmoji2Gif>(async j =>
			{
				string filePath = await j.DoJobAsync();
				await UploadGifFileAsync(filePath);
			});
			queue.RegisterHandler<Webp2Gif>(async j =>
			{
				string filePath = await j.DoJobAsync();
				await UploadGifFileAsync(filePath);
			});

			Console.WriteLine("Press 'q' to quit the sample.");
			while (Console.Read() != 'q') ;
			botClient.StopReceiving();
			queue.Stop();
		}

		private async void BotOnMessageReciving(object sender, MessageEventArgs e)
		{
			lock (chatLock)
			{
				chatId = e.Message.Chat;
			}

			Sticker sticker = e?.Message?.Sticker;
			string text = e?.Message?.Text;

			if (sticker == null && text == null)
			{
				await SendWarnMessage("Send a sticker(animated or static) or unicode emoji.");
				return;
			}

			await TextOperationsAsync(text);

			await StickerOperationsAsync(sticker);
		}

		private async Task StickerOperationsAsync(Sticker sticker)
		{
			IJob job = null;
			string[] args;
			string pathToImportFile = "";

			if (sticker == null)
			{
				return;
			}

			string pathToGifFile = Path.Combine(config.PathToGifDirectory, $"{sticker.FileUniqueId}.gif");
			if (File.Exists(pathToGifFile))
			{
				logger.Info($"This file ({Path.GetFullPath(pathToGifFile)}) already exists in the cache folder.");
				await UploadGifFileAsync(pathToGifFile);
			}
			else
			{
				pathToImportFile = Path.Combine(config.PathToGifDirectory, $"{sticker.FileUniqueId}");

				if (sticker.IsAnimated)
				{
					pathToImportFile += ".tgs";
					args = new string[] { pathToImportFile };
					job = new Tgs2Gif(args, logger);
				}
				else
				{
					pathToImportFile += ".webp";
					args = new string[] { pathToImportFile };
					job = new Webp2Gif(args);
				}

				// Download file from Telegram
				using (var output =  File.Open(pathToImportFile, FileMode.OpenOrCreate))
				{
					_ = await botClient.GetInfoAndDownloadFileAsync(sticker.FileId, output);
				}

				await queue.Enqueue(job);
			}
		}

		private async Task TextOperationsAsync(string text)
		{
			if (text == null)
				return;

			IJob job = null;
			string[] args;

			// Search emoji in text.
			var regex = new Regex(@"\u00a9|\u00ae|[\u2000-\u3300]|\ud83c[\ud000-\udfff]|\ud83d[\ud000-\udfff]|\ud83e[\ud000-\udfff]");
			var matches = regex.Matches(text);

			if (matches.Count <= 0)
			{
				await SendWarnMessage("Send a sticker(animated or static) or unicode emoji.");
				return;
			}

			foreach (Match match in matches)
			{
				string pathToGifFile = Path.Combine(config.PathToGifDirectory, $"{match.Value.GetHashCode()}.gif");

				if (System.IO.File.Exists(pathToGifFile))
				{
					logger.Info($"This file ({Path.GetFullPath(pathToGifFile)}) already exists in the cache folder.");
					await UploadGifFileAsync(pathToGifFile);
				}
				else
				{
					args = new string[] { pathToGifFile, match.Value }; ;
					job = new UnicodeEmoji2Gif(args, logger);

					await queue.Enqueue(job);
				}
			}
		}

		private async Task UploadGifFileAsync(string filePath)
		{
			try
			{
				using (Stream stream = File.OpenRead(filePath))
				{
					_ = await botClient.SendAnimationAsync(
						chatId: GetChat(),
						animation: new InputOnlineFile(stream, Path.GetFileName(filePath)),
						disableNotification: true
						).ConfigureAwait(false);
				}
			}
			catch (ApiRequestException ex)
			{
				logger.Error($"There was an error sendin. {ex.Message}");
				botClient.StartReceiving();
			}
		}

		private async Task SendWarnMessage(string message)
		{
			try
			{
				_ = await botClient.SendTextMessageAsync(
						 chatId: GetChat(),
						 text: message
						 ).ConfigureAwait(false);
			}
			catch (ApiRequestException ex)
			{
				logger.Error($"Catch exception when send a message \n {ex.Message}");
			}
		}
	}
}
