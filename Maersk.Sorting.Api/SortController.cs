using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Maersk.Sorting.Api.Controllers
{
    [ApiController]
    [Route("sort")]
    public class SortController : ControllerBase
    {
        private readonly ISortJobProcessor _sortJobProcessor;
        private readonly SortJobList jobList;
        private readonly SortJobQueue jobQueue;

        public SortController(ISortJobProcessor sortJobProcessor,
                              SortJobList jobList,
                              SortJobQueue jobQueue)
        {
            _sortJobProcessor = sortJobProcessor;
            this.jobList = jobList;
            this.jobQueue = jobQueue;
        }

        [HttpPost("run")]
        [Obsolete("This executes the sort job asynchronously. Use the asynchronous 'EnqueueJob' instead.")]
        public async Task<ActionResult<SortJob>> EnqueueAndRunJob(int[] values)
        {
            var pendingJob = new SortJob(
                id: Guid.NewGuid(),
                status: SortJobStatus.Pending,
                duration: null,
                input: values,
                output: null);

            var completedJob = await _sortJobProcessor.Process(pendingJob);

            return Ok(completedJob);
        }

        [HttpPost]
        public async Task<ActionResult<SortJob>> EnqueueJob(int[] values)
        {
            // TODO: Should enqueue a job to be processed in the background.
            var pendingJob = new SortJob(
                id: Guid.NewGuid(),
                status: SortJobStatus.Pending,
                duration: null,
                input: values,
                output: null);
            await jobQueue.EnQueueJobAsync(pendingJob);
            await jobList.AddAsync(pendingJob);

            return pendingJob;
        }

        [HttpGet]
        public async Task<ActionResult<SortJob[]>> GetJobs()
        {
            // TODO: Should return all jobs that have been enqueued (both pending and completed).
            return await jobList.GetAllJobsAsync();
        }

        [HttpGet("{jobId}")]
        public async Task<ActionResult<SortJob>> GetJob(Guid jobId)
        {
            // TODO: Should return a specific job by ID.
            return await jobList.GetJobByIdAsync(jobId);
        }
    }
}
