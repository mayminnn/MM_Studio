using Automation.Components;
using Automation.Core;
using Automation.Pages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test_Cases.Attributes;
using System.Diagnostics;
using Test_Cases.Login;

namespace Test_Cases.Channel_Maps
{
    [TestClass]
    [TestTags("Project_Explorer", "Channel_Maps", "Smoke", "Negative", "Automated")]
    public class tFJ_65 : BaseTest
    {

        private UIAssistant _assistant;
        private PSPage _psPage;

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
        }

        [TestMethod]
        public void Change_Number_of_Sites_with_Invalid_Input()
        {
            var sitePane = new SitePane(AppManager.MainWindow, _assistant);
            string invalidInput = "a";
            sitePane.ChangeSiteInvalid(invalidInput);

            Thread.Sleep(1000);

            Assert.IsTrue(
                sitePane.VerifyValidationSuccessLog($"Invalid value '{invalidInput}' was entered"));

            Debug.WriteLine("---------------------------------");
            Debug.WriteLine($"| tFJ_65 passed|");
            Debug.WriteLine("---------------------------------");

            // Cleanup();
        }
    }
}
