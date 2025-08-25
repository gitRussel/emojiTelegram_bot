using EmojiTelegramBot.Application;
using EmojiTelegramBot.Configuration;
using EmojiTelegramBot.Logger;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;
using Serilog;

namespace EmojiTelegramBot
{
	/// <summary>
	/// IoC Container
	/// </summary>
	public static class ServicesContainer
	{
		/// <summary>
		/// Service provider
		/// </summary>
		private static IServiceProvider _provider;

		/// <inheritdoc cref="IApplicationService"/>
		public static IApplicationService Application
		{
			get
			{
				return _provider.GetRequiredService<IApplicationService>();
			}
		}

		/// <inheritdoc cref="ILoggerService"/>
		public static ILoggerService Logger
		{
			get
			{
				return _provider.GetRequiredService<ILoggerService>();
			}
		}

		/// <inheritdoc cref="ICustomConfiguration"/>
		public static ICustomConfiguration Configuration
		{
			get
			{
				return _provider.GetRequiredService<ICustomConfiguration>();
			}
		}

		/// <summary>
		/// Container settings
		/// </summary>
		static ServicesContainer()
		{
			// Initialize Serilog early
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.Enrich.FromLogContext()
				.WriteTo.Console()
				.CreateLogger();

			var config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddUserSecrets<CustomConfiguration>()
				.Build();

			var cfg = new CustomConfiguration()
			{
				ApiBotToken = config.GetSection("DevConfig:ApiBotToken").Value,
				ParallelCount = int.TryParse(config.GetSection("DevConfig:ParallelCount").Value, out int count) ? count : 1,
				PathToGifDirectory = BuildPathToGifDirectory(config.GetSection("DevConfig:PathToGifDirectory").Value),
			};

			var services = new ServiceCollection();

			services.AddSingleton<ICustomConfiguration>(cfg);
			services.AddSingleton<ILoggerService, LoggerService>();
			services.AddSingleton<IApplicationService, ApplicationService>();

			_provider = services.BuildServiceProvider();
		}

		private static string BuildPathToGifDirectory(string source)
		{
			if (string.IsNullOrEmpty(source))
			{
				return string.Empty;
			}

			string dirPath = source.Replace('/', Path.DirectorySeparatorChar);
			if (EmojiTelegramBot.Application.OperatingSystem.IsWindows)
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
}
