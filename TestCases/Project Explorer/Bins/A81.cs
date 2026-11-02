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
    [TestLinkIds("tFJ-81")]
    public class A81 : BaseTest
    {
        private UIAssistant _assistant = null!;
        private PSPage _psPage = null!;
        private ProjectExplorerPane _projectExplorer = null!;
        private BinSheetPage _binSheet = null!;

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
            _projectExplorer.OpenPage("Bins", TestSettings.BinSheetName);

            _binSheet = new BinSheetPage(mainWindow, _assistant, TestSettings.BinSheetName);
        }

        [TestMethod]
        public void Blank_HW_Bin()
        {
            var assistant = new UIAssistant(AppManager.Automation);
            var binSheet = new BinSheetPage(AppManager.MainWindow, assistant, TestSettings.BinSheetName);

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

            Console.WriteLine("-------------------------------------");
            Console.WriteLine("| tFJ81_BinConfig_BlankHWBin passed |");
            Console.WriteLine("-------------------------------------");


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

            //Console.WriteLine("-------------------------------");
            //Console.WriteLine("| 0 value HW Bin check passed |");
            //Console.WriteLine("-------------------------------");

            Cleanup();
        }
    }
}

