using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using EmojiTelegramBot.Jobs;
using EmojiTelegramBot.Logger;

namespace EmojiTelegramBot.Application
{
    public class ChannelsQueuePubSub
    {
        private ILogger _logger;
        private ChannelWriter<IJob> _writer;
        private Dictionary<Type, Action<IJob>> _handlers = new Dictionary<Type, Action<IJob>>();
        private List<Task> tasks;

        public ChannelsQueuePubSub(int threads, ILogger logger)
        {
            _logger = logger;
            tasks = new List<Task>();

            var channel = Channel.CreateUnbounded<IJob>();

            var reader = channel.Reader;
            _writer = channel.Writer;

            for (int i = 0; i < threads; i++)
            {
                var task = Task.Factory.StartNew(async () =>
                {
                    while (await reader.WaitToReadAsync())
                    {
                        var job = await reader.ReadAsync();
                        bool handlerExists = _handlers.TryGetValue(job.GetType(), out Action<IJob> value);

                        _logger.Info($"Processing {job.GetType().Name} in thread with id: {Thread.CurrentThread.ManagedThreadId}.");
                        if (handlerExists)
                        {
                            value.Invoke(job);
                        }
                    }
                }, TaskCreationOptions.LongRunning);
                tasks.Add(task);
            }
        }

        public async Task Enqueue(IJob job)
        {
            await _writer.WriteAsync(job);
        }

        public void RegisterHandler<T>(Action<T> handleAction) where T : IJob
        {
            Action<IJob> actionWrapper = (job) => handleAction((T)job);
            _handlers.Add(typeof(T), actionWrapper);
        }

        public void Stop()
        {
            _writer.Complete();
        }

    }
}

