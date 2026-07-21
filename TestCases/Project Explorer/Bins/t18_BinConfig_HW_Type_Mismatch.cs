using Automation.Core;
using Automation.Components;
using Automation.Pages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using Test_Cases.Login;

namespace Test_Cases.Project_Explorer.Bins
{
    //[TestClass]
    public class t18_BinConfig_HW_Type_Mismatch : BaseTest
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

        //[TestMethod]
        public void HW_Type_Mismatch_Should_Fail()
        {
            var assistant = new UIAssistant(AppManager.Automation);
            var binSheet = new BinSheetPage(AppManager.MainWindow, assistant);
            binSheet.ConfigureBinRow(
                8,
                "2001",
                "Power",
                "2997",
                "Fail",
                "");

            binSheet.ClickValidate();

            Assert.IsTrue(
                binSheet.VerifyValidationSuccessLog("Bin type does not match with previous set Hard Bin type"));

            Debug.WriteLine("--------------------------------------------");
            Debug.WriteLine("| HW bin type check validated successfully |");
            Debug.WriteLine("--------------------------------------------");

            Cleanup();
        }
    }
}
