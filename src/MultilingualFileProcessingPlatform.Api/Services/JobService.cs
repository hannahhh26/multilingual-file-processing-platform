using MultilingualFileProcessingPlatform.Api.Models;

namespace MultilingualFileProcessingPlatform.Api.Services
{
    /// <summary>
    /// Provides operations for managing jobs.
    /// </summary>
    public class JobService : IJobService
    {
       private readonly List<Job> _jobs = [];

        /// <summary>
        /// Returns all jobs.
        /// </summary>
        public List<Job> GetJobs()
        {
            return _jobs;
        }

        public Job? GetJob(Guid id)
        {
            return _jobs.FirstOrDefault(job => job.Id == id);
        }

        public Job CreateJob(string name)
        {
            Job job = new Job
            {
                Id = Guid.NewGuid(),
                Name = name,
                Status = "Created",
                CreatedAt = DateTime.UtcNow
            };

            _jobs.Add(job);

            return job;
        }

        public bool DeleteJob(Guid id)
        {
            Job? job = _jobs.FirstOrDefault(job => job.Id == id);

            if (job == null)
            {
                return false;
            }

            _jobs.Remove(job);

            return true;
        }

        public Job? UpdateJob(Guid id, string name)
        {
            Job? job = _jobs.FirstOrDefault(job => job.Id == id);

            if (job == null)
            {
                return null;
            }

            job.Name = name;

            return job;
        }
    }
}
