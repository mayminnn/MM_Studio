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
    public class tFJ_63 : BaseTest
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
        public void Reuse_Pin_Name_Value_for_Group()
        {
            var assistant = new UIAssistant(AppManager.Automation);
            var pinSheet = new PinMapPage(AppManager.MainWindow, assistant);

            pinSheet.ConfigurePMRow(
                9,
                "ABC",
                "SHUTDOWN",
                "",
                "");

            pinSheet.ClickValidate();

            Assert.IsTrue(
                pinSheet.VerifyValidationSuccessLog("PinGroup 'ABC' redefinition"));

            Debug.WriteLine("-----------------------------------");
            Debug.WriteLine("| tFJ63_ReusePinNameforGrp passed |");
            Debug.WriteLine("-----------------------------------");

            Cleanup();
        }

        [TestMethod]
        public void zCleanUpReusePinNameForGroup()
        {
            var assistant = new UIAssistant(AppManager.Automation);
            var pinSheet = new PinMapPage(AppManager.MainWindow, assistant);

            pinSheet.ConfigurePMRow(
                9,
                "HALF_PIN",
                "SHUTDOWN",
                "",
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
