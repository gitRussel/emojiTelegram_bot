using System.Threading.Tasks;

namespace EmojiTelegramBot.Jobs
{
	/// <summary>
	/// Converting job
	/// </summary>
	public interface IJob
	{
		/// <summary>
		/// Do converting async
		/// </summary>
		/// <returns>Result of job working</returns>
		Task<JobResult> DoJobAsync();
	}
}
