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
    [TestLinkIds("tFJ-78")]
    public class A78 : BaseTest
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
        public void Duplicate_Bin_Name()
        {
            var assistant = new UIAssistant(AppManager.Automation);
            var binSheet = new BinSheetPage(AppManager.MainWindow, assistant, TestSettings.BinSheetName);

            binSheet.ConfigureBinRow(
                8,
                "2950",
                "Aborted",
                "1888",
                "Fail",
                "");

            binSheet.ClickValidate();

            Assert.IsTrue(
                binSheet.VerifyValidationSuccessLog("Bin name 'Aborted' already exists in bin set 'BinAutomation'"));

            Console.WriteLine("-----------------------------------");
            Console.WriteLine("| Duplicate bin name check passed |");
            Console.WriteLine("-----------------------------------");

            Cleanup();
        }
    }
}
