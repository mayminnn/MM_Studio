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
    public class tFJ_58 : BaseTest
    {
        private UIAssistant _assistant;
        private PSPage _psPage;
        private ProjectExplorerPane _projectExplorer;
        private PinMapPage _pinMap;
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
            _projectExplorer.OpenPage("Pin Maps");

            _pinMap = new PinMapPage(mainWindow, _assistant);
        }

        [TestMethod]
        public void Defining_Pin_Name_without_Type()
        {
            var assistant = new UIAssistant(AppManager.Automation);
            var pinSheet = new PinMapPage(AppManager.MainWindow, assistant);

            pinSheet.ConfigurePMRow(
                6,
                "",
                "SHUTDOWN",
                "",
                "");

            pinSheet.ClickValidate();

            Assert.IsTrue(
                pinSheet.VerifyValidationSuccessLog("Type unspecified for PinName "));

            Debug.WriteLine("-------------------------------");
            Debug.WriteLine("| tFJ58_DefPinNameNoType passed |");
            Debug.WriteLine("-------------------------------");

            Cleanup();
        }

        [TestMethod]
        public void zCleanUpPinName_noType_()
        {
            var assistant = new UIAssistant(AppManager.Automation);
            var pinSheet = new PinMapPage(AppManager.MainWindow, assistant);

            pinSheet.ConfigurePMRow(
                6,
                "",
                "SHUTDOWN",
                "IO",
                "");

            pinSheet.ClickValidate();

            Assert.IsTrue(
                pinSheet.VerifyValidationSuccessLog("has successfully completed"));

            Debug.WriteLine("---------------");
            Debug.WriteLine("| Error reset |");
            Debug.WriteLine("---------------");

            Cleanup();
        }
    }
}
