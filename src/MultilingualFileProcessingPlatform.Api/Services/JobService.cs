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

            JsonPreprocessingResult preprocessingResult =
                _jsonProcessingService.PreprocessJson(sourceJson);

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
                new
                {
                    Segments = preprocessingResult.Segments
                },
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            File.WriteAllText(preparedSourcePath, preparedJson);

            string reconstructionDataDirectory = Path.Combine(
                "Uploads",
                id.ToString(),
                "ReconstructionData");

            Directory.CreateDirectory(reconstructionDataDirectory);

            string reconstructionDataPath = Path.Combine(
                reconstructionDataDirectory,
                fileName);

            string reconstructionJson = preprocessingResult.ReconstructionData!
                .ToJsonString(new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(reconstructionDataPath, reconstructionJson);

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

        public PostprocessJobResult PostprocessJob(Guid id)
        {
            Job? job = _context.Jobs.FirstOrDefault(job => job.Id == id);

            if (job == null)
            {
                return new PostprocessJobResult
                {
                    Result = PostprocessJobResultType.JobNotFound
                };
            }

            string reconstructionDataDirectory = Path.Combine(
                "Uploads",
                id.ToString(),
                "ReconstructionData");

            if (!Directory.Exists(reconstructionDataDirectory))
            {
                return new PostprocessJobResult
                {
                    Result = PostprocessJobResultType.ReconstructionDataNotFound
                };
            }

            string? reconstructionDataPath = Directory
                .GetFiles(reconstructionDataDirectory, "*.json")
                .FirstOrDefault();

            if (reconstructionDataPath == null)
            {
                return new PostprocessJobResult
                {
                    Result = PostprocessJobResultType.ReconstructionDataNotFound
                };
            }

            string translationDirectory = Path.Combine(
                "Uploads",
                id.ToString(),
                "Translation");

            if (!Directory.Exists(translationDirectory))
            {
                return new PostprocessJobResult
                {
                    Result = PostprocessJobResultType.TranslationFileNotFound
                };
            }

            string? translationFilePath = Directory
                .GetFiles(translationDirectory, "*.json")
                .FirstOrDefault();

            if (translationFilePath == null)
            {
                return new PostprocessJobResult
                {
                    Result = PostprocessJobResultType.TranslationFileNotFound
                };
            }

            string reconstructionJson = File.ReadAllText(reconstructionDataPath);
            string translationJson = File.ReadAllText(translationFilePath);

            TranslationValidationResult validationResult =
                _jsonProcessingService.ValidateTranslation(
                    reconstructionJson,
                    translationJson);

            if (!validationResult.IsValid)
            {
                return new PostprocessJobResult
                {
                    Result = PostprocessJobResultType.TranslationValidationFailed,
                    Validation = validationResult
                };
            }

            string rebuiltJson = _jsonProcessingService.RebuildJson(
                reconstructionJson,
                translationJson);

            string deliveryDirectory = Path.Combine(
                "Uploads",
                id.ToString(),
                "Delivery");

            Directory.CreateDirectory(deliveryDirectory);

            string fileName = Path.GetFileName(translationFilePath);

            string deliveryPath = Path.Combine(
                deliveryDirectory,
                fileName);

            File.WriteAllText(deliveryPath, rebuiltJson);

            return new PostprocessJobResult
            {
                Result = PostprocessJobResultType.Success
            };
        }

        public GetDeliveryResult GetDelivery(Guid id)
        {
            Job? job = _context.Jobs.Find(id);

            if (job == null)
            {
                return new GetDeliveryResult
                {
                    Result = GetDeliveryResultType.JobNotFound
                };
            }

            string deliveryDirectory = Path.Combine("Uploads", id.ToString(), "Delivery");

            if (!Directory.Exists(deliveryDirectory))
            {
                return new GetDeliveryResult
                {
                    Result = GetDeliveryResultType.DeliveryNotFound
                };
            }

            string? deliveryFile = Directory
                .GetFiles(deliveryDirectory, "*.json")
                .FirstOrDefault();

            if (deliveryFile == null)
            {
                return new GetDeliveryResult
                {
                    Result = GetDeliveryResultType.DeliveryNotFound
                };
            }

            return new GetDeliveryResult
            {
                Result = GetDeliveryResultType.Success,
                FilePath = deliveryFile
            };
        }
    }
}
