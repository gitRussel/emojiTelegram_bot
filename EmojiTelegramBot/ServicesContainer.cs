using Autofac;
using EmojiTelegramBot.Application;
using EmojiTelegramBot.Configuration;
using EmojiTelegramBot.Logger;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;

namespace EmojiTelegramBot
{
	/// <summary>
	/// IoC Container
	/// </summary>
	public static class ServicesContainer
	{
		/// <summary>
		/// Container
		/// </summary>
		private static Autofac.IContainer _container;

		/// <inheritdoc cref="IApplicationService"/>
		public static IApplicationService Application
		{
			get
			{
				return _container.Resolve<IApplicationService>();
			}
		}

		/// <inheritdoc cref="ILoggerService"/>
		public static ILoggerService Logger
		{
			get
			{
				return _container.Resolve<ILoggerService>();
			}
		}

		/// <inheritdoc cref="ICustomConfiguration"/>
		public static ICustomConfiguration Configuration
		{
			get
			{
				return _container.Resolve<ICustomConfiguration>();
			}
		}

		/// <summary>
		/// Container settings
		/// </summary>
		static ServicesContainer()
		{
			var config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddUserSecrets<CustomConfiguration>()
				.Build();

			var cfg = new CustomConfiguration()
			{
				ApiBotToken = config.GetSection("DevConfig:ApiBotToken").Value,
				ParallelCount = int.TryParse(config.GetSection("DevConfig:ParallelCount").Value, out int count) ? count : 1,
				PathToGifDirectory = BuildPathToGifDirectory(config.GetSection("DevConfig:PathToGifDirectory").Value),
				ProxyHostName = config.GetSection("DevConfig:ProxyHostName").Value,
				ProxyPort = int.TryParse(config.GetSection("DevConfig:ProxyPort").Value, out int port) ? port : 1080,
			};

			var builder = new ContainerBuilder();

			var lgr = new LoggerService();

			builder.RegisterInstance(cfg).As<ICustomConfiguration>();
			builder.RegisterInstance(lgr).As<ILoggerService>();

			builder.RegisterType<ApplicationService>().As<IApplicationService>().
					WithParameter("logger", lgr).
					WithParameter("configuration", cfg);

			_container = builder.Build();
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
