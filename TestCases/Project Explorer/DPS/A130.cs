using Automation.Components;
using Automation.Core;
using Automation.Pages;
using FlaUI.Core.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Test_Cases.Attributes;
using Test_Cases.Login;

namespace Test_Cases.Project_Explorer.DPS
{
    [TestClass]
    [TestTags("Project_Explorer", "DPS", "Positive", "Automated")]
    [TestLinkIds("tFJ-130")]
    public class A130 : BaseTest
    {
        private const string DPSSheetName = "DPS";
        private const string FlowSheetName = "Testing";
        private const int BypassRow = 4;

        private UIAssistant _assistant = null!;
        private PSPage _psPage = null!;
        private ProjectExplorerPane _projectExplorer = null!;
        private DPSPage _dpsPage = null!;
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

            _dpsPage = new DPSPage(mainWindow, _assistant, DPSSheetName);
            _flowPage = new Flow(mainWindow, _assistant, FlowSheetName);
            _dc = new DatalogClient(mainWindow, _assistant);
        }

        [TestMethod]
        public void Bypassed_Pin_Is_Skipped_During_Run()
        {
            // Steps 1-2: open DPS sheet "DPS" and set Bypass = TRUE on row 4
            _projectExplorer.OpenPage("DPSSets", DPSSheetName);

            string bypassedPin = _dpsPage.GetCellValue($"E{BypassRow}");

            Assert.IsFalse(string.IsNullOrEmpty(bypassedPin),
                $"No pin name in E{BypassRow} of DPS sheet '{DPSSheetName}'");

            _dpsPage.EnterDropdownValue($"F{BypassRow}", "TRUE");

            Assert.AreEqual("TRUE", _dpsPage.GetCellValue($"F{BypassRow}"),
                $"Bypass was not set to TRUE for {bypassedPin} (F{BypassRow})");

            // Validate the DPS sheet so the flow picks up the bypass change
            _dpsPage.ClickValidate();

            bool dpsValidated = Retry.WhileFalse(
                () => _dpsPage.VerifyValidationSuccessLog("has successfully completed"),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromMilliseconds(500)).Result;

            Assert.IsTrue(dpsValidated, $"DPS sheet '{DPSSheetName}' validation did not complete successfully");

            // Steps 3-4: expand Flows and open flow sheet "Testing", which already uses the DPS sheet
            _projectExplorer.OpenPage("Flows", FlowSheetName);

            // Step 5: validate
            _flowPage.ClickValidate();

            bool validated = Retry.WhileFalse(
                () => _flowPage.VerifyValidationSuccessLog("has successfully completed"),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromMilliseconds(500)).Result;

            Assert.IsTrue(validated, $"Flow sheet '{FlowSheetName}' validation did not complete successfully");

            // Step 6: run and wait for the whole datalog
            _flowPage.ClickStartDebug();

            string datalog = "";

            bool finished = Retry.WhileFalse(() =>
                {
                    datalog = _dc.GetDCResults();
                    return datalog.Contains("End Flow");
                },
                TimeSpan.FromSeconds(60),
                TimeSpan.FromSeconds(2)).Result;

            Assert.IsTrue(finished, "Run did not finish: datalog has no 'End Flow'");

            Console.WriteLine("========== DATALOG ==========");
            Console.WriteLine(datalog);
            Console.WriteLine("=============================");

            // Step 7: the bypassed pin is skipped, so it does not show up in the datalog
            Assert.IsFalse(
                Regex.IsMatch(datalog, $@"\b{Regex.Escape(bypassedPin)}\b"),
                $"Bypassed pin {bypassedPin} still appears in the datalog:\n{datalog}");

            Console.WriteLine("-----------------------------------");
            Console.WriteLine("| A130 BYPASSED PIN SKIPPED |");
            Console.WriteLine("-----------------------------------");

            Cleanup();
        }
    }
}
