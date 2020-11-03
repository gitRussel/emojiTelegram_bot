using System.Threading.Tasks;

namespace EmojiTelegramBot.Application
{
    public interface IApplicationService
    {
        /// <summary>
        /// Run application
        /// </summary>
        Task Run(string[] args);
    }
}
