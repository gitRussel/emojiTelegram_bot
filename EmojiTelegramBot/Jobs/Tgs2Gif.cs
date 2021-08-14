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
        private ILogger logger;

        public Tgs2Gif(string[] args, ILogger logger)
        {
            this.logger = logger;
            this.args = args;
        }

        public async Task<JobResult> DoJobAsync()
        {
            string pathToGif = Path.ChangeExtension(args[0], ".gif");
            var result = new JobResult(pathToGif, long.Parse(args[1]));

            int commandResult = await ProcessCommandAsync(args);

            if (commandResult == 0)
            {
                logger.Info($"File is ready {pathToGif}");
                return result;
            }
            else
            {
                return new JobResult("-1", 0); 
            }
        }

        private Task<int> ProcessCommandAsync(string[] args)
        {
            logger.Info($"Start tgs converting with path to script file {Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}" +
                $" and gif directory {args[0]}");

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

            process.OutputDataReceived += (s, ea) => logger.Info(ea.Data);
            process.ErrorDataReceived += (s, ea) => logger.Error(ea.Data);

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
                logger.Error($"Error in tgs converting \n {ex.Message}");
            }

            return tcs.Task;
        }
    }
}
