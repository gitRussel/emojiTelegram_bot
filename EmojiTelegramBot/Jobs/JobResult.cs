using System;
using System.Collections.Generic;
using System.Text;

namespace EmojiTelegramBot.Jobs
{
	public class JobResult
	{
		/// <summary>
		/// Path to gif file
		/// </summary>
		public string PathToGif { get; set; }

		/// <summary>
		/// Chat id
		/// </summary>
		public long ChatId { get; set; }

		public JobResult(string pathToGif, long chatId)
		{
			PathToGif = pathToGif;
			ChatId = chatId;
		}
	}
}
