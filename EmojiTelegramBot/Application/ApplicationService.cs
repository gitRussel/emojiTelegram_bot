using EmojiTelegramBot.Configuration;
using EmojiTelegramBot.Jobs;
using EmojiTelegramBot.Logger;
using MihaZupan;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
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
		private ITelegramBotClient _botClient;
		private ChannelsQueuePubSub _queue;
		private ICustomConfiguration _config;
		private ILogger _logger;
		private ILoggerService _loggerSvc;

		public ApplicationService(
			ILoggerService loggerService,
			ICustomConfiguration configuration)
		{
			_loggerSvc = loggerService;
			_logger = _loggerSvc.Create("Application");
			_config = configuration;
			try
			{
				var proxy = new HttpToSocks5Proxy(_config.ProxyHostName, _config.ProxyPort);
				_botClient = new TelegramBotClient(_config.ApiBotToken, proxy) { Timeout = TimeSpan.FromSeconds(20) };
				_queue = new ChannelsQueuePubSub(_config.ParallelCount, _logger);
			}
			catch (Exception ex)
			{
				_logger.Error($"Error with bot registration: {ex.Message}");
			}

		}

		public async Task Run(string[] args)
		{
			User me = await _botClient.GetMeAsync();

			_logger.Info($"Started bot with id {me.Id}, named {me.FirstName}.");

			_botClient.OnMessage += async (s, e) => await BotOnMessageReciving(s, e);
			_botClient.StartReceiving();

			_queue.RegisterHandler<Tgs2Gif>(async j =>
			{
				var result = j.DoJobAsync().Result;
				if (result.PathToGif == "-1")
				{
					await SendWarnMessage("Convert is failed.", result.ChatId);
				}
				else
				{
					await UploadGifFileAsync(result.PathToGif, result.ChatId);
				}
			});
			_queue.RegisterHandler<UnicodeEmoji2Gif>(async j =>
			{
				var result = j.DoJobAsync().Result;
				await UploadGifFileAsync(result.PathToGif, result.ChatId);
			});
			_queue.RegisterHandler<Webp2Gif>(async j =>
			{
				var result = j.DoJobAsync().Result;
				await UploadGifFileAsync(result.PathToGif, result.ChatId);
			});

			Console.WriteLine("Press 'q' to quit the sample.");
			while (Console.Read() != 'q') ;
			_botClient.StopReceiving();
			_queue.Stop();
		}

		private async Task BotOnMessageReciving(object sender, MessageEventArgs e)
		{
			var chat = e.Message.Chat;
			_logger.Info($"Thread {Thread.CurrentThread.ManagedThreadId} has entered the protected area with chatid {chat.Id}.");

			Sticker sticker = e?.Message?.Sticker;
			string text = e?.Message?.Text;

			if (sticker == null && text == null)
			{
				await SendWarnMessage("Send a sticker(animated or static) or unicode emoji.", chat.Id);
				return;
			}

			await TextOperationsAsync(text, chat);

			await StickerOperationsAsync(sticker, chat);

		}

		private async Task StickerOperationsAsync(Sticker sticker, Chat chat)
		{
			IJob job = null;
			string[] args;
			string pathToImportFile = "";

			if (sticker == null)
			{
				return;
			}

			string pathToGifFile = Path.Combine(_config.PathToGifDirectory, $"{sticker.FileUniqueId}.gif");
			if (File.Exists(pathToGifFile))
			{
				_logger.Info($"This file ({Path.GetFullPath(pathToGifFile)}) already exists in the cache folder.");
				await UploadGifFileAsync(pathToGifFile, chat.Id);
			}
			else
			{
				pathToImportFile = Path.Combine(_config.PathToGifDirectory, $"{sticker.FileUniqueId}");

				if (sticker.IsAnimated)
				{
					pathToImportFile += ".tgs";
					args = new string[] { pathToImportFile, chat.Id.ToString() };
					job = new Tgs2Gif(args, _logger);
				}
				else
				{
					pathToImportFile += ".webp";
					args = new string[] { pathToImportFile, chat.Id.ToString() };
					job = new Webp2Gif(args);
				}

				const int numberOfRetries = 3;
				const int delayOnRetry = 10 * 1000;
				for (int i = 0; i < numberOfRetries; i++)
				{
					try
					{
						// Download file from Telegram
						using (var output = File.Open(pathToImportFile, FileMode.OpenOrCreate))
						{
							_ = await _botClient.GetInfoAndDownloadFileAsync(sticker.FileId, output);
						}
					}
					catch (IOException e) when (i < numberOfRetries)
					{
						_logger.Error(e.Message);
						Thread.Sleep(delayOnRetry);
					}
				}

				await _queue.Enqueue(job);
			}
		}

		private async Task TextOperationsAsync(string text, Chat chat)
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
				await SendWarnMessage("Send a sticker(animated or static) or unicode emoji.", chat.Id);
				return;
			}

			foreach (Match match in matches)
			{
				string pathToGifFile = Path.Combine(_config.PathToGifDirectory, $"{match.Value.GetHashCode()}.gif");

				if (System.IO.File.Exists(pathToGifFile))
				{
					_logger.Info($"This file ({Path.GetFullPath(pathToGifFile)}) already exists in the cache folder.");
					await UploadGifFileAsync(pathToGifFile, chat.Id);
				}
				else
				{
					args = new string[] { pathToGifFile, match.Value, chat.Id.ToString() }; ;
					job = new UnicodeEmoji2Gif(args, _logger);

					await _queue.Enqueue(job);
				}
			}
		}

		private async Task UploadGifFileAsync(string filePath, long chatId)
		{
			try
			{
				using (Stream stream = File.OpenRead(filePath))
				{
					_logger.Info($"Thread {Thread.CurrentThread.ManagedThreadId} sending animation to chat {chatId}");

					_ = await _botClient.SendAnimationAsync(
						chatId: chatId,
						animation: new InputOnlineFile(stream, Path.GetFileName(filePath)),
						disableNotification: true
						).ConfigureAwait(false);
				}
			}
			catch (ApiRequestException ex)
			{
				_logger.Error($"There was an error sendin. {ex.Message}");
				_botClient.StartReceiving();
			}
		}

		private async Task SendWarnMessage(string message, long chatId)
		{
			try
			{
				_ = await _botClient.SendTextMessageAsync(
						 chatId: chatId,
						 text: message
						 ).ConfigureAwait(false);
			}
			catch (ApiRequestException ex)
			{
				_logger.Error($"Catch exception when send a message \n {ex.Message}");
			}
		}
	}
}
