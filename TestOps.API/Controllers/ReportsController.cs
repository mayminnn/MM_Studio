using Microsoft.AspNetCore.Mvc;
using TestOps.API.Services;

namespace TestOps.API.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly ReportService _service;

        public ReportsController(ReportService service)
        {
            _service = service;
        }

        // Installer versions that have results, newest first
        [HttpGet("releases")]
        public IActionResult Releases()
        {
            return Ok(_service.GetReleases());
        }

        // GET api/reports/release?installer=1.0&compareTo=0.9
        // compareTo is optional: omitted = the previously tested release, "none" = no comparison
        [HttpGet("release")]
        public IActionResult Release([FromQuery] string installer, [FromQuery] string? compareTo)
        {
            if (string.IsNullOrWhiteSpace(installer))
                return BadRequest("installer is required");

            if (string.Equals(compareTo, "none", StringComparison.OrdinalIgnoreCase))
                compareTo = "";

            var report = _service.GetReport(installer, compareTo);

            if (report == null)
                return NotFound($"No results for installer version '{installer}'.");

            return Ok(report);
        }
    }
}
