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
    [TestTags("Project_Explorer", "Flows", "Subflow", "Positive", "Automated", "1.0")]
    [TestLinkIds("tFJ-283")]
    public class A283 : BaseTest
    {
        // Fixture: flow sheet "283". Sites 6 and 7 fail and are binned VREF_Stress_ext;
        // sites 0 to 5 pass and are binned PassB
        private const string FlowName = "283";
        private const string FailBin = "VREF_Stress_ext";
        private const string PassBin = "PassB";
        private static readonly int[] SitesExpectedFailBin = { 6, 7 };
        private static readonly int[] SitesExpectedPassBin = { 0, 1, 2, 3, 4, 5 };

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

            // Open the flow sheet "283"
            _projectExplorer.OpenPage("Flows", FlowName);

            _flowPage = new Flow(mainWindow, _assistant, FlowName);
            _dc = new DatalogClient(mainWindow, _assistant);
        }

        [TestMethod]
        public void Validate_Flow_283_Binning()
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
                _flowPage, _dc, ProjectName, Version, nameof(A283), FlowName);

            A96.PrintCapture(ProjectName, Version, nameof(A283), FlowName);

            FlowSheetData sheet = LoadSheet(FlowName);

            // A98 needs the sheet of every flow this one calls to simulate it
            string[] subFlows = A95.SaveCalledFlowSheets(_projectExplorer, AppManager.MainWindow, _assistant,
                ProjectName, Version, nameof(A283), sheet);

            var sheets = new List<FlowSheetData> { sheet };
            sheets.AddRange(subFlows.Select(LoadSheet));

            AssertFixture(sheets);

            AssertSiteBinning(result);

            // Datalog is consistent (A97) and matches the flow simulation (A98)
            A97.ValidateCapture(ProjectName, Version, nameof(A283), FlowName);
            A98.CompareCapture(ProjectName, Version, nameof(A283), FlowName, subFlows);

            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine("|        A283 FLOW 283 BINNING PASSED         |");
            Console.WriteLine("-----------------------------------------------");

            Cleanup();
        }

        // Sites 6 and 7 must end binned VREF_Stress_ext with STATUS Fail; sites 0 to 5 must end
        // binned PassB with STATUS Pass
        private static void AssertSiteBinning(DatalogResult result)
        {
            int expectedSiteCount = SitesExpectedFailBin.Length + SitesExpectedPassBin.Length;
            int siteCount = result.Summary.BinName.Count;

            Assert.AreEqual(expectedSiteCount, siteCount,
                $"Flow '{FlowName}' should run on {expectedSiteCount} sites, but the datalog has {siteCount}");

            var issues = new List<string>();

            for (int site = 0; site < siteCount; site++)
            {
                string bin = result.Summary.BinName[site];
                string status = site < result.Summary.Status.Count ? result.Summary.Status[site] : "";

                bool shouldFail = SitesExpectedFailBin.Contains(site);
                string expectedBin = shouldFail ? FailBin : PassBin;
                string expectedStatus = shouldFail ? "Fail" : "Pass";

                Console.WriteLine($"Site {site}: bin '{bin}', STATUS '{status}' " +
                                  $"(expected '{expectedBin}', '{expectedStatus}')");

                // BinMatches allows for the datalog cutting VREF_Stress_ext off at the column width
                if (!A224.BinMatches(bin, expectedBin))
                    issues.Add($"site {site}: bin '{bin}', expected '{expectedBin}'");

                if (!string.Equals(status, expectedStatus, StringComparison.OrdinalIgnoreCase))
                    issues.Add($"site {site}: STATUS '{status}', expected '{expectedStatus}'");
            }

            Console.WriteLine($"Expected {FailBin}: sites {string.Join(", ", SitesExpectedFailBin)}");
            Console.WriteLine($"Expected {PassBin}: sites {string.Join(", ", SitesExpectedPassBin)}");

            Assert.IsFalse(issues.Any(),
                $"Flow '{FlowName}' did not bin as expected (sites {string.Join(", ", SitesExpectedFailBin)} " +
                $"-> {FailBin}, sites {string.Join(", ", SitesExpectedPassBin)} -> {PassBin}):\n" +
                string.Join("\n", issues));
        }

        // Fixture guard: the flow (or a flow it calls) must be able to produce both bins,
        // otherwise the run proves nothing
        private static void AssertFixture(List<FlowSheetData> sheets)
        {
            var rows = sheets.SelectMany(s => s.Rows).ToList();

            if (!rows.Any(r => string.Equals(r.FailBin?.Trim(), FailBin, StringComparison.OrdinalIgnoreCase)))
                Assert.Fail($"Fixture changed: flow '{FlowName}' and the flows it calls have no row with Fail Bin '{FailBin}'");

            if (!rows.Any(r => string.Equals(r.PassBin?.Trim(), PassBin, StringComparison.OrdinalIgnoreCase)))
                Assert.Fail($"Fixture changed: flow '{FlowName}' and the flows it calls have no row with Pass Bin '{PassBin}'");
        }

        private FlowSheetData LoadSheet(string flowName)
        {
            return FlowSheetStorage.Load(
                FlowSheetStorage.FindLatest(ProjectName, Version, nameof(A283), flowName)
                    ?? throw new AssertFailedException($"Flow sheet capture for '{flowName}' was not saved"));
        }
    }
}
