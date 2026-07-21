using Automation.Components;
using Automation.Core;
using Automation.Pages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Test_Cases.Attributes;
using System.Drawing;
using Test_Cases.Login;
using static System.Net.Mime.MediaTypeNames;

namespace Test_Cases.Project_Explorer.Global_Variables
{
    [TestClass]
    [TestTags("Project_Explorer", "Global_Variables", "Smoke", "Positive", "Automated")]
    public class tFJ_66 : BaseTest
    {
        private UIAssistant _assistant;
        private PSPage _psPage;
        private ProjectExplorerPane _projectExplorer;
        private ChannelMapPage _cmSheet;
        private ScreenCapture _screenCapture;

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
            _projectExplorer.OpenPage("Channel Maps");

            _cmSheet = new ChannelMapPage(mainWindow, _assistant);
        }

        [TestMethod]
        public void Reference_Global_Variable_in_Another_Sheets()
        {
            var assistant = new UIAssistant(AppManager.Automation);
            var cmSheet = new ChannelMapPage(AppManager.MainWindow, assistant);

            cmSheet.ConfigureCMRow(
                5,
                "ABC",
                "=_BuildVersion",
                "Gnd",
                "");

            cmSheet.ConfigureCMRow(
                6,
                "SDA",
                "=_Delay",
                "IO",
                "");

            //cmSheet.ClickValidate();

            _screenCapture = new ScreenCapture();
            _screenCapture.ElementCapture(cmSheet.GetCMRow(50));
            _screenCapture.ElementCapture(cmSheet.GetCMRow(5));
            _screenCapture.ElementCapture(cmSheet.GetCMRow(100));
            _screenCapture.ElementCapture(cmSheet.GetCMRow(6));

            //Assert.IsTrue(
            //    cmSheet.VerifyValidationSuccessLog("has successfully completed"));

            //Debug.WriteLine("--------------------------------------------");
            //Debug.WriteLine("| CM configuration validated successfully |");
            //Debug.WriteLine("--------------------------------------------");

            //Cleanup();
        }

        // [TestMethod]
        public void Verify_Screenshot_Text_Should_Contain_Value()
        {
            _cmSheet.ConfigureCMRow(5, "ABC", "=_BuildVersion", "Gnd", "");
            _cmSheet.ConfigureCMRow(6, "SDA", "=_TestVersion", "IO", "");

            _screenCapture = new ScreenCapture();

            string row5Path = _screenCapture.ElementCapture(_cmSheet.GetCMRow(5));
            //string row6Path = _screenCapture.ElementCapture(_cmSheet.GetCMRow(6));

            var ocr = new OCRhelper(@"C:\Aemulus\techFlowJazz\bin\Debug\tessdata");

            bool row5Valid = ocr.ContainsText(row5Path, "200");
            //bool row6Valid = ocr.ContainsText(row6Path, "2.2");

            Assert.IsTrue(row5Valid, "Row 5 does not contain '1'");
            //Assert.IsTrue(row6Valid, "Row 6 does not contain '2.2'");
        }
    }
}
