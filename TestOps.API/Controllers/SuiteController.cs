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

            _service.Update(id, suite);

            return Ok(suite);
        }

        // Adds tests to the end of the suite, skipping any it already has
        [HttpPost("{id}/tests/add")]
        public IActionResult AddTests(string id, [FromBody] List<SuiteTest> tests)
        {
            var suite = _service.GetById(id);

            if (suite == null)
                return NotFound();

            var existing = new HashSet<string>(
                suite.Tests.Select(t => t.ClassName), StringComparer.OrdinalIgnoreCase);

            int next = suite.Tests.Count;

            foreach (var test in tests ?? new List<SuiteTest>())
            {
                if (string.IsNullOrWhiteSpace(test.ClassName) || !existing.Add(test.ClassName))
                    continue;

                test.Order = ++next;
                suite.Tests.Add(test);
            }

            _service.Update(id, suite);

            return Ok(_service.GetById(id));
        }

        [HttpPut("{id}")]
        public IActionResult Update(string id, [FromBody] TestSuite suite)
        {
            _service.Update(id, suite);

            return Ok();
        }
    }
}