using System.Collections.Concurrent;
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

        // Cancellation handle for each run that is still in progress
        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _activeRuns = new();

        public ExecutionService(
            ResultService resultService,
            ExecutionTracker tracker)
        {
            _resultService = resultService;
            _tracker = tracker;
        }

        public async Task<RunExecutionResponse> RunSuiteAsync(
            Guid runId,
            string suiteName,
            List<SuiteTest> tests,
            string projectName = "",
            string version = "",
            string installerVersion = "")
        {
            var results = new List<object>();
            // var runId = Guid.NewGuid();
            var status = new ExecutionStatus
            {
                RunId = runId,
                SuiteName = suiteName,
                ProjectName = projectName?.Trim() ?? "",
                Version = version?.Trim() ?? "",
                InstallerVersion = installerVersion?.Trim() ?? "",
                TotalTests = tests.Count,
                CompletedTests = 0,
                Finished = false
            };

            _tracker.Add(status);

            using var cts = new CancellationTokenSource();
            _activeRuns[runId] = cts;

            try
            {
                string runnerPath =
                    @"..\TestRunner\bin\Debug\net48\TestRunner.exe";

                if (!File.Exists(runnerPath))
                    throw new Exception($"TestRunner not found:\n{runnerPath}");

                var orderedTests = tests.OrderBy(x => x.Order).ToList();
                int processed = 0;

                // foreach (var test in tests)
                foreach (var test in orderedTests)
                {
                    if (cts.IsCancellationRequested)
                        break;

                    status.CurrentTest = test.DisplayName;
                    _tracker.Update(status);

                    var psi = new ProcessStartInfo
                    {
                        FileName = runnerPath,

                        // TestRunner.exe A09 Login_As_Admin
                        Arguments = $"{test.ClassName} {test.ExecutionName}",

                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    // Read by TestCases' TestSettings
                    if (!string.IsNullOrEmpty(status.ProjectName))
                        psi.Environment["MM_STUDIO_PROJECT"] = status.ProjectName;

                    if (!string.IsNullOrEmpty(status.Version))
                        psi.Environment["MM_STUDIO_VERSION"] = status.Version;

                    if (!string.IsNullOrEmpty(status.InstallerVersion))
                        psi.Environment["MM_STUDIO_INSTALLER_VERSION"] = status.InstallerVersion;

                    using var process = Process.Start(psi);

                    if (process == null)
                        throw new Exception("Unable to start TestRunner.");

                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();

                    bool stopped = false;

                    try
                    {
                        await process.WaitForExitAsync(cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        // Kill TestRunner and everything it launched (e.g. techFlowJazz)
                        stopped = true;
                        process.Kill(entireProcessTree: true);
                        await process.WaitForExitAsync();
                    }

                    string output = await outputTask;
                    string error = await errorTask;

                    if (stopped)
                    {
                        AddResult(status, test, "Stopped", output, error);
                        processed++;
                        break;
                    }

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
                        ProjectName = status.ProjectName,
                        Version = status.Version,
                        InstallerVersion = status.InstallerVersion,
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
                    processed++;

                }

                // List the tests a stopped run never reached
                if (cts.IsCancellationRequested)
                {
                    foreach (var test in orderedTests.Skip(processed))
                        AddResult(status, test, "Not Run", "", "");
                }
            }
            finally
            {
                _activeRuns.TryRemove(runId, out _);

                status.CurrentTest = "";
                status.Stopped = cts.IsCancellationRequested && status.CompletedTests < status.TotalTests;
                status.Finished = true;
                _tracker.Update(status);
            }

            // return results;
            return new RunExecutionResponse
            {
                RunId = runId,
                Results = results
            };
        }

        private void AddResult(ExecutionStatus status, SuiteTest test, string result, string output, string error)
        {
            var runResult = new TestRunResult
            {
                Id = Guid.NewGuid(),
                RunId = status.RunId,
                SuiteName = status.SuiteName,
                ProjectName = status.ProjectName,
                Version = status.Version,
                InstallerVersion = status.InstallerVersion,
                ClassName = test.ClassName,
                DisplayName = test.DisplayName,
                Status = result,
                Output = output,
                Error = error,
                ExecutedAt = DateTime.Now
            };

            _resultService.Add(runResult);

            status.Results.Add(runResult);
            _tracker.Update(status);
        }

        // Returns false if the run is unknown or already finished
        public bool Stop(Guid runId)
        {
            if (!_activeRuns.TryGetValue(runId, out var cts))
                return false;

            try
            {
                cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // Run finished between the lookup and the cancel
                return false;
            }

            return true;
        }
    }
}