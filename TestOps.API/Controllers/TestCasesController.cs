using Microsoft.AspNetCore.Mvc;
using TestOps.API.Services;

namespace TestOps.API.Controllers;

[ApiController]
[Route("api/testcases")]
public class TestCasesController : ControllerBase
{
    private readonly IConfiguration _config;

    public TestCasesController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var path = _config["TestCasePath"];

        if (string.IsNullOrWhiteSpace(path))
            return BadRequest("TestCasePath not configured");

        var results = TestCaseScanner.Scan(path);

        return Ok(results);
    }
}