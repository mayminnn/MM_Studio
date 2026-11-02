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
    [TestLinkIds("tFJ-227")]
    public class A227 : BaseTest
    {
        // Fixture: test-limit rows with a Result (pass / fail) in column P. A site whose outcome
        // matches the Result is binned at that step and leaves the flow, so it must not run any
        // later test; the other sites carry on. Sites 0-2 and 5-7 fail ATSPEED_SCAN3_375ns, so only
        // sites 3 and 4 are left to run ATSPEED_SCAN2_125ns
        private const string FlowName = "FailResult";
        private const string ExpectedResult = "fail";
        private const string CheckedTest = "ATSPEED_SCAN2_125ns";
        private static readonly int[] SitesExpectedAtCheckedTest = { 3, 4 };

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

            // Open the flow sheet
            _projectExplorer.OpenPage("Flows", FlowName);

            _flowPage = new Flow(mainWindow, _assistant, FlowName);
            _dc = new DatalogClient(mainWindow, _assistant);
        }

        [TestMethod]
        public void Validate_Result_Binning()
        {
            // Validate
            _flowPage.ClickValidate();

            bool validated = Retry.WhileFalse(
                () => _flowPage.VerifyValidationSuccessLog("has successfully completed"),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromMilliseconds(500)).Result;

            Assert.IsTrue(validated, $"Flow '{FlowName}' validation did not complete successfully");

            // Copy the flow sheet, Start Debug and save the datalog (A95)
            DatalogResult result = A95.RunAndCapture(
                _flowPage, _dc, ProjectName, Version, nameof(A227), FlowName);

            A96.PrintCapture(ProjectName, Version, nameof(A227), FlowName);

            FlowSheetData sheet = FlowSheetStorage.Load(
                FlowSheetStorage.FindLatest(ProjectName, Version, nameof(A227), FlowName)
                    ?? throw new AssertFailedException($"Flow sheet capture for '{FlowName}' was not saved"));

            A224.PrintStepTable(result, sheet, FlowName);

            AssertResultBinning(result, sheet, FlowName, ExpectedResult, CheckedTest, SitesExpectedAtCheckedTest);

            // A98 needs the sheet of every flow this one calls to simulate it
            string[] subFlows = A95.SaveCalledFlowSheets(_projectExplorer, AppManager.MainWindow, _assistant,
                ProjectName, Version, nameof(A227), sheet);

            // Datalog is consistent (A97) and matches the Result-aware flow simulation (A98)
            A97.ValidateCapture(ProjectName, Version, nameof(A227), FlowName);
            A98.CompareCapture(ProjectName, Version, nameof(A227), FlowName, subFlows);

            Console.WriteLine("-----------------------------------");
            Console.WriteLine("|   A227 RESULT BINNING PASSED   |");
            Console.WriteLine("-----------------------------------");

            Cleanup();
        }

        // Result binning check shared with A408 (pass Result): walk each site through the
        // measured tests; a site whose outcome matches a row's Result (column P) must be binned
        // with that branch's bin and run no later test. Only expectedSites reach checkedTest
        internal static void AssertResultBinning(DatalogResult result, FlowSheetData sheet, string flowName,
            string expectedResult, string checkedTest, int[] expectedSites)
        {
            List<FlowSheetRow> limitRows = MeasuredRows(sheet);
            AssertFixtureHasResult(limitRows, flowName, expectedResult, checkedTest);

            int siteCount = result.Summary.BinName.Count;
            var issues = new List<string>();
            int binnedOut = 0, carriedOn = 0;

            for (int site = 0; site < siteCount; site++)
            {
                string actual = result.Summary.BinName[site];
                int exitIndex = -1;
                string exitBin = "", exitResult = "";

                // Walk the measured tests in sheet order until one bins the site out with a Result
                for (int i = 0; i < limitRows.Count; i++)
                {
                    FlowSheetRow row = limitRows[i];
                    TestLimitResult limit = A224.FindMainFlowLimit(result, row.TestName, flowName);
                    double value = SiteValue(limit, site);

                    if (double.IsNaN(value))
                        continue;

                    bool passed = Within(limit, value);
                    string bin = passed ? row.PassBin : row.FailBin;

                    // Result "pass" kicks out the passing sites, "fail" the failing ones;
                    // the other sites carry on to the next step
                    if (!ResultMatches(row.Result, passed))
                        continue;

                    exitIndex = i;
                    exitBin = bin;
                    exitResult = row.Result;

                    Console.WriteLine($"Site {site}: {(passed ? "passed" : "failed")} {row.TestName}={value} " +
                                    $"-> Result '{exitResult}', binned '{exitBin}' (datalog bin '{actual}')");
                    break;
                }

                if (exitIndex < 0)
                {
                    carriedOn++;
                    Console.WriteLine($"Site {site}: no Result triggered, carried on to the end (bin '{actual}')");
                    continue;
                }

                binnedOut++;

                if (!A224.BinMatches(actual, exitBin))
                    issues.Add($"site {site}: bin '{actual}', expected '{exitBin}' from {limitRows[exitIndex].TestName} " +
                               $"(Result '{exitResult}')");

                // Binned out, so every later test must have no reading for this site
                foreach (FlowSheetRow later in limitRows.Skip(exitIndex + 1))
                {
                    double laterValue = SiteValue(A224.FindMainFlowLimit(result, later.TestName, flowName), site);

                    if (!double.IsNaN(laterValue))
                        issues.Add($"site {site} was binned out at {limitRows[exitIndex].TestName} " +
                                   $"(Result '{exitResult}') but still ran {later.TestName} ({laterValue})");
                }
            }

            // Both outcomes must happen for the check to mean anything
            if (binnedOut == 0 || carriedOn == 0)
                Assert.Fail($"Fixture: '{flowName}' needs sites that are binned out by a Result and sites that carry on; " +
                            $"got {binnedOut} binned out and {carriedOn} carried on");

            // Only the sites still in the flow reach the checked test
            TestLimitResult checkedLimit = A224.FindMainFlowLimit(result, checkedTest, flowName);
            int[] ranChecked = Enumerable.Range(0, siteCount)
                .Where(s => !double.IsNaN(SiteValue(checkedLimit, s)))
                .ToArray();

            Console.WriteLine($"{checkedTest}: run by sites {string.Join(", ", ranChecked)}");

            if (!ranChecked.SequenceEqual(expectedSites))
                issues.Add($"{checkedTest} was run by sites [{string.Join(", ", ranChecked)}], " +
                           $"expected only [{string.Join(", ", expectedSites)}]");

            Assert.IsFalse(issues.Any(),
                $"Result binning in flow '{flowName}' did not behave as expected " +
                "(a site that triggers a Result must be binned at that step and run no later test):\n" +
                string.Join("\n", issues));
        }

        // Measured tests (test-limit rows) in sheet order, one per test name
        private static List<FlowSheetRow> MeasuredRows(FlowSheetData sheet)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            return sheet.Rows
                .Where(r => string.Equals(r.Command, "test-limit", StringComparison.OrdinalIgnoreCase) &&
                            !string.IsNullOrWhiteSpace(r.TestName) &&
                            seen.Add(r.TestName))
                .ToList();
        }

        private static bool ResultMatches(string result, bool passed)
        {
            return string.Equals(result?.Trim(), passed ? "pass" : "fail", StringComparison.OrdinalIgnoreCase);
        }

        // Fixture guard: a test-limit row has the expected Result (pass / fail), and the checked
        // test comes after it. Without these the run proves nothing
        private static void AssertFixtureHasResult(List<FlowSheetRow> limitRows, string flowName,
            string expectedResult, string checkedTest)
        {
            int resultIndex = limitRows.FindIndex(r =>
                string.Equals(r.Result?.Trim(), expectedResult, StringComparison.OrdinalIgnoreCase));

            if (resultIndex < 0)
                Assert.Fail($"Fixture changed: flow '{flowName}' has no test-limit row with Result '{expectedResult}'");

            int checkedIndex = limitRows.FindIndex(r =>
                string.Equals(r.TestName, checkedTest, StringComparison.OrdinalIgnoreCase));

            if (checkedIndex <= resultIndex)
                Assert.Fail($"Fixture changed: {checkedTest} is not after the Result row " +
                            $"({limitRows[resultIndex].TestName}) in flow '{flowName}'");
        }

        private static double SiteValue(TestLimitResult test, int site)
        {
            return site < test.SiteValues.Count ? test.SiteValues[site] : double.NaN;
        }

        private static bool Within(TestLimitResult test, double value)
        {
            return value >= test.LowLimit && value <= test.HighLimit;
        }
    }
}
