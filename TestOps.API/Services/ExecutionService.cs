using System.Diagnostics;
using TestOps.API.Models;

namespace TestOps.API.Services
{
    public class ExecutionService
    {
        private readonly ResultService _resultService;

        public ExecutionService(ResultService resultService)
        {
            _resultService = resultService;
        }

        public async Task<List<object>> RunSuiteAsync(List<SuiteTest> tests)
        {
            var results = new List<object>();
            var runId = Guid.NewGuid();

            string runnerPath =
                @"..\TestRunner\bin\Debug\net48\TestRunner.exe";

            if (!File.Exists(runnerPath))
                throw new Exception($"TestRunner not found:\n{runnerPath}");

            foreach (var test in tests)
            {
                var psi = new ProcessStartInfo
                {
                    FileName = runnerPath,

                    // TestRunner.exe tFJ_09 Login_As_Admin
                    Arguments = $"{test.ClassName} {test.ExecutionName}",

                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);

                if (process == null)
                    throw new Exception("Unable to start TestRunner.");

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                _resultService.Add(new TestRunResult
                {
                    Id = Guid.NewGuid(),
                    RunId = runId,
                    SuiteName = test.DisplayName,
                    ClassName = test.ClassName,
                    DisplayName = test.DisplayName,
                    Status = process.ExitCode == 0 ? "Passed" : "Failed",
                    Output = output,
                    Error = error,
                    ExecutedAt = DateTime.Now
                });

                results.Add(new
                {
                    test.ClassName,
                    test.DisplayName,
                    // ExitCode = process.ExitCode,
                    Status = process.ExitCode == 0 ? "Passed" : "Failed",
                    Output = output,
                    Error = error,
                    RunId = runId
                });
            }

            return results;
        }
    }
}