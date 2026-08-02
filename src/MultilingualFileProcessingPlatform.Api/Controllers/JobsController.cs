using Microsoft.AspNetCore.Mvc;

namespace MultilingualFileProcessingPlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class JobsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetJobs()
    {
        return Ok("Jobs endpoint is working");
    }
}