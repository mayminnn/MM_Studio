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
    [TestTags("Project_Explorer", "Pin_Maps", "Positive", "Automated")]
    public class tFJ_213 : BaseTest
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
        public void Defined_Pin_Group_as_Pin_Under_Another_Group()
        {
            var assistant = new UIAssistant(AppManager.Automation);
            var pinSheet = new PinMapPage(AppManager.MainWindow, assistant);

            pinSheet.ConfigurePMRow(
                12,
                "FULL_PIN",
                "HALF_PIN",
                "",
                "");

            pinSheet.ClickValidate();

            Assert.IsTrue(
                pinSheet.VerifyValidationSuccessLog("has successfully completed"));

            Debug.WriteLine("-------------------=====---------");
            Debug.WriteLine("| tFJ213_DefinedGrpAsPin passed |");
            Debug.WriteLine("---------------------------------");

            Cleanup();
        }
    }
}
