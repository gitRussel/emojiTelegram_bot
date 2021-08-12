using System;
using System.Collections.Generic;
using System.Text;
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
        Task<JobResult> DoJobAsync();
    }
}
