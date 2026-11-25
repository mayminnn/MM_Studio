using Automation.Components;
using Automation.Core;
using Automation.Models;
using Automation.Pages;
using FlaUI.Core.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Test_Cases.Attributes;
using Test_Cases.Login;

namespace Test_Cases.Project_Explorer.Flows
{
    [TestClass]
    [TestTags("Project_Explorer", "Flows", "Positive", "Functional_Test", "Automated", "1.0")]
    [TestLinkIds("tFJ-284")]
    public class A284 : BaseTest
    {
        // Fixture: flow sheet "284". Test limits A, B and C each have a Fail Path to label "ret"
        // and their own Fail Bin (FailB1, FailB2, FailB3). A site that passes all three reaches
        // set-device and is binned PassB. Label "ret" is a return step with Result fail, so a
        // site that jumped there leaves the flow failed, keeping the bin of the test it failed.
        //   Site 0 fails A -> FailB1 (811), site 1 fails B -> FailB2 (711),
        //   site 2 fails C -> FailB3 (611), sites 3-7 pass all -> PassB (999)
        private const string FlowName = "284";
        private const string ReturnLabel = "ret";
        private const string PassBin = "PassB";
        private static readonly string[] Tests = { "A", "B", "C" };
        private static readonly string[] FailBins = { "FailB1", "FailB2", "FailB3" };

        // Expected datalog summary, site by site
        private static readonly SiteExpectation[] Expected =
        {
            new SiteExpectation(0, "A", "FailB1", "Fail", 811),
            new SiteExpectation(1, "B", "FailB2", "Fail", 711),
            new SiteExpectation(2, "C", "FailB3", "Fail", 611),
            new SiteExpectation(3, null, PassBin, "Pass", 999),
            new SiteExpectation(4, null, PassBin, "Pass", 999),
            new SiteExpectation(5, null, PassBin, "Pass", 999),
            new SiteExpectation(6, null, PassBin, "Pass", 999),
            new SiteExpectation(7, null, PassBin, "Pass", 999),
        };

        private UIAssistant _assistant = null!;
        private PSPage _psPage = null!;
        private ProjectExplorerPane _projectExplorer = null!;
        private Flow _flowPage = null!;
        private DatalogClient _dc = null!;

        [TestInitialize]
        public void Start()
        {
            _assistant = new UIAssistant(AppManager.Automation);

            var mainWindow = AppManager.MainWindow;

            _psPage = new PSPage(mainWindow, _assistant);
            _psPage.OpenDesignPage();

            var ribbon = new RibbonPane(mainWindow, _assistant);
            ribbon.ClickRibbonButton("Open Project");

            var openProjectWindow =
                _assistant.FindByAutomationId(mainWindow, "OpenProjectForm");

            var openDialog =
                new OpenProjectDialog(openProjectWindow, _assistant);

            openDialog.OpenProject(ProjectName, Version);

            _projectExplorer =
                new ProjectExplorerPane(mainWindow, _assistant);

            // Step 1: navigate to Flows and open the flow sheet "284"
            _projectExplorer.OpenPage("Flows", FlowName);

            _flowPage = new Flow(mainWindow, _assistant, FlowName);
            _dc = new DatalogClient(mainWindow, _assistant);
        }

        [TestMethod]
        public void Validate_Flow_284_Return_Binning()
        {
            // Step 3: validate
            _flowPage.ClickValidate();

            bool validated = Retry.WhileFalse(
                () => _flowPage.VerifyValidationSuccessLog("has successfully completed"),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromMilliseconds(500)).Result;

            Assert.IsTrue(validated, $"Flow '{FlowName}' validation did not complete successfully");

            // Step 4: copy the flow sheet, Start Debug and save the datalog (A95)
            DatalogResult result = A95.RunAndCapture(
                _flowPage, _dc, ProjectName, Version, nameof(A284), FlowName);

            A96.PrintCapture(ProjectName, Version, nameof(A284), FlowName);

            FlowSheetData sheet = FlowSheetStorage.Load(
                FlowSheetStorage.FindLatest(ProjectName, Version, nameof(A284), FlowName)
                    ?? throw new AssertFailedException($"Flow sheet capture for '{FlowName}' was not saved"));

            // Step 2: the sheet is configured as the test case describes
            AssertFixture(sheet);

            A224.PrintStepTable(result, sheet, FlowName);

            // Step 5: verify the datalog
            AssertReturnStepRan(result);
            AssertSiteResults(result);

            // Datalog is consistent (A97) and matches the return-aware flow simulation (A98)
            string[] subFlows = A95.SaveCalledFlowSheets(_projectExplorer, AppManager.MainWindow, _assistant,
                ProjectName, Version, nameof(A284), sheet);

            A97.ValidateCapture(ProjectName, Version, nameof(A284), FlowName);
            A98.CompareCapture(ProjectName, Version, nameof(A284), FlowName, subFlows);

            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine("|     A284 FLOW BIN OUT LAST BIN PASSED       |");
            Console.WriteLine("-----------------------------------------------");

            Cleanup();
        }

