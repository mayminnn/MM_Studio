using Microsoft.AspNetCore.Mvc;
using TestOps.API.Services;

namespace TestOps.API.Controllers
{
    [ApiController]
    [Route("api/results")]
    public class ResultsController : ControllerBase
    {
        private readonly ResultService _service;

        public ResultsController(ResultService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_service.GetAll());
        }

        [HttpGet("run/{runId}")]
        public IActionResult GetByRun(Guid runId)
        {
            var results = _service.GetAll()
                .Where(x => x.RunId == runId)
                .ToList();

            return Ok(results);
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var result = _service.Get(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }
}