using Automation.Core;
using Automation.Components;
using Automation.Pages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test_Cases.Attributes;
using System.Diagnostics;
using Test_Cases.Login;

namespace Test_Cases.Project_Explorer.Bins
{
    [TestClass]
    [TestTags("Project_Explorer", "Bins", "Negative", "Automated")]
    [TestLinkIds("tFJ-103")]
    public class A103 : BaseTest
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
        public void Non_Numeric_SW_Bin()
        {
            var assistant = new UIAssistant(AppManager.Automation);
            var binSheet = new BinSheetPage(AppManager.MainWindow, assistant, TestSettings.BinSheetName);

            binSheet.ConfigureBinRow(
                8,
                "DEC",
                "Power",
                "88",
                "Undefined",
                "");

            binSheet.ClickValidate();

            Assert.IsTrue(
                binSheet.VerifyValidationSuccessLog("Input string was not in a correct format."));

            Console.WriteLine("-----------------------------------");
            Console.WriteLine("| Non-numeric SW Bin check passed |");
            Console.WriteLine("-----------------------------------");

            Cleanup();
        }

        [TestMethod]
        public void zCleanup_NonNumericSW()
        {
            var assistant = new UIAssistant(AppManager.Automation);
            var binSheet = new BinSheetPage(AppManager.MainWindow, assistant, TestSettings.BinSheetName);

            binSheet.ConfigureBinRow(
                8,
                "346",
                "Power",
                "88",
                "Undefined",
                "");

            binSheet.ClickValidate();

            Assert.IsTrue(
                binSheet.VerifyValidationSuccessLog("has successfully completed"));

            Console.WriteLine("---------------");
            Console.WriteLine("| Error reset |");
            Console.WriteLine("---------------");

            Cleanup();
        }
    }
}

