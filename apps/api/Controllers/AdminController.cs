using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("[controller]")]
public class AdminController : ControllerBase
{
    private readonly BulkSyncJobService _jobService;
    private readonly BulkFilmSyncService _bulkSync;

    public AdminController(BulkSyncJobService jobService, BulkFilmSyncService bulkSync)
    {
        _jobService = jobService;
        _bulkSync = bulkSync;
    }

    [Authorize(Roles = "SysAdmin")]
    [HttpPost("bulkSync/start")]
    public IActionResult StartBulkSync()
    {
        if (!_jobService.TryStart())
            return Conflict("A bulk sync job is already running.");

        _ = Task.Run(async () =>
        {
            try
            {
                await _bulkSync.RunAsync();
            }
            catch (Exception ex)
            {
                _jobService.Fail(ex.Message);
            }
        });

        return Accepted();
    }

    [Authorize(Roles = "SysAdmin")]
    [HttpGet("bulkSync/status")]
    public IActionResult GetBulkSyncStatus()
    {
        return Ok(_jobService.GetSnapshot());
    }
}
