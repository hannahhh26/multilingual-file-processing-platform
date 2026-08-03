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
}