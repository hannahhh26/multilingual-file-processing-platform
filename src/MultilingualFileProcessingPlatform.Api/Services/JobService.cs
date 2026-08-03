using MultilingualFileProcessingPlatform.Api.Models;

namespace MultilingualFileProcessingPlatform.Api.Services
{
    /// <summary>
    /// Provides operations for managing jobs.
    /// </summary>
    public class JobService
    {
       private readonly List<Job> _jobs = [];

        /// <summary>
        /// Returns all jobs.
        /// </summary>
        public List<Job> GetJobs()
        {
            return _jobs;
        }
    }
}
