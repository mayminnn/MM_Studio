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
    [TestLinkIds("tFJ-408")]
    public class A408 : BaseTest
    {
        // Fixture: same flow as A227 but ATSPEED_SCAN3_375ns has Result "pass" in column P.
        // Sites 3 and 4 pass it, are binned PassB and leave the flow; the rest fail it, carry on
        // and are the only sites left to run ATSPEED_SCAN2_125ns
        private const string FlowName = "PassResult";
        private const string ExpectedResult = "pass";
        private const string CheckedTest = "ATSPEED_SCAN2_125ns";
        private static readonly int[] SitesExpectedAtCheckedTest = { 0, 1, 2, 5, 6, 7 };

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
        public void Validate_Pass_Result_Binning()
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
                _flowPage, _dc, ProjectName, Version, nameof(A408), FlowName);

            A96.PrintCapture(ProjectName, Version, nameof(A408), FlowName);

            FlowSheetData sheet = FlowSheetStorage.Load(
                FlowSheetStorage.FindLatest(ProjectName, Version, nameof(A408), FlowName)
                    ?? throw new AssertFailedException($"Flow sheet capture for '{FlowName}' was not saved"));

            A224.PrintStepTable(result, sheet, FlowName);

            // Passing sites are kicked out at the pass Result row; failing sites carry on (A227)
            A227.AssertResultBinning(result, sheet, FlowName, ExpectedResult, CheckedTest, SitesExpectedAtCheckedTest);

            // A98 needs the sheet of every flow this one calls to simulate it
            string[] subFlows = A95.SaveCalledFlowSheets(_projectExplorer, AppManager.MainWindow, _assistant,
                ProjectName, Version, nameof(A408), sheet);

            // Datalog is consistent (A97) and matches the Result-aware flow simulation (A98)
            A97.ValidateCapture(ProjectName, Version, nameof(A408), FlowName);
            A98.CompareCapture(ProjectName, Version, nameof(A408), FlowName, subFlows);

            Console.WriteLine("----------------------------------------");
            Console.WriteLine("|   A408 PASS RESULT BINNING PASSED   |");
            Console.WriteLine("----------------------------------------");

            Cleanup();
        }
    }
}
