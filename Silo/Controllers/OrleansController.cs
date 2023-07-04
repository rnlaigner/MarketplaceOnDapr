using Common.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Orleans.Controllers;

[ApiController]
public class OrleansController : ControllerBase
{
    private readonly ILogger<OrleansController> logger;

    public OrleansController(ILogger<OrleansController> logger)
    {
        this.logger = logger;
    }

    [HttpGet]
    [Route("/")]
    public async Task<ActionResult<string>> Get([FromServices] IGrainFactory grains)
    {
        return Ok( await grains.GetGrain<IPersistentGrain>(0).GetUrl() );
    }
}

