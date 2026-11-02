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
    [TestTags("Project_Explorer", "Bins", "Smoke", "Positive", "Automated")]
    [TestLinkIds("tFJ-76")]
    public class A76 : BaseTest
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
        public void Bin_Configuration()
        {
            var assistant = new UIAssistant(AppManager.Automation);
            var binSheet = new BinSheetPage(AppManager.MainWindow, assistant, TestSettings.BinSheetName);

            binSheet.ConfigureBinRow(
                7,
                "1",
                "PASS_BIN",
                "11",
                "Pass",
                "Auto test");

            binSheet.ClickValidate();

            Assert.IsTrue(
                binSheet.VerifyValidationSuccessLog("has successfully completed"));

            Console.WriteLine("--------------------------------------------");
            Console.WriteLine("| Bin configuration validated successfully |");
            Console.WriteLine("--------------------------------------------");

            Cleanup();
        }
    }
}
