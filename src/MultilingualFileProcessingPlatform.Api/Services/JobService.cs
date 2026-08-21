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
        private readonly JsonProcessingService _jsonProcessingService;

        public JobService(
            AppDbContext context,
            JsonProcessingService jsonProcessingService)
        {
            _context = context;
            _jsonProcessingService = jsonProcessingService;
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

            string uploadsDirectory = Path.Combine("Uploads", id.ToString(), "Original");

            Directory.CreateDirectory(uploadsDirectory);

            string filePath = Path.Combine(uploadsDirectory, file.FileName);

            using FileStream stream = new FileStream(filePath, FileMode.Create);

            file.CopyTo(stream);

            return SaveSourceFileResult.Success;
        }

        public PreprocessJobResult PreprocessJob(Guid id)
        {
            Job? job = _context.Jobs.FirstOrDefault(job => job.Id == id);

            if (job == null)
            {
                return PreprocessJobResult.JobNotFound;
            }

            string originalDirectory = Path.Combine(
                "Uploads",
                id.ToString(),
                "Original");

            if (!Directory.Exists(originalDirectory))
            {
                return PreprocessJobResult.SourceFileNotFound;
            }

            string? sourceFilePath = Directory
                .GetFiles(originalDirectory, "*.json")
                .FirstOrDefault();

            if (sourceFilePath == null)
            {
                return PreprocessJobResult.SourceFileNotFound;
            }

            string sourceJson = File.ReadAllText(sourceFilePath);

            Dictionary<string, string> strings =
                _jsonProcessingService.ExtractStrings(sourceJson);

            string preparedSourceDirectory = Path.Combine(
                "Uploads",
                id.ToString(),
                "PreparedSource");

            Directory.CreateDirectory(preparedSourceDirectory);

            string fileName = Path.GetFileName(sourceFilePath);

            string preparedSourcePath = Path.Combine(
                preparedSourceDirectory,
                fileName);

            string preparedJson = JsonSerializer.Serialize(
                strings,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(preparedSourcePath, preparedJson);

            return PreprocessJobResult.Success;
        }

        public GetPreparedSourceResult GetPreparedSource(Guid id)
        {
            var job = _context.Jobs.Find(id);

            if (job == null)
            {
                return new GetPreparedSourceResult
                {
                    Result = GetPreparedSourceResultType.JobNotFound
                };
            }

            var preparedSourceDirectory = Path.Combine(
                "Uploads",
                id.ToString(),
                "PreparedSource"
            );

            if (!Directory.Exists(preparedSourceDirectory))
            {
                return new GetPreparedSourceResult
                {
                    Result = GetPreparedSourceResultType.PreparedSourceNotFound
                };
            }

            var preparedSourceFile = Directory
                .GetFiles(preparedSourceDirectory, "*.json")
                .FirstOrDefault();

            if (preparedSourceFile == null)
            {
                return new GetPreparedSourceResult
                {
                    Result = GetPreparedSourceResultType.PreparedSourceNotFound
                };
            }

            return new GetPreparedSourceResult
            {
                Result = GetPreparedSourceResultType.Success,
                FilePath = preparedSourceFile
            };
        }

        public SaveTranslationFileResult SaveTranslationFile(Guid id, IFormFile file)
        {
            var job = _context.Jobs.Find(id);

            if (job == null)
            {
                return new SaveTranslationFileResult
                {
                    Result = SaveTranslationFileResultType.JobNotFound
                };
            }

            if (Path.GetExtension(file.FileName).ToLower() != ".json")
            {
                return new SaveTranslationFileResult
                {
                    Result = SaveTranslationFileResultType.InvalidFileType
                };
            }

            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                var json = reader.ReadToEnd();

                JsonDocument.Parse(json);

                var translationDirectory = Path.Combine(
                    "Uploads",
                    id.ToString(),
                    "Translation"
                );

                Directory.CreateDirectory(translationDirectory);

                var filePath = Path.Combine(
                    translationDirectory,
                    Path.GetFileName(file.FileName)
                );

                System.IO.File.WriteAllText(filePath, json);

                return new SaveTranslationFileResult
                {
                    Result = SaveTranslationFileResultType.Success
                };
            }
            catch (JsonException)
            {
                return new SaveTranslationFileResult
                {
                    Result = SaveTranslationFileResultType.InvalidJson
                };
            }
        }
    }
}
