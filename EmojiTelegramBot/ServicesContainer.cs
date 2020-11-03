using Autofac;
using EmojiTelegramBot.Application;
using EmojiTelegramBot.Configuration;
using EmojiTelegramBot.Logger;

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

        /// <inheritdoc cref="IConfiguration"/>
        public static IConfiguration Configuration
        {
            get
            {
                return _container.Resolve<IConfiguration>();
            }
        }

        /// <summary>
        /// Container settings
        /// </summary>
        static ServicesContainer()
        {
            var builder = new ContainerBuilder();

            var cfg = new EmojiTelegramBot.Configuration.Configuration();
            var lgr = new LoggerService();

            builder.RegisterInstance(cfg).As<IConfiguration>();
            builder.RegisterInstance(lgr).As<ILoggerService>();

            builder.RegisterType<ApplicationService>().As<IApplicationService>().
                    WithParameter("logger", lgr).
                    WithParameter("configuration", cfg);

            _container = builder.Build();
        }

    }
}
