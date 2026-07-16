using Automation.Core;
using Automation.Components;
using Automation.Pages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test_Cases.Attributes;
using System.Diagnostics;
using Test_Cases.Login;

namespace Test_Cases.Project_Explorer.Global_Variables
{
    [TestClass]
    [TestTags("Project_Explorer", "Global_Variables", "Smoke", "Positive", "Automated")]
    public class tFJ_68 : BaseTest
    {
        private UIAssistant _assistant;
        private PSPage _psPage;
        private ProjectExplorerPane _projectExplorer;
        private GlobalVariablePage _GVpage;
        //private ChannelMapPage _channelMapPage;

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
            _projectExplorer.OpenPage("Global Variables");

            _GVpage = new GlobalVariablePage(mainWindow, _assistant);
            //_channelMapPage = new ChannelMapPage(mainWindow, _assistant);
        }

        [TestMethod]
        public void Configure_Global_Variables()
        {
            var assistant = new UIAssistant(AppManager.Automation);
            var globalSheet = new GlobalVariablePage(AppManager.MainWindow, assistant);
            //var channelSheet = new ChannelMapPage(AppManager.MainWindow, assistant);

            globalSheet.ConfigureGVRow(
                4,
                "BuildVersion",
                "1.0");

            //globalSheet.ClickValidate();

            globalSheet.ConfigureGVRow(
                5,
                "Delay",
                "200*ms");

            globalSheet.ConfigureGVRow(
                6,
                "BootVoltage",
                "3.4");

            globalSheet.ClickValidate();

            Assert.IsTrue(
                globalSheet.VerifyValidationSuccessLog("has successfully completed"));

            Debug.WriteLine("--------------------------------------------------------");
            Debug.WriteLine("| Global Variable configuration validated successfully |");
            Debug.WriteLine("--------------------------------------------------------");

            Cleanup();
        }
    }
}
