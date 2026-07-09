using Microsoft.AspNetCore.Mvc;
using TestOps.API.Models;
using TestOps.API.Services;

namespace TestOps.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExecutionController : ControllerBase
    {
        private readonly ExecutionService _service;

        public ExecutionController(ExecutionService service)
        {
            _service = service;
        }
        
        [HttpPost("run")]
        public async Task<IActionResult> Run([FromBody] RunSuiteRequest request)
        {
            // try
            // {
                if (request.Tests.Count == 0)
                    return BadRequest("No tests selected.");

                var result = await _service.RunSuiteAsync(request.Tests);

                return Ok(result);
            // }
            // catch (Exception ex)
            // {
            //     return StatusCode(500, ex.ToString());
            // }
        }
    }
}

// using Microsoft.AspNetCore.Mvc;
// using TestOps.API.Services;
// using TestOps.API.Models;

// namespace TestOps.API.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class ExecutionController : ControllerBase
//     {
//         private readonly SuiteService _suiteService;
//         private readonly ExecutionService _executionService;

//         public ExecutionController(
//             SuiteService suiteService,
//             ExecutionService executionService)
//         {
//             _suiteService = suiteService;
//             _executionService = executionService;
//         }

//         [HttpPost("run")]
//         public async Task<IActionResult> RunSuite([FromBody] RunSuiteRequest request)
//         {
//             if (request == null)
//                 return BadRequest("Request is null");

//             List<string> testsToRun = new();

//             // CASE 1: run suite
//             if (!string.IsNullOrEmpty(request.SuiteName))
//             {
//                 var suite = _suiteService
//                     .GetAll()
//                     .FirstOrDefault(x => x.Name == request.SuiteName);

//                 if (suite == null)
//                     return NotFound("Suite not found");

//                 testsToRun = suite.Tests
//                     .Select(t => t.ExecutionName)
//                     .ToList();
//             }

//             // CASE 2: run selected tests
//             else if (request.Tests != null && request.Tests.Count > 0)
//             {
//                 testsToRun = request.Tests;
//             }

//             if (!testsToRun.Any())
//                 return BadRequest("No tests found");

//             var result = await _executionService.RunSelectedTestsAsync(testsToRun);

//             return Ok(new
//             {
//                 message = "Execution completed",
//                 output = result
//             });
//         }
//     }
// }