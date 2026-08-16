using MultilingualFileProcessingPlatform.Api.Models;

namespace MultilingualFileProcessingPlatform.Api.Services
{
    public interface IJobService
    {
        List<Job> GetJobs();

        Job CreateJob(string name);

        Job? GetJob(Guid id);

        Job? UpdateJob(Guid id, string name);

        bool DeleteJob(Guid id);

        SaveSourceFileResult SaveSourceFile(Guid id, IFormFile file);

        PreprocessJobResult PreprocessJob(Guid id);
    }
}