using Automation.Components;
using Automation.Core;
using Automation.Pages;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Test_Cases.Attributes;
using Test_Cases.Login;

namespace Test_Cases.Project_Explorer.Channel_Maps
{
    [TestClass]
    [TestTags("Project_Explorer", "Channel_Maps", "Smoke", "Positive", "Automated")]
    public class tFJ_64 : BaseTest
    {
        private UIAssistant _assistant;
        private PSPage _psPage;
        private ProjectExplorerPane _projectExplorer;
        private ChannelMapPage _cmSheet;

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

        private string GetColumnLetter(int index)
        {
            string column = "";

            while (index >= 0)
            {
                column = (char)('A' + (index % 26)) + column;
                index = (index / 26) - 1;
            }

            return column;
        }

        [TestMethod]
        public void Sync_Site_on_Channel_Map()
        {
            Thread.Sleep(500);

            var sitePane = new SitePane(AppManager.MainWindow, _assistant);

            int siteCount = 30;

            sitePane.ChangeSiteCount(siteCount);

            _cmSheet.ClickSync();

            int startColumnIndex = 'F' - 'A';
            int row = 4;

            int lastIndex = siteCount - 1;
            int lastColumnIndex = startColumnIndex + lastIndex;

            string lastColumn = GetColumnLetter(lastColumnIndex);
            string lastCell = $"{lastColumn}{row}";

            _cmSheet.SelectCellByFormula(lastCell);

            Thread.Sleep(500);

            string actualValue = _cmSheet.GetSelectedCellValueByCopy();

            Assert.AreEqual(
                lastIndex.ToString(),
                actualValue,
                $"Mismatch at {lastCell}. Expected {lastIndex}, got {actualValue}"
            );

            Debug.WriteLine($"Checking {lastCell} = {actualValue}");

            Debug.WriteLine("-----------------");
            Debug.WriteLine("| tFJ-64 PASSED |");
            Debug.WriteLine("-----------------");

            Cleanup();
        }
    }
}
