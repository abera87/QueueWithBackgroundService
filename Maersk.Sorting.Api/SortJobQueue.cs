using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Maersk.Sorting.Api
{
    public class SortJobQueue
    {
        private readonly ILogger<SortJobQueue> logger;
        private ConcurrentQueue<SortJob> jobsQueue;
        private SemaphoreSlim signal = new SemaphoreSlim(0);
        public SortJobQueue(ILogger<SortJobQueue> logger)
        {
            this.logger = logger;
            this.jobsQueue = new ConcurrentQueue<SortJob>();
        }

        public async Task EnQueueJobAsync(SortJob job)
        {
            logger.LogInformation($"Queuing job with Id:{job.Id}");
            await Task.Run(() =>
            {
                jobsQueue.Enqueue(job);
                signal.Release();
            });
            logger.LogInformation($"Queued job with Id:{job.Id}");
        }
        public async Task<SortJob?> DeQueueJobAsync(CancellationToken token)
        {
            bool result;
            logger.LogInformation($"Start dequeing process");
            await signal.WaitAsync(token);
            result = jobsQueue.TryDequeue(out SortJob? job);
            if (result)
                logger.LogInformation($"Dequeue completed for job with Id:{job?.Id}");
            else
                logger.LogInformation("Unable to dequeue job");

            return job;
        }
    }
}