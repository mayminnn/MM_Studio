using Automation.Components;
using Automation.Core;
using Automation.Models;
using Automation.Pages;
using FlaUI.Core.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Test_Cases.Attributes;
using Test_Cases.Login;

namespace Test_Cases.Project_Explorer.Flows
{
    [TestClass]
    [TestTags("Project_Explorer", "Flows", "Positive", "Automated")]
    [TestLinkIds("tFJ-215", "tFJ-234", "tFJ-235", "tFJ-237")]
    public class A234 : BaseTest
    {
        // Fixture: this flow has a goto that jumps over this test
        private const string FlowName = "TestGOTO";
        private const string SkippedTest = "PATTERN_FAST";

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

            _projectExplorer.OpenPage("Flows", FlowName);

            _flowPage = new Flow(mainWindow, _assistant, FlowName);
            _dc = new DatalogClient(mainWindow, _assistant);
        }

        [TestMethod]
        public void Goto_Skips_To_Label()
        {
            // Validate
            _flowPage.ClickValidate();

            bool validated = Retry.WhileFalse(
                () => _flowPage.VerifyValidationSuccessLog("has successfully completed"),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromMilliseconds(500)).Result;

            Assert.IsTrue(validated, $"Flow '{FlowName}' validation did not complete successfully");

            // Start Debug, saving the flow sheet + datalog under this test's name (A95)
            DatalogResult result = A95.RunAndCapture(
                _flowPage, _dc, ProjectName, Version, nameof(A234), FlowName);

            A96.PrintCapture(ProjectName, Version, nameof(A234), FlowName);

            FlowSheetData sheet = FlowSheetStorage.Load(
                FlowSheetStorage.FindLatest(ProjectName, Version, nameof(A234), FlowName)
                    ?? throw new AssertFailedException($"Flow sheet capture for '{FlowName}' was not saved"));

            AssertFixtureSkipsTest(sheet);

            // The goto jumps over the test, so it must not run in this flow. Only steps of
            // this flow count ("TestGOTO:..."): a called sub-flow ("TestGOTO>SUB:...") may run
            // its own copy of the test, which the goto does not affect
            var skippedPattern = new Regex($@"\b{Regex.Escape(SkippedTest)}\b", RegexOptions.IgnoreCase);

            var mainFlowSteps = result.Steps
                .Select(x => x.Description)
                .Where(x => x.StartsWith($"{FlowName}:", StringComparison.OrdinalIgnoreCase))
                .ToList();

            Assert.IsTrue(mainFlowSteps.Any(x => x.Contains("GOTO")),
                $"No GOTO step ran in flow '{FlowName}'. Steps: [{string.Join("; ", mainFlowSteps)}]");

            var ranSkipped = mainFlowSteps.Where(x => skippedPattern.IsMatch(x)).ToList();

            Assert.IsFalse(ranSkipped.Any(),
                $"{SkippedTest} should have been skipped by goto but ran in flow '{FlowName}': " +
                $"[{string.Join("; ", ranSkipped)}]");

            // A98 needs the sheet of every flow this one calls to simulate it
            string[] subFlows = SaveCalledFlowSheets(sheet);

            // Datalog is consistent (A97) and matches the goto-aware flow simulation (A98)
            A97.ValidateCapture(ProjectName, Version, nameof(A234), FlowName);
            A98.CompareCapture(ProjectName, Version, nameof(A234), FlowName, subFlows);

            Console.WriteLine("-----------------------------------");
            Console.WriteLine($"| A234 {SkippedTest} SKIPPED BY GOTO |");
            Console.WriteLine("-----------------------------------");

            Cleanup();
        }

        // Fixture guard: the sheet must still have a goto whose jump passes over SkippedTest,
        // otherwise "SkippedTest did not run" would prove nothing
        private static void AssertFixtureSkipsTest(FlowSheetData sheet)
        {
            var rows = sheet.Rows;
            var gotos = new System.Collections.Generic.List<string>();

            for (int i = 0; i < rows.Count; i++)
            {
                if (!string.Equals(rows[i].Command, "goto", StringComparison.OrdinalIgnoreCase))
                    continue;

                string label = rows[i].Parameter;
                int target = rows.FindIndex(r => string.Equals(r.Label, label, StringComparison.OrdinalIgnoreCase));

                gotos.Add($"goto {label} -> {(target < 0 ? "label not found" : $"row #{target + 1}")}");

                if (target > i && rows.Skip(i + 1).Take(target - i - 1)
                        .Any(r => string.Equals(r.TestName, SkippedTest, StringComparison.OrdinalIgnoreCase)))
                    return;
            }

            Assert.Fail(
                $"Fixture changed: flow '{FlowName}' has no goto that jumps over '{SkippedTest}'. " +
                $"Gotos found: [{string.Join("; ", gotos)}]");
        }

        // Opens and saves each flow named in a "call" row, returning their names
        private string[] SaveCalledFlowSheets(FlowSheetData sheet)
        {
            string[] subFlows = sheet.Rows
                .Where(r => string.Equals(r.Command, "call", StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Parameter?.Trim() ?? "")
                .Where(x => x.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (string subFlow in subFlows)
            {
                _projectExplorer.OpenPage("Flows", subFlow);

                new Flow(AppManager.MainWindow, _assistant, subFlow)
                    .SaveFlow(ProjectName, Version, nameof(A234), subFlow);
            }

            return subFlows;
        }
    }
}
