using Automation.Components;
using Automation.Core;
using Automation.Pages;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test_Cases.Attributes;
using System.Diagnostics;
using System.Linq;
using Test_Cases.Login;

namespace Test_Cases.Site
{
    [TestClass]
    [TestTags("Site", "Positive", "Automated")]
    public class tFJ_282 : BaseTest
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

        public void clickOK()
        {
            var okButton = Retry.WhileNull(() =>
            {
                return AppManager.MainWindow.FindAllDescendants()
                    .FirstOrDefault(e =>
                        e.Name == "OK")?.AsButton();
            }, TimeSpan.FromSeconds(5)).Result;

            Assert.IsNotNull(okButton, "OK button not found on popup");

            okButton.Focus();
            okButton.Click(); // close the dialog
            System.Diagnostics.Debug.WriteLine("Popup closed by clicking OK");
        }

        [TestMethod]
        public void Validate_Site_Count_Range_Limit()
        {
            var sitePane = new SitePane(AppManager.MainWindow, _assistant);
            int siteCount = 257;
            sitePane.ChangeSiteCount(siteCount);

            var errorText = Retry.WhileNull(() =>
            {
                return AppManager.MainWindow.FindAllDescendants()
                    .FirstOrDefault(e =>
                        e.Name != null &&
                        e.Name.Contains("techFlowJazz projects only support maximum 256 sites."));

            }, TimeSpan.FromSeconds(10)).Result;

            System.Diagnostics.Debug.WriteLine($"Error popup observed: '{errorText.Name}'");

            clickOK();

            int siteCount2 = 0;
            sitePane.ChangeSiteCount(siteCount2);

            var errorText2 = Retry.WhileNull(() =>
            {
                return AppManager.MainWindow.FindAllDescendants()
                    .FirstOrDefault(e =>
                        e.Name != null &&
                        e.Name.Contains("Minimum 1 site is required for techFlowJazz projects."));

            }, TimeSpan.FromSeconds(10)).Result;

            System.Diagnostics.Debug.WriteLine($"\nError popup observed: '{errorText2.Name}'");

            clickOK();

            Debug.WriteLine("------------------------------------------------------");
            Debug.WriteLine($"| Site Count Upper and Lower Range Limit Validated |");
            Debug.WriteLine("------------------------------------------------------");

            Cleanup();
        }
    }
}
