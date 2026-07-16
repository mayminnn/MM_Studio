using Automation.Core;
using Automation.Components;
using Automation.Pages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test_Cases.Attributes;
using System;
using System.Diagnostics;
using Test_Cases.Login;

namespace Test_Cases.Project_Explorer.Bins
{
    [TestClass]
    [TestTags("Project_Explorer", "Bins", "Negative", "Automated")]
    public class tFJ_81 : BaseTest
    {
        private UIAssistant _assistant;
        private PSPage _psPage;
        private ProjectExplorerPane _projectExplorer;
        private BinSheetPage _binSheet;

        [TestInitialize]
        public void Start()
        {
            _assistant = new UIAssistant(AppManager.Automation);
            var mainWindow = AppManager.MainWindow;

            _psPage = new PSPage(mainWindow, _assistant);
            _psPage.OpenDesignPage();

            var ribbon = new RibbonPane(mainWindow, _assistant);
            ribbon.ClickRibbonButton("Open Project");

            var openProjectWindow = _assistant.FindByAutomationId(mainWindow, "OpenProjectForm");
            var openDialog = new OpenProjectDialog(openProjectWindow, _assistant);
            openDialog.OpenProject(ProjectName, Version);

            _projectExplorer = new ProjectExplorerPane(mainWindow, _assistant);
            _projectExplorer.OpenPage("Bins");

            _binSheet = new BinSheetPage(mainWindow, _assistant);
        }

        [TestMethod]
        public void Blank_HW_Bin()
        {
            var assistant = new UIAssistant(AppManager.Automation);
            var binSheet = new BinSheetPage(AppManager.MainWindow, assistant);

            binSheet.ConfigureBinRow(
                8,
                "1234",
                "Power",
                "",
                "Undefined",
                "");

            binSheet.ClickValidate();

            Assert.IsTrue(
                binSheet.VerifyValidationSuccessLog("Field value of HW Bin cannot be empty"));

            Debug.WriteLine("-------------------------------------");
            Debug.WriteLine("| tFJ81_BinConfig_BlankHWBin passed |");
            Debug.WriteLine("-------------------------------------");


            //binSheet.ConfigureBinRow(
            //                8,
            //                "2345",
            //                "Power",
            //                "0",
            //                "Non-Functional",
            //                "");

            //binSheet.ClickValidate();

            //Assert.IsTrue(
            //    binSheet.VerifyValidationSuccessLog("Softbin or hardware bin number cannot use 0"));

            //Debug.WriteLine("-------------------------------");
            //Debug.WriteLine("| 0 value HW Bin check passed |");
            //Debug.WriteLine("-------------------------------");

            Cleanup();
        }
    }
}

