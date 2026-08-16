using MultilingualFileProcessingPlatform.Api.Models;
using MultilingualFileProcessingPlatform.Api.Data;

namespace MultilingualFileProcessingPlatform.Api.Services
{
    /// <summary>
    /// Provides operations for managing jobs.
    /// </summary>
    public class JobService : IJobService
    {
        private readonly AppDbContext _context;

        public JobService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns all jobs.
        /// </summary>
        public List<Job> GetJobs()
        {
            return _context.Jobs.ToList();
        }

        public Job? GetJob(Guid id)
        {
            return _context.Jobs.FirstOrDefault(job => job.Id == id);
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

            _context.Jobs.Add(job);
            _context.SaveChanges();

            return job;
        }

        public bool DeleteJob(Guid id)
        {
            Job? job = _context.Jobs.FirstOrDefault(job => job.Id == id);

            if (job == null)
            {
                return false;
            }

            _context.Jobs.Remove(job);
            _context.SaveChanges();

            return true;
        }

        public Job? UpdateJob(Guid id, string name)
        {
            Job? job = _context.Jobs.FirstOrDefault(job => job.Id == id);

            if (job == null)
            {
                return null;
            }

            job.Name = name;

            _context.SaveChanges();

            return job;
        }
    }
}
