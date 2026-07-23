using Microsoft.AspNetCore.Mvc;
using TestOps.API.Models;
using TestOps.API.Services;

namespace TestOps.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExecutionController : ControllerBase
    {
        // private readonly ExecutionService _service;

        // public ExecutionController(ExecutionService service)
        // {
        //     _service = service;
        // }
        private readonly ExecutionService _service;
        private readonly ExecutionTracker _tracker;

        public ExecutionController(
            ExecutionService service,
            ExecutionTracker tracker)
        {
            _service = service;
            _tracker = tracker;
        }

        // [HttpPost("run")]
        // public async Task<IActionResult> Run([FromBody] List<SuiteTest> tests)
        // {
        //     if (tests.Count == 0)
        //         return BadRequest("No tests selected.");

        //     var result = await _service.RunSuiteAsync(tests);

        //     return Ok(result);
        // }

        [HttpPost("run")]
        public async Task<IActionResult> Run([FromBody] RunSuiteRequest request)
        {
            // try
            // {
            if (request.Tests.Count == 0)
                return BadRequest("No tests selected.");

            // var result = await _service.RunSuiteAsync(request.SuiteName, request.Tests);

            // return Ok(result);
            // }
            // catch (Exception ex)
            // {
            //     return StatusCode(500, ex.ToString());
            // }

            var runId = Guid.NewGuid();

            _ = Task.Run(async () =>
            {
                await _service.RunSuiteAsync(
                    runId,
                    request.SuiteName,
                    request.Tests);
            });

            return Ok(new
            {
                RunId = runId
            });
        }

        [HttpGet("status/{runId}")]
        public IActionResult Status(Guid runId)
        {
            var status = _tracker.Get(runId);

            if (status == null)
                return NotFound();

            return Ok(status);
        }
    }
}