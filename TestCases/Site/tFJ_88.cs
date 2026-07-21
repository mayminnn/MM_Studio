using Automation.Components;
using Automation.Core;
using Automation.Pages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test_Cases.Attributes;
using System.Diagnostics;
using Test_Cases.Login;

namespace Test_Cases.Site
{
    [TestClass]
    [TestTags("Site", "Smoke", "Positive", "Automated")]
    public class tFJ_88 : BaseTest
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
        public void Change_Site_Count()
        {
            var sitePane = new SitePane(AppManager.MainWindow, _assistant);
            int siteCount = 3;
            sitePane.ChangeSiteCount(siteCount);
            sitePane.ValidateGallerySites(siteCount);

            Debug.WriteLine("---------------------------------");
            Debug.WriteLine($"| Site count changed to {siteCount}|");
            Debug.WriteLine("---------------------------------");

            Cleanup();
        }
    }
}
