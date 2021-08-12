using EmojiTelegramBot.Logger;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EmojiTelegramBot.Jobs
{
    /// <inheritdoc cref="IJob"/>
    public class Tgs2Gif : IJob
    {
        private string[] args;
        private ILogger Logger;

        public Tgs2Gif(string[] args, ILogger logger)
        {
            Logger = logger;
            this.args = args;
        }

        public async Task<JobResult> DoJobAsync()
        {
            string pathToGif = Path.ChangeExtension(args[0], ".gif");
            var result = new JobResult(pathToGif, long.Parse(args[1]));

            int commandResult = await ProcessCommandAsync(args);

            if (commandResult == 0)
            {
                Logger.Info($"File is ready {pathToGif}");
                return result;
            }
            else
            {
                return new JobResult("-1", 0); 
            }
        }

        private Task<int> ProcessCommandAsync(string[] args)
        {
            var tcs = new TaskCompletionSource<int>();
            string fileName = "";
            if (Application.OperatingSystem.IsLinux)
            {
                fileName = "python3";
            }
            else if (Application.OperatingSystem.IsWindows)
            {
                fileName = "python";
            }

            var start = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/tgsconvert.py \"{args[0]}\" \"{Path.ChangeExtension(args[0], ".gif")}\" ",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true

            };
            using var process = new Process
            {
                StartInfo = start,
                EnableRaisingEvents = true
            };

            process.OutputDataReceived += (s, ea) => Logger.Info(ea.Data);
            process.ErrorDataReceived += (s, ea) => Logger.Error(ea.Data);

            process.Exited += (s, a) =>
            {
                tcs.TrySetResult(process.ExitCode);
            };
            try
            {
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in tgs convertingg \n {ex.Message}");
            }

            return tcs.Task;
        }
    }
}
