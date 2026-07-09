using Microsoft.AspNetCore.Mvc;
using TestOps.API.Models;
using TestOps.API.Services;

namespace TestOps.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuiteController : ControllerBase
    {
        private readonly SuiteService _service;

        public SuiteController(SuiteService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_service.GetAll());
        }

        [HttpPost]
        public IActionResult Create([FromBody] TestSuite suite)
        {
            if (suite == null || string.IsNullOrEmpty(suite.Name))
                return BadRequest("Invalid suite");

            var created = _service.Create(suite);
            return Ok(created);
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var suite = _service.GetById(id);

            if (suite == null)
                return NotFound();

            return Ok(suite);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            _service.Delete(id);
            return Ok();
        }

        [HttpPost("{id}/tests/remove")]
        public IActionResult RemoveTests(string id, [FromBody] List<string> classNames)
        {
            var suite = _service.GetById(id);

            if (suite == null)
                return NotFound();

            suite.Tests = suite.Tests
                .Where(t => !classNames.Contains(t.ClassName))
                .ToList();

            _service.Update(suite);

            return Ok(suite);
        }
    }
}