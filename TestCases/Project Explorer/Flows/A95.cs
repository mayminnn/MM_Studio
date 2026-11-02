using Automation.Components;
using Automation.Core;
using Automation.Pages;
using Automation.Parsers;
using Automation.Models;
using FlaUI.Core.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Linq;
using Test_Cases.Attributes;
using System.Threading;
using Test_Cases.Login;

namespace Test_Cases.Project_Explorer.Flows
{
    [TestClass]
    [TestTags("Project_Explorer", "Flows", "Positive", "Automated", "1.0")]
    [TestLinkIds("tFJ-95")]

    public class A95 : BaseTest
    {
        private UIAssistant _assistant = null!;
        private PSPage _psPage = null!;
        private ProjectExplorerPane _projectExplorer = null!;
        private Flow _flowPage = null!;
        private DatalogClient _dc = null!;

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
            RunAndCapture(_flowPage, _dc, ProjectName, Version, nameof(A95));
        }

        // Validates and runs the open flow, saving the flow sheet and datalog under
        // testName so A96-A98 can check them. Other tests call this for their own captures
        public static DatalogResult RunAndCapture(Flow flowPage, DatalogClient dc,
            string projectName, string version, string testName, string flowName = "Default")
        {
            Console.WriteLine("STEP 1 - Start");

            flowPage.ClickValidate();
            Console.WriteLine("STEP 2 - Validate done");

            flowPage.SaveFlow(projectName, version, testName, flowName);

            flowPage.ClickStartDebug();
            Console.WriteLine("STEP 3 - Debug started");

            Thread.Sleep(5000);
            Console.WriteLine("STEP 4 - Wait done");

            DatalogResult result = dc.CaptureAndSave(
                projectName: projectName,
                version: version,
                testName: testName);

            Console.WriteLine("STEP 5 - Capture saved");
            Console.WriteLine($"Flow: {result.FlowName}");
            Console.WriteLine($"Total Test Limits: {result.TestLimits.Count}");
            Console.WriteLine($"Sites: {result.Summary.Status.Count}");

            return result;
        }

        // Opens and saves the sheet of each flow named in a "call" row of the saved flow sheet,
        // so A98 can simulate the calls. Returns the sub-flow names to pass to A98
        public static string[] SaveCalledFlowSheets(ProjectExplorerPane projectExplorer,
            FlaUI.Core.AutomationElements.AutomationElement window, UIAssistant assistant,
            string projectName, string version, string testName, FlowSheetData sheet)
        {
            string[] subFlows = sheet.Rows
                .Where(r => string.Equals(r.Command, "call", System.StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Parameter?.Trim() ?? "")
                .Where(x => x.Length > 0)
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (string subFlow in subFlows)
            {
                projectExplorer.OpenPage("Flows", subFlow);

                new Flow(window, assistant, subFlow)
                    .SaveFlow(projectName, version, testName, subFlow);
            }

            return subFlows;
        }
    }
}