        // Each site must end with the expected bin name, STATUS and hard/soft bin. A site that
        // failed a test jumped to "ret", so it has readings up to and including the failed test
        // and none after it; a passing site has a reading for every test
        private static void AssertSiteResults(DatalogResult result)
        {
            int siteCount = result.Summary.BinName.Count;

            Assert.AreEqual(Expected.Length, siteCount,
                $"Flow '{FlowName}' should run on {Expected.Length} sites, but the datalog has {siteCount}");

            List<TestLimitResult> limits = Tests
                .Select(t => A224.FindMainFlowLimit(result, t, FlowName))
                .ToList();

            var issues = new List<string>();

            foreach (SiteExpectation exp in Expected)
            {
                int site = exp.Site;
                string bin = result.Summary.BinName[site];
                string status = At(result.Summary.Status, site) ?? "";
                int? hardBin = AtInt(result.Summary.HardBin, site);
                int? softBin = AtInt(result.Summary.SoftBin, site);

                Console.WriteLine($"Site {site}: {(exp.FailedTest == null ? "passed all" : $"failed {exp.FailedTest}")} " +
                                  $"-> expected '{exp.BinName}' / {exp.Status} / {exp.Bin}; " +
                                  $"datalog '{bin}' / '{status}' / HB {hardBin} SB {softBin}");

                if (!A224.BinMatches(bin, exp.BinName))
                    issues.Add($"site {site}: BIN NAME '{bin}', expected '{exp.BinName}'");

                if (!string.Equals(status, exp.Status, StringComparison.OrdinalIgnoreCase))
                    issues.Add($"site {site}: STATUS '{status}', expected '{exp.Status}'");

                if (hardBin != exp.Bin)
                    issues.Add($"site {site}: HARDBIN {hardBin}, expected {exp.Bin}");

                if (softBin != exp.Bin)
                    issues.Add($"site {site}: SOFTBIN {softBin}, expected {exp.Bin}");

                // Readings: every test up to the failed one (or all of them), none after
                int lastRun = exp.FailedTest == null ? Tests.Length - 1 : Array.IndexOf(Tests, exp.FailedTest);

                for (int i = 0; i < Tests.Length; i++)
                {
                    double value = SiteValue(limits[i], site);
                    bool shouldRun = i <= lastRun;

                    if (shouldRun && double.IsNaN(value))
                    {
                        issues.Add($"site {site}: has no reading for {Tests[i]}, but should have run it");
                        continue;
                    }

                    if (!shouldRun && !double.IsNaN(value))
                    {
                        issues.Add($"site {site}: failed {exp.FailedTest} and should have jumped to " +
                                   $"'{ReturnLabel}', but still ran {Tests[i]} ({value})");
                        continue;
                    }

                    if (!shouldRun)
                        continue;

                    // The failed test must read out of limits; every earlier test within them
                    bool failsHere = Tests[i] == exp.FailedTest;
                    bool within = Within(limits[i], value);

                    if (failsHere && within)
                        issues.Add($"site {site}: {Tests[i]} = {value} is within limits, expected it to fail");

                    if (!failsHere && !within)
                        issues.Add($"site {site}: {Tests[i]} = {value} is out of limits, expected it to pass");
                }
            }

            Assert.IsFalse(issues.Any(),
                $"Flow '{FlowName}' did not bin as expected (a failed test jumps to '{ReturnLabel}' keeping its " +
                $"Fail Bin with STATUS Fail; passing all tests bins {PassBin} with STATUS Pass):\n" +
                string.Join("\n", issues));
        }

