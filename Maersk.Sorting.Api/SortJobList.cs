using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Maersk.Sorting.Api
{
    public class SortJobList
    {
        private readonly ILogger<SortJobList> logger;
        private ConcurrentDictionary<Guid, SortJob> jobs;
        public SortJobList(ILogger<SortJobList> logger)
        {
            this.logger = logger;
            this.jobs = new ConcurrentDictionary<Guid, SortJob>();
        }

        public async Task AddAsync(SortJob job)
        {
            logger.LogInformation($"Adding job with Id:{job.Id}");
            if (!jobs.ContainsKey(job.Id))
                await Task.Run(() => jobs.TryAdd(job.Id, job));
            logger.LogInformation($"Added job with Id:{job.Id}");
        }

        public async Task<SortJob[]> GetAllJobsAsync()
        {
            return await Task.Run(() => jobs.Values.ToArray<SortJob>());
        }

        public async Task UpdateJobAsync(SortJob job)
        {
            logger.LogInformation($"Updating job with Id:{job.Id}");
            await Task.Run(() =>
                        jobs.AddOrUpdate(job.Id, job, (id, eJob) =>
                        new SortJob(eJob.Id, job.Status, job.Duration, eJob.Input, job.Output)));
            logger.LogInformation($"Updated job with Id:{job.Id}");
        }

        public async Task<SortJob> GetJobByIdAsync(Guid id)
        {
            return await Task.Run(() =>
                        jobs.Where(j => j.Key == id).FirstOrDefault().Value);
        }
    }
}