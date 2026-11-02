using Automation.Components;
using Automation.Core;
using Automation.Models;
using Automation.Pages;
using FlaUI.Core.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Test_Cases.Attributes;
using Test_Cases.Login;

namespace Test_Cases.Project_Explorer.Flows
{
    [TestClass]
    [TestTags("Project_Explorer", "Flows", "Positive", "Automated")]
    [TestLinkIds("tFJ-224")]
    public class A224 : BaseTest
    {
        // Fixture: an early test-limit row has a Pass Path to a label and bins passing sites
        // as Pass. A later row would bin passing sites as PassB, but they have already jumped
        // past it. Failing sites carry on down the flow and end up binned as LEAKAGE_H
        private const string FlowName = "PassPath";
        private const string ExpectedPassBin = "Pass";
        private const string ExpectedFailBin = "LEAKAGE_H";
        private const string LaterPassBin = "PassB";

        // The datalog's BIN NAME column cuts names off at this many characters
        internal const int BinNameColumnWidth = 8;

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

            // Step 1: open the flow sheet
            _projectExplorer.OpenPage("Flows", FlowName);

            _flowPage = new Flow(mainWindow, _assistant, FlowName);
            _dc = new DatalogClient(mainWindow, _assistant);
        }

        [TestMethod]
        public void Validate_Pass_Path()
        {
            // Step 5: validate
            _flowPage.ClickValidate();

            bool validated = Retry.WhileFalse(
                () => _flowPage.VerifyValidationSuccessLog("has successfully completed"),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromMilliseconds(500)).Result;

            Assert.IsTrue(validated, $"Flow '{FlowName}' validation did not complete successfully");

            // Step 6: copy the flow sheet, Start Debug and save the datalog (A95)
            DatalogResult result = A95.RunAndCapture(
                _flowPage, _dc, ProjectName, Version, nameof(A224), FlowName);

            A96.PrintCapture(ProjectName, Version, nameof(A224), FlowName);

            FlowSheetData sheet = FlowSheetStorage.Load(
                FlowSheetStorage.FindLatest(ProjectName, Version, nameof(A224), FlowName)
                    ?? throw new AssertFailedException($"Flow sheet capture for '{FlowName}' was not saved"));

            PrintStepTable(result, sheet, FlowName);

            // Step 7: pass sites jump and are binned Pass; fail sites carry on to LEAKAGE_H
            FlowSheetRow passPathRow = AssertFixtureHasPassPath(sheet);
            TestLimitResult passPathTest = FindMainFlowLimit(result, passPathRow.TestName);

            int siteCount = result.Summary.BinName.Count;
            var issues = new List<string>();
            int passSites = 0;
            var sitePassed = new bool[siteCount];

            for (int site = 0; site < siteCount; site++)
            {
                if (site >= passPathTest.SiteValues.Count)
                    Assert.Fail($"No {passPathTest.Name} value recorded for site {site}");

                double value = passPathTest.SiteValues[site];

                // Every site should reach the Pass Path test; one that didn't can't be judged
                if (double.IsNaN(value))
                    Assert.Fail($"Site {site} did not run {passPathTest.Name}, so its Pass Path can't be checked");

                bool passed = value >= passPathTest.LowLimit && value <= passPathTest.HighLimit;
                string expected = passed ? ExpectedPassBin : ExpectedFailBin;
                string actual = result.Summary.BinName[site];

                sitePassed[site] = passed;

                if (passed)
                    passSites++;

                Console.WriteLine($"Site {site}: {passPathTest.Name}={value} " +
                                $"({(passed ? "pass" : "fail")}) -> bin '{actual}', expected '{expected}'");

                if (!BinMatches(actual, expected))
                {
                    issues.Add($"site {site} ({passPathTest.Name}={value}, {(passed ? "passed" : "failed")}): " +
                               $"bin '{actual}', expected '{expected}'");
                }
            }

            // Both paths must actually be exercised for the check to mean anything
            Assert.IsTrue(passSites > 0 && passSites < siteCount,
                $"Fixture: '{FlowName}' needs both passing and failing sites on {passPathTest.Name}, " +
                $"but {passSites} of {siteCount} sites passed");

            Assert.IsFalse(issues.Any(),
                $"Wrong bins in flow '{FlowName}' (pass sites should jump to '{passPathRow.PassPath}' " +
                $"and bin {ExpectedPassBin}, not {LaterPassBin}; fail sites should end at {ExpectedFailBin}):\n" +
                string.Join("\n", issues));

            // The jump itself: tests between the Pass Path row and its label must have no reading
            // for passing sites (they jumped straight to the label) and a reading for failing
            // sites (they carried on through them)
            var jumpIssues = new List<string>();

            foreach (string skippedTest in TestsJumpedOver(sheet, passPathRow))
            {
                TestLimitResult limit = FindMainFlowLimit(result, skippedTest);

                for (int site = 0; site < siteCount; site++)
                {
                    bool ran = site < limit.SiteValues.Count && !double.IsNaN(limit.SiteValues[site]);

                    if (sitePassed[site] && ran)
                        jumpIssues.Add($"site {site} passed but still ran {skippedTest} (should have jumped to '{passPathRow.PassPath}')");

                    if (!sitePassed[site] && !ran)
                        jumpIssues.Add($"site {site} failed but did not run {skippedTest} (should have carried on)");
                }

                Console.WriteLine($"{skippedTest}: run by sites " +
                    string.Join(", ", Enumerable.Range(0, siteCount)
                        .Where(s => s < limit.SiteValues.Count && !double.IsNaN(limit.SiteValues[s]))));
            }

            Assert.IsFalse(jumpIssues.Any(),
                $"Pass Path '{passPathRow.PassPath}' in flow '{FlowName}' did not jump as expected:\n" +
                string.Join("\n", jumpIssues));

            // A98 needs the sheet of every flow this one calls to simulate it
            string[] subFlows = A95.SaveCalledFlowSheets(_projectExplorer, AppManager.MainWindow, _assistant,
                ProjectName, Version, nameof(A224), sheet);

            // Datalog is consistent (A97) and matches the Pass Path-aware flow simulation (A98)
            A97.ValidateCapture(ProjectName, Version, nameof(A224), FlowName);
            A98.CompareCapture(ProjectName, Version, nameof(A224), FlowName, subFlows);

            Console.WriteLine("-----------------------------------");
            Console.WriteLine("| A224 PASS PATH BINNING PASSED |");
            Console.WriteLine("-----------------------------------");

            Cleanup();
        }

        // Fixture guard: a test-limit row with a Pass Path whose label exists further down, and a
        // later row binning passes as PassB. Without both, "bin is Pass, not PassB" proves nothing
        private static FlowSheetRow AssertFixtureHasPassPath(FlowSheetData sheet)
        {
            var rows = sheet.Rows;

            int passPathIndex = rows.FindIndex(r =>
                string.Equals(r.Command, "test-limit", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(r.PassPath));

            if (passPathIndex < 0)
                Assert.Fail($"Fixture changed: flow '{FlowName}' has no test-limit row with a Pass Path");

            FlowSheetRow passPathRow = rows[passPathIndex];

            int labelIndex = rows.FindIndex(r =>
                string.Equals(r.Label, passPathRow.PassPath, StringComparison.OrdinalIgnoreCase));

            if (labelIndex <= passPathIndex)
                Assert.Fail($"Fixture changed: Pass Path '{passPathRow.PassPath}' on {passPathRow.TestName} " +
                            "does not point to a label further down the flow");

            Assert.IsTrue(rows.Skip(passPathIndex + 1).Any(r =>
                    string.Equals(r.PassBin, LaterPassBin, StringComparison.OrdinalIgnoreCase)),
                $"Fixture changed: no row after {passPathRow.TestName} bins passes as {LaterPassBin}");

            return passPathRow;
        }

        private static List<string> TestsJumpedOver(FlowSheetData sheet, FlowSheetRow passPathRow)
        {
            return TestsBetween(sheet, passPathRow, passPathRow.PassPath);
        }

        // Measured tests (test-limit rows) between a row and the row carrying the given label.
        // Shared with A225 (Fail Path)
        internal static List<string> TestsBetween(FlowSheetData sheet, FlowSheetRow fromRow, string label)
        {
            var rows = sheet.Rows;
            int from = rows.IndexOf(fromRow);
            int to = rows.FindIndex(r => string.Equals(r.Label, label, StringComparison.OrdinalIgnoreCase));

            var tests = rows.Skip(from + 1).Take(to - from - 1)
                .Where(r => string.Equals(r.Command, "test-limit", StringComparison.OrdinalIgnoreCase) &&
                            !string.IsNullOrWhiteSpace(r.TestName))
                .Select(r => r.TestName)
                .ToList();

            if (tests.Count == 0)
                Assert.Fail($"Fixture changed: no measured test between {fromRow.TestName} and label '{label}'");

            return tests;
        }

        private static TestLimitResult FindMainFlowLimit(DatalogResult result, string testName)
        {
            return FindMainFlowLimit(result, testName, FlowName);
        }

        // The run of the test in this flow itself, not in a called sub-flow. Shared with A225
        internal static TestLimitResult FindMainFlowLimit(DatalogResult result, string testName, string flowName)
        {
            return result.TestLimits.FirstOrDefault(x =>
                       string.Equals(x.Name, testName, StringComparison.OrdinalIgnoreCase) &&
                       (string.IsNullOrEmpty(x.FlowPath) ||
                        string.Equals(x.FlowPath, flowName, StringComparison.OrdinalIgnoreCase)))
                   ?? throw new AssertFailedException($"{testName} not found in the datalog of flow '{flowName}'");
        }

        // Exact match, or a name cut off by the BIN NAME column. "PassB" must not match "Pass",
        // so a shorter actual name only counts when it fills the whole column. Shared with A225
        internal static bool BinMatches(string? actual, string expected)
        {
            if (string.IsNullOrEmpty(actual))
                return false;

            return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase) ||
                   (actual!.Length >= BinNameColumnWidth &&
                    expected.StartsWith(actual, StringComparison.OrdinalIgnoreCase));
        }

        // Console report: what each site did at every measured step, one column per group of
        // sites that took the same route, then each group's final bin and STATUS. Shared with
        // A225 and A227
        internal static void PrintStepTable(DatalogResult result, FlowSheetData sheet, string flowName)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<FlowSheetRow> rows = sheet.Rows
                .Where(r => string.Equals(r.Command, "test-limit", StringComparison.OrdinalIgnoreCase) &&
                            !string.IsNullOrWhiteSpace(r.TestName) &&
                            seen.Add(r.TestName))
                .ToList();

            List<TestLimitResult> limits = rows
                .Select(r => FindMainFlowLimit(result, r.TestName, flowName))
                .ToList();

            int siteCount = result.Summary.BinName.Count;

            // One cell per step for each site, then its final bin
            var siteCells = new List<string[]>();

            for (int site = 0; site < siteCount; site++)
            {
                var cells = new string[rows.Count + 1];

                for (int i = 0; i < rows.Count; i++)
                    cells[i] = StepCell(rows[i], limits[i], site);

                string status = site < result.Summary.Status.Count ? result.Summary.Status[site] : "?";
                cells[rows.Count] = $"{result.Summary.BinName[site]}, STATUS {status}";

                siteCells.Add(cells);
            }

            // Sites that took exactly the same route share a column
            var groups = Enumerable.Range(0, siteCount)
                .GroupBy(s => string.Join("\u0001", siteCells[s]))
                .Select(g => g.ToList())
                .OrderBy(g => g[0])
                .ToList();

            var header = new List<string> { "Step", "Limits" };
            header.AddRange(groups.Select(g => (g.Count == 1 ? "Site " : "Sites ") + string.Join(", ", g)));

            var table = new List<List<string>> { header };

            for (int i = 0; i < rows.Count; i++)
            {
                var line = new List<string>
                {
                    rows[i].TestName + (string.IsNullOrWhiteSpace(rows[i].Result) ? "" : $" (Result {rows[i].Result})"),
                    $"{limits[i].LowLimit:0.###}-{limits[i].HighLimit:0.###}"
                };
                line.AddRange(groups.Select(g => siteCells[g[0]][i]));
                table.Add(line);
            }

            var final = new List<string> { "Final", "" };
            final.AddRange(groups.Select(g => siteCells[g[0]][rows.Count]));
            table.Add(final);

            int[] widths = Enumerable.Range(0, header.Count)
                .Select(c => table.Max(l => l[c].Length))
                .ToArray();

            Console.WriteLine();
            Console.WriteLine($"Flow '{flowName}' step by step:");

            for (int l = 0; l < table.Count; l++)
            {
                Console.WriteLine("| " + string.Join(" | ", table[l].Select((cell, c) => cell.PadRight(widths[c]))) + " |");

                if (l == 0)
                    Console.WriteLine("|" + string.Join("|", widths.Select(w => new string('-', w + 2))) + "|");
            }

            Console.WriteLine();
        }

        // "pass, PassB, carry on", "fail, LEAKAGE_H, kicked out", "fail, ATSPEED_SCAN, jump to 'done'"
        // or "no reading" when the site never got to this step
        private static string StepCell(FlowSheetRow row, TestLimitResult limit, int site)
        {
            double value = site < limit.SiteValues.Count ? limit.SiteValues[site] : double.NaN;

            if (double.IsNaN(value))
                return "no reading";

            bool passed = value >= limit.LowLimit && value <= limit.HighLimit;
            string bin = passed ? row.PassBin : row.FailBin;
            string path = passed ? row.PassPath : row.FailPath;

            string action;

            if (string.Equals(row.Result?.Trim(), passed ? "pass" : "fail", StringComparison.OrdinalIgnoreCase))
                action = "kicked out";
            else if (!string.IsNullOrWhiteSpace(path))
                action = $"jump to '{path}'";
            else
                action = "carry on";

            return $"{(passed ? "pass" : "fail")}, {(string.IsNullOrWhiteSpace(bin) ? "no bin" : bin)}, {action}";
        }
    }
}
