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
    [TestLinkIds("tFJ-225")]
    public class A225 : BaseTest
    {
        // Fixture: the first test-limit row has a Fail Path to the last step ("done") and bins
        // failing sites as ATSPEED_SCAN, so they jump away. Passing sites carry on to the next
        // test-limit, which bins them PassB if they pass it and LEAKAGE_H if they fail it
        private const string FlowName = "FailPath";
        private const string JumpBin = "ATSPEED_SCAN";
        private const string ContinuePassBin = "PassB";
        private const string ContinueFailBin = "LEAKAGE_H";

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
        public void Validate_Fail_Path()
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
                _flowPage, _dc, ProjectName, Version, nameof(A225), FlowName);

            A96.PrintCapture(ProjectName, Version, nameof(A225), FlowName);

            FlowSheetData sheet = FlowSheetStorage.Load(
                FlowSheetStorage.FindLatest(ProjectName, Version, nameof(A225), FlowName)
                    ?? throw new AssertFailedException($"Flow sheet capture for '{FlowName}' was not saved"));

            A224.PrintStepTable(result, sheet, FlowName);

            FlowSheetRow failPathRow = AssertFixtureHasFailPath(sheet);
            List<string> jumpedTests = A224.TestsBetween(sheet, failPathRow, failPathRow.FailPath);

            TestLimitResult firstTest = A224.FindMainFlowLimit(result, failPathRow.TestName, FlowName);
            TestLimitResult lastTest = A224.FindMainFlowLimit(result, jumpedTests.Last(), FlowName);

            int siteCount = result.Summary.BinName.Count;
            var issues = new List<string>();
            int jumped = 0, continuedPass = 0, continuedFail = 0;

            for (int site = 0; site < siteCount; site++)
            {
                double first = SiteValue(firstTest, site);

                if (double.IsNaN(first))
                    Assert.Fail($"Site {site} did not run {firstTest.Name}, so its Fail Path can't be checked");

                bool failedFirst = !Within(firstTest, first);
                string actual = result.Summary.BinName[site];
                string expected;

                if (failedFirst)
                {
                    // Fail Path: binned at the first step and jumped straight to the label
                    jumped++;
                    expected = JumpBin;

                    foreach (string test in jumpedTests)
                    {
                        if (!double.IsNaN(SiteValue(A224.FindMainFlowLimit(result, test, FlowName), site)))
                            issues.Add($"site {site} failed {firstTest.Name} but still ran {test} " +
                                       $"(should have jumped to '{failPathRow.FailPath}')");
                    }
                }
                else
                {
                    // Passed the first step, so it carries on and the next test decides its bin
                    double last = SiteValue(lastTest, site);

                    if (double.IsNaN(last))
                    {
                        issues.Add($"site {site} passed {firstTest.Name} but did not run {lastTest.Name} (should have carried on)");
                        continue;
                    }

                    bool passedLast = Within(lastTest, last);

                    if (passedLast)
                        continuedPass++;
                    else
                        continuedFail++;

                    expected = passedLast ? ContinuePassBin : ContinueFailBin;
                }

                Console.WriteLine($"Site {site}: {firstTest.Name}={first} ({(failedFirst ? "fail -> jump" : "pass -> continue")}) " +
                                $"-> bin '{actual}', expected '{expected}'");

                if (!A224.BinMatches(actual, expected))
                    issues.Add($"site {site}: bin '{actual}', expected '{expected}'");
            }

            // All three outcomes must happen for the check to mean anything
            if (jumped == 0 || continuedPass == 0 || continuedFail == 0)
                Assert.Fail($"Fixture: '{FlowName}' needs sites that jump, continue and pass, and continue and fail; " +
                            $"got {jumped} jumped, {continuedPass} continued and passed, {continuedFail} continued and failed");

            Assert.IsFalse(issues.Any(),
                $"Fail Path '{failPathRow.FailPath}' in flow '{FlowName}' did not behave as expected " +
                $"(failing sites: bin {JumpBin} and jump; passing sites: carry on to {ContinuePassBin} or {ContinueFailBin}):\n" +
                string.Join("\n", issues));

            // A98 needs the sheet of every flow this one calls to simulate it
            string[] subFlows = A95.SaveCalledFlowSheets(_projectExplorer, AppManager.MainWindow, _assistant,
                ProjectName, Version, nameof(A225), sheet);

            // Datalog is consistent (A97) and matches the Fail Path-aware flow simulation (A98)
            A97.ValidateCapture(ProjectName, Version, nameof(A225), FlowName);
            A98.CompareCapture(ProjectName, Version, nameof(A225), FlowName, subFlows);

            Console.WriteLine("-----------------------------------");
            Console.WriteLine("| A225 FAIL PATH BINNING PASSED |");
            Console.WriteLine("-----------------------------------");

            Cleanup();
        }

        // Fixture guard: the first Fail Path row bins ATSPEED_SCAN and points to a label further
        // down, and the last measured test before that label bins PassB / LEAKAGE_H. These tie
        // the expected bins above to what the sheet actually says
        private static FlowSheetRow AssertFixtureHasFailPath(FlowSheetData sheet)
        {
            var rows = sheet.Rows;

            int failPathIndex = rows.FindIndex(r =>
                string.Equals(r.Command, "test-limit", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(r.FailPath));

            if (failPathIndex < 0)
                Assert.Fail($"Fixture changed: flow '{FlowName}' has no test-limit row with a Fail Path");

            FlowSheetRow failPathRow = rows[failPathIndex];

            if (!string.Equals(failPathRow.FailBin, JumpBin, StringComparison.OrdinalIgnoreCase))
                Assert.Fail($"Fixture changed: {failPathRow.TestName} bins failures as '{failPathRow.FailBin}', expected '{JumpBin}'");

            int labelIndex = rows.FindIndex(r =>
                string.Equals(r.Label, failPathRow.FailPath, StringComparison.OrdinalIgnoreCase));

            if (labelIndex <= failPathIndex)
                Assert.Fail($"Fixture changed: Fail Path '{failPathRow.FailPath}' on {failPathRow.TestName} " +
                            "does not point to a label further down the flow");

            string lastTestName = A224.TestsBetween(sheet, failPathRow, failPathRow.FailPath).Last();
            FlowSheetRow lastRow = rows.Skip(failPathIndex + 1).Take(labelIndex - failPathIndex - 1)
                .Last(r => string.Equals(r.TestName, lastTestName, StringComparison.OrdinalIgnoreCase));

            if (!string.Equals(lastRow.PassBin, ContinuePassBin, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(lastRow.FailBin, ContinueFailBin, StringComparison.OrdinalIgnoreCase))
                Assert.Fail($"Fixture changed: {lastRow.TestName} bins '{lastRow.PassBin}' / '{lastRow.FailBin}', " +
                            $"expected '{ContinuePassBin}' / '{ContinueFailBin}'");

            return failPathRow;
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
