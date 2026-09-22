using Automation.Components;
using Automation.Core;
using Automation.Pages;
using Automation.Parsers;
using Automation.Models;
using FlaUI.Core.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Test_Cases.Attributes;
using System.Threading;
using Test_Cases.Login;

namespace Test_Cases.Project_Explorer.Flows
{
    [TestClass]
    [TestTags("Project_Explorer", "Flows", "Positive", "Automated", "1.0")]

    public class tFJ_95 : BaseTest
    {
        private UIAssistant _assistant;
        private PSPage _psPage;
        private ProjectExplorerPane _projectExplorer;
        private Flow _flowPage;
        private DatalogClient _dc;

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
        public void Execute_Flow()
        {
            Console.WriteLine("STEP 1 - Start");

            _flowPage.ClickValidate();
            Console.WriteLine("STEP 2 - Validate done");

            _flowPage.ClickStartDebug();
            Console.WriteLine("STEP 3 - Debug started");

            Thread.Sleep(5000);
            Console.WriteLine("STEP 4 - Wait done");

            DatalogResult result = _dc.CaptureAndSave(
                projectName: ProjectName,
                version: Version,
                testName: nameof(tFJ_95));

            Console.WriteLine("STEP 5 - Capture saved");
            Console.WriteLine($"Flow: {result.FlowName}");
            Console.WriteLine($"Total Test Limits: {result.TestLimits.Count}");
            Console.WriteLine($"Sites: {result.Summary.Status.Count}");
        }

        // [TestMethod]
        // public void Execute_Flow()
        // {
        //     Console.WriteLine("STEP 1 - Start");

        //     _flowPage.ClickValidate();
        //     Console.WriteLine("STEP 2 - Validate done");

        //     _flowPage.ClickStartDebug();
        //     Console.WriteLine("STEP 3 - Debug started");

        //     Thread.Sleep(5000);
        //     Console.WriteLine("STEP 4 - Wait done");

        //     string text = _dc.GetDCResults();
        //     Console.WriteLine("STEP 5 - GetDCResults done");

        //     Console.WriteLine("hi");
        //     Console.WriteLine($"TEXT LENGTH = {text?.Length ?? 0}");
        //     Console.WriteLine(text);
        // }
    }
}
