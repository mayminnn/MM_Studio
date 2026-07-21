using Automation.Components;
using Automation.Core;
using Automation.Pages;
using Automation.Parsers;
using Automation.Models;
using FlaUI.Core.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using Test_Cases.Login;

namespace Test_Cases.Project_Explorer.Flows
{
    [TestClass]
    public class StartDebug : BaseTest
    {
        private UIAssistant _assistant;
        private PSPage _psPage;
        private ProjectExplorerPane _projectExplorer;
        private Flow _flowPage;
        private DatalogClient _dc;
        private DatalogParser dp;

        [TestInitialize]
        public void Start()
        {
            _assistant = new UIAssistant(AppManager.Automation);

            var mainWindow = AppManager.MainWindow;

            _psPage = new PSPage(mainWindow, _assistant);
            _psPage.OpenDesignPage();

            var ribbon = new RibbonPane(mainWindow, _assistant);
            ribbon.ClickRibbonButton("Open Project");

            var openProjectWindow =
                _assistant.FindByAutomationId(mainWindow, "OpenProjectForm");

            var openDialog =
                new OpenProjectDialog(openProjectWindow, _assistant);

            openDialog.OpenProject(ProjectName, Version);

            _projectExplorer =
                new ProjectExplorerPane(mainWindow, _assistant);

            _projectExplorer.OpenPage("Flows");

            _flowPage = new Flow(mainWindow, _assistant);
            _dc = new DatalogClient(mainWindow, _assistant);
        }

        [TestMethod]
        public void RunDebug()
        {
            _flowPage.ClickValidate();
            _flowPage.ClickStartDebug();
            Thread.Sleep(1000);
            string text = _dc.GetDCResults();

            var parser = new DatalogParser();

            DatalogResult result = parser.Parse(text);

            System.Diagnostics.Debug.WriteLine(
            $"Total Test Limits = {result.TestLimits.Count}");

            foreach (var test in result.TestLimits)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Found Test = {test.Name}");
            }

            System.Diagnostics.Debug.WriteLine(
                $"Total Test Limits = {result.TestLimits.Count}");

            foreach (var test in result.TestLimits)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Found Test = [{test.Name}]");
            }

            TestLimitResult limit =
                result.GetTest("ATSPEED_SCAN3_125ns");

            if (limit == null)
            {
                throw new Exception("ATSPEED_SCAN3_125ns was not found.");
            }

            System.Diagnostics.Debug.WriteLine("========== Parsed Limit ==========");
            System.Diagnostics.Debug.WriteLine($"Name : {limit.Name}");
            System.Diagnostics.Debug.WriteLine($"Low  : {limit.LowLimit}");
            System.Diagnostics.Debug.WriteLine($"High : {limit.HighLimit}");
            System.Diagnostics.Debug.WriteLine($"Unit : {limit.Unit}");

            for (int i = 0; i < limit.SiteValues.Count; i++)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Site {i} = {limit.SiteValues[i]}");
            }

            //Cleanup();
        }
    }
}
