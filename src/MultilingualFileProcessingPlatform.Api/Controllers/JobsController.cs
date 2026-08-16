using Microsoft.AspNetCore.Mvc;
using MultilingualFileProcessingPlatform.Api.Models;
using MultilingualFileProcessingPlatform.Api.Services;

namespace MultilingualFileProcessingPlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    [HttpGet]
    public IActionResult GetJobs()
    {
        List<Job> jobs = _jobService.GetJobs();

        return Ok(jobs);
    }

    [HttpGet("{id}")]
    public IActionResult GetJob(Guid id)
    {
        Job? job = _jobService.GetJob(id);

        if (job == null)
        {
            return NotFound();
        }

        return Ok(job);
    }

    [HttpPost]
    public IActionResult CreateJob(CreateJobRequest request)
    {
        Job job = _jobService.CreateJob(request.Name);

        return CreatedAtAction(
            nameof(GetJob),
            new { id = job.Id },
            job);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteJob(Guid id)
    {
        bool deleted = _jobService.DeleteJob(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPut("{id}")]
    public IActionResult UpdateJob(Guid id, UpdateJobRequest request)
    {
        Job? job = _jobService.UpdateJob(id, request.Name);

        if (job == null)
        {
            return NotFound();
        }

        return Ok(job);
    }

    [HttpPost("{id}/source")]
    public IActionResult UploadSourceFile(Guid id, IFormFile file)
    {
        if (file.Length == 0)
        {
            return BadRequest("File is empty.");
        }

        SaveSourceFileResult result = _jobService.SaveSourceFile(id, file);

        if (result == SaveSourceFileResult.JobNotFound)
        {
            return NotFound();
        }

        if (result == SaveSourceFileResult.InvalidFileType)
        {
            return BadRequest("Only JSON files are supported.");
        }

        if (result == SaveSourceFileResult.InvalidJson)
        {
            return BadRequest("File does not contain valid JSON.");
        }

        return Ok();
    }

    [HttpPost("{id}/preprocess")]
    public IActionResult PreprocessJob(Guid id)
    {
        PreprocessJobResult result = _jobService.PreprocessJob(id);

        if (result == PreprocessJobResult.JobNotFound)
        {
            return NotFound();
        }

        if (result == PreprocessJobResult.SourceFileNotFound)
        {
            return BadRequest("Source file has not been uploaded.");
        }

        return Ok();
    }
}