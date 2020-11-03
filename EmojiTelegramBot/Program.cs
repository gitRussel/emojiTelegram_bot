using System;
using System.Threading.Tasks;

namespace EmojiTelegramBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                await ServicesContainer.Application.Run(args);
            }
            catch (Exception ex)
            {
                var logger = ServicesContainer.Logger.Create("Program");
                logger.Fatal($"Unexpected exception: {ex}");
            }
        }

    }
}
