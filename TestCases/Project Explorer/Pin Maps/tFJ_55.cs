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
    [TestTags("Project_Explorer", "Pin_Maps", "Smoke", "Positive", "Automated")]
    public class tFJ_55 : BaseTest
    {
        private UIAssistant _assistant;
        private PSPage _psPage;
        private ProjectExplorerPane _projectExplorer;
        private PinMapPage _pinMap;

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
        public void Valid_Pin_Name()
        {
            var assistant = new UIAssistant(AppManager.Automation);
            var pinSheet = new PinMapPage(AppManager.MainWindow, assistant);

            pinSheet.ConfigurePMRow(
                4,
                "",
                "ABC",
                "Gnd",
                "first");

            pinSheet.ConfigurePMRow(
                5,
                "",
                "DEF",
                "Gnd",
                "");

            pinSheet.ConfigurePMRow(
                6,
                "",
                "SHUTDOWN",
                "IO",
                "");

            pinSheet.ConfigurePMRow(
                7,
                "",
                "AVDD",
                "Power",
                "");

            pinSheet.ConfigurePMRow(
                8,
                "",
                "SDA",
                "IO",
                "I2C interface");

            pinSheet.ClickValidate();

            Assert.IsTrue(
                pinSheet.VerifyValidationSuccessLog("has successfully completed"));

            Debug.WriteLine("-------------------------------------------------");
            Debug.WriteLine("| Pin Name configuration validated successfully |");
            Debug.WriteLine("-------------------------------------------------");

            Cleanup();
        }
    }
}
