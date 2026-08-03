using Microsoft.AspNetCore.Mvc;
using MultilingualFileProcessingPlatform.Api.Models;
using MultilingualFileProcessingPlatform.Api.Services;

namespace MultilingualFileProcessingPlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class JobsController : ControllerBase
{
    private readonly JobService _jobService;

    public JobsController(JobService jobService)
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
}