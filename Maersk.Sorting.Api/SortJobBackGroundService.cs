using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Maersk.Sorting.Api
{
    public class SortJobBackGroundService : BackgroundService
    {
        private readonly ILogger<SortJobBackGroundService> logger;
        private readonly ISortJobProcessor jobProcessor;
        private readonly SortJobList jobList;
        private readonly SortJobQueue jobQueue;

        public SortJobBackGroundService(ILogger<SortJobBackGroundService> logger,
                                        ISortJobProcessor jobProcessor,
                                        SortJobList jobList,
                                        SortJobQueue jobQueue)
        {
            this.logger = logger;
            this.jobProcessor = jobProcessor;
            this.jobList = jobList;
            this.jobQueue = jobQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Started background service...");
            while (!stoppingToken.IsCancellationRequested)
            {
                var job = await jobQueue.DeQueueJobAsync(stoppingToken);
                if (job != null)
                {
                    var processedJob = await jobProcessor.Process(job);
                    await jobList.UpdateJobAsync(processedJob);
                }
            }
            logger.LogInformation("Completed background service");
        }
    }
}