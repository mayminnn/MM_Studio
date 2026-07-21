using Automation.Components;
using Automation.Core;
using Automation.Pages;
using FlaUI.Core.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using Test_Cases.Attributes;
using Test_Cases.Login;

namespace Test_Cases.Project_Explorer.DPS
{
    [TestClass]
    [TestTags("Project_Explorer", "DPS", "Positive", "Automated")]
    public class tFJ_311 : BaseTest
    {
        private UIAssistant _assistant;
        private PSPage _psPage;
        private ProjectExplorerPane _projectExplorer;
        private DPSPage _dpsPage;

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

            _projectExplorer.OpenPage("DPSSets");

            _dpsPage = new DPSPage(mainWindow, _assistant);
        }

        public void CongifureDPS(int row, string dpsset, string sequence, string pin,
            string mode, string fvalue, string clamp, string delay1, string limitl, string limith)
        {
            _dpsPage.ClearAndEnterCellValue($"B{row}", dpsset);
            _dpsPage.EnterDropdownValue($"C{row}", sequence);
            _dpsPage.EnterDropdownValue($"E{row}", pin);
            _dpsPage.EnterDropdownValue($"G{row}", mode);
            _dpsPage.ClearAndEnterCellValue($"H{row}", fvalue);
            _dpsPage.ClearAndEnterCellValue($"J{row}", clamp);
            _dpsPage.ClearAndEnterCellValue($"L{row}", delay1);
            _dpsPage.ClearAndEnterCellValue($"N{row}", limitl);
            _dpsPage.ClearAndEnterCellValue($"O{row}", limith);

            Wait.UntilInputIsProcessed();

            Thread.Sleep(500);
        }

        [TestMethod]
        public void Using_Different_Mode_for_Same_DPS_Set()
        {
            CongifureDPS(4, "Test", "Parallel", "AGND", "FVMI",
                "1", "0.5", "1", "0.1", "0.4");
            CongifureDPS(5, "", "", "AVDD", "FVMV", "1.4", "0.5",
                "1", "0.1", "0.4");

            _dpsPage.ClickValidate();

            Assert.IsTrue(
                _dpsPage.VerifyValidationSuccessLog("Every pin must be under the same mode"));

            Debug.WriteLine("-----------------------------");
            Debug.WriteLine("| tFJ_311 MODE CHECK PASSED |");
            Debug.WriteLine("-----------------------------");

            Cleanup();
        }
    }
}
