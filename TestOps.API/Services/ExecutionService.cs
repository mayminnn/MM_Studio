using System.Diagnostics;
using TestOps.API.Models;

namespace TestOps.API.Services
{
    public class ExecutionService
    {
        // private readonly ResultService _resultService;

        // public ExecutionService(ResultService resultService)
        // {
        //     _resultService = resultService;
        // }

        private readonly ResultService _resultService;
        private readonly ExecutionTracker _tracker;

        public ExecutionService(
            ResultService resultService,
            ExecutionTracker tracker)
        {
            _resultService = resultService;
            _tracker = tracker;
        }

        public async Task<RunExecutionResponse> RunSuiteAsync(Guid runId, string suiteName, List<SuiteTest> tests)
        {
            var results = new List<object>();
            // var runId = Guid.NewGuid();
            var status = new ExecutionStatus
            {
                RunId = runId,
                SuiteName = suiteName,
                TotalTests = tests.Count,
                CompletedTests = 0,
                Finished = false
            };

            _tracker.Add(status);

            string runnerPath =
                @"..\TestRunner\bin\Debug\net48\TestRunner.exe";

            if (!File.Exists(runnerPath))
                throw new Exception($"TestRunner not found:\n{runnerPath}");

            // foreach (var test in tests)
            foreach (var test in tests.OrderBy(x => x.Order))
            {
                status.CurrentTest = test.DisplayName;
                _tracker.Update(status);

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

                // _resultService.Add(new TestRunResult
                // {
                //     Id = Guid.NewGuid(),
                //     RunId = runId,
                //     SuiteName = suiteName,
                //     ClassName = test.ClassName,
                //     DisplayName = test.DisplayName,
                //     Status = process.ExitCode == 0 ? "Passed" : "Failed",
                //     Output = output,
                //     Error = error,
                //     ExecutedAt = DateTime.Now
                // });

                // results.Add(new
                // {
                //     test.ClassName,
                //     test.DisplayName,
                //     // ExitCode = process.ExitCode,
                //     Status = process.ExitCode == 0 ? "Passed" : "Failed",
                //     Output = output,
                //     Error = error,
                //     RunId = runId
                // });

                // status.CompletedTests++;

                // status.Results.Add(new TestRunResult
                // {
                //     Id = Guid.NewGuid(),
                //     RunId = runId,
                //     SuiteName = suiteName,
                //     ClassName = test.ClassName,
                //     DisplayName = test.DisplayName,
                //     Status = process.ExitCode == 0 ? "Passed" : "Failed",
                //     Output = output,
                //     Error = error,
                //     ExecutedAt = DateTime.Now
                // });

                var result = new TestRunResult
                {
                    Id = Guid.NewGuid(),
                    RunId = runId,
                    SuiteName = suiteName,
                    ClassName = test.ClassName,
                    DisplayName = test.DisplayName,
                    Status = process.ExitCode == 0 ? "Passed" : "Failed",
                    Output = output,
                    Error = error,
                    ExecutedAt = DateTime.Now
                };

                _resultService.Add(result);

                status.CompletedTests++;
                status.Results.Add(result);
                _tracker.Update(status);

            }
            status.CurrentTest = "";
            status.Finished = true;
            _tracker.Update(status);

            // return results;
            return new RunExecutionResponse
            {
                RunId = runId,
                Results = results
            };
        }
    }
}