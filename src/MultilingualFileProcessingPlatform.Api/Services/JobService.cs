using System.Text.Json;
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

        public SaveSourceFileResult SaveSourceFile(Guid id, IFormFile file)
        {
            Job? job = _context.Jobs.FirstOrDefault(job => job.Id == id);

            if (job == null)
            {
                return SaveSourceFileResult.JobNotFound;
            }

            string extension = Path.GetExtension(file.FileName);

            if (!extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                return SaveSourceFileResult.InvalidFileType;
            }

            try
            {
                using Stream jsonStream = file.OpenReadStream();
                using JsonDocument document = JsonDocument.Parse(jsonStream);
            }
            catch (JsonException)
            {
                return SaveSourceFileResult.InvalidJson;
            }

            string uploadsDirectory = Path.Combine("Uploads", id.ToString());

            Directory.CreateDirectory(uploadsDirectory);

            string filePath = Path.Combine(uploadsDirectory, file.FileName);

            using FileStream stream = new FileStream(filePath, FileMode.Create);

            file.CopyTo(stream);

            return SaveSourceFileResult.Success;
        }
    }
}