        // The flow must finish on the return step
        private static void AssertReturnStepRan(DatalogResult result)
        {
            DatalogStep? last = result.Steps.LastOrDefault();

            Assert.IsNotNull(last, $"Flow '{FlowName}' datalog has no steps");
            Assert.IsTrue(last!.Description.IndexOf("RETURN", StringComparison.OrdinalIgnoreCase) >= 0,
                $"Flow '{FlowName}' should end on the RETURN step, but the last step is '{last.Description}'");
        }

        // Fixture guard: the sheet must match the test case, otherwise the run proves nothing
        private static void AssertFixture(FlowSheetData sheet)
        {
            List<FlowSheetRow> rows = sheet.Rows;

            int returnIndex = rows.FindIndex(r =>
                string.Equals(r.Label?.Trim(), ReturnLabel, StringComparison.OrdinalIgnoreCase));

            if (returnIndex < 0)
                Assert.Fail($"Fixture changed: flow '{FlowName}' has no row labelled '{ReturnLabel}'");

            FlowSheetRow returnRow = rows[returnIndex];

            if (!string.Equals(returnRow.Command?.Trim(), "return", StringComparison.OrdinalIgnoreCase))
                Assert.Fail($"Fixture changed: label '{ReturnLabel}' is on a '{returnRow.Command}' row, expected 'return'");

            if (!string.Equals(returnRow.Result?.Trim(), "fail", StringComparison.OrdinalIgnoreCase))
                Assert.Fail($"Fixture changed: the '{ReturnLabel}' return row has Result '{returnRow.Result}', expected 'fail'");

            // Test limits A, B, C in order, each failing to "ret" with its own Fail Bin
            List<FlowSheetRow> failPathRows = rows
                .Take(returnIndex)
                .Where(r => string.Equals(r.Command, "test-limit", StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(r.FailPath?.Trim(), ReturnLabel, StringComparison.OrdinalIgnoreCase))
                .ToList();

            string[] sheetTests = failPathRows.Select(r => r.TestName?.Trim() ?? "").ToArray();
            string[] sheetFailBins = failPathRows.Select(r => r.FailBin?.Trim() ?? "").ToArray();

            if (!sheetTests.SequenceEqual(Tests, StringComparer.OrdinalIgnoreCase))
                Assert.Fail($"Fixture changed: flow '{FlowName}' test limits with Fail Path '{ReturnLabel}' are " +
                            $"[{string.Join(", ", sheetTests)}], expected [{string.Join(", ", Tests)}]");

            if (!sheetFailBins.SequenceEqual(FailBins, StringComparer.OrdinalIgnoreCase))
                Assert.Fail($"Fixture changed: flow '{FlowName}' Fail Bins are [{string.Join(", ", sheetFailBins)}], " +
                            $"expected [{string.Join(", ", FailBins)}]");

            // Passing all tests reaches set-device, which bins PassB
            int lastTest = rows.IndexOf(failPathRows.Last());

            bool hasPassDevice = rows
                .Skip(lastTest + 1)
                .Take(returnIndex - lastTest - 1)
                .Any(r => string.Equals(r.Command?.Trim(), "set-device", StringComparison.OrdinalIgnoreCase) &&
                          string.Equals(r.PassBin?.Trim(), PassBin, StringComparison.OrdinalIgnoreCase));

            if (!hasPassDevice)
                Assert.Fail($"Fixture changed: flow '{FlowName}' has no set-device row with Pass Bin '{PassBin}' " +
                            $"between the last test and '{ReturnLabel}'");
        }

        private static string? At(List<string> list, int i) => i < list.Count ? list[i] : null;

        private static int? AtInt(List<int> list, int i) => i < list.Count ? list[i] : (int?)null;

        private static double SiteValue(TestLimitResult test, int site)
        {
            return site < test.SiteValues.Count ? test.SiteValues[site] : double.NaN;
        }

        private static bool Within(TestLimitResult test, double value)
        {
            return value >= test.LowLimit && value <= test.HighLimit;
        }

        private sealed class SiteExpectation
        {
            public int Site { get; }
            public string? FailedTest { get; }
            public string BinName { get; }
            public string Status { get; }
            public int Bin { get; }

            public SiteExpectation(int site, string? failedTest, string binName, string status, int bin)
            {
                Site = site;
                FailedTest = failedTest;
                BinName = binName;
                Status = status;
                Bin = bin;
            }
        }
    }
}
