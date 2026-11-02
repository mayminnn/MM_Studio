using Automation.Core;
using Automation.Components;
using Automation.Pages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test_Cases.Attributes;
using System.Diagnostics;
using Test_Cases.Login;

namespace Test_Cases.Project_Explorer.Pin_Maps
{
    [TestClass]
    [TestTags("Project_Explorer", "Pin_Maps", "Negative", "Automated")]
    [TestLinkIds("tFJ-58")]
    public class A58 : BaseTest
    {
        private UIAssistant _assistant = null!;
        private PSPage _psPage = null!;
        private ProjectExplorerPane _projectExplorer = null!;
        private PinMapPage _pinMap = null!;
        //private ScreenCapture _screenCapture;

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
            _projectExplorer.OpenPage("Pin Maps", TestSettings.PinMapSheetName);

            _pinMap = new PinMapPage(mainWindow, _assistant, TestSettings.PinMapSheetName);
        }

        [TestMethod]
        public void Defining_Pin_Name_without_Type()
        {
            var assistant = new UIAssistant(AppManager.Automation);
            var pinSheet = new PinMapPage(AppManager.MainWindow, assistant, TestSettings.PinMapSheetName);

            pinSheet.ConfigurePMRow(
                6,
                "",
                "SHUTDOWN",
                "",
                "");

            pinSheet.ClickValidate();

            Assert.IsTrue(
                pinSheet.VerifyValidationSuccessLog("Type unspecified for PinName "));

            Console.WriteLine("-------------------------------");
            Console.WriteLine("| tFJ58_DefPinNameNoType passed |");
            Console.WriteLine("-------------------------------");

            pinSheet.ConfigurePMRow(
                6,
                "",
                "SHUTDOWN",
                "IO",
                "");

            pinSheet.ClickValidate();

            Assert.IsTrue(
                pinSheet.VerifyValidationSuccessLog("has successfully completed"));

            Console.WriteLine("---------------");
            Console.WriteLine("| Error reset |");
            Console.WriteLine("---------------");


            Cleanup();
        }

        // [TestMethod]
        // public void zCleanUpPinName_noType_()
        // {
        //     var assistant = new UIAssistant(AppManager.Automation);
        //     var pinSheet = new PinMapPage(AppManager.MainWindow, assistant, TestSettings.PinMapSheetName);

        //     pinSheet.ConfigurePMRow(
        //         6,
        //         "",
        //         "SHUTDOWN",
        //         "IO",
        //         "");

        //     pinSheet.ClickValidate();

        //     Assert.IsTrue(
        //         pinSheet.VerifyValidationSuccessLog("has successfully completed"));

        //     Console.WriteLine("---------------");
        //     Console.WriteLine("| Error reset |");
        //     Console.WriteLine("---------------");

        //     Cleanup();
        // }
    }
}
