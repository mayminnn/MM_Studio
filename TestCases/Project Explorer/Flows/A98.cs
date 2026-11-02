using Automation.Core;
using Automation.Models;
using Automation.Parsers;
using Test_Cases.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Test_Cases.Project_Explorer.Flows
{
    [TestClass]
    [TestTags("Project_Explorer", "Flows", "Positive", "Automated", "1.0")]
    [TestLinkIds("tFJ-222", "tFJ-223", "tFJ-226")]
    public class A98
    {
        private static string ProjectName => TestSettings.ProjectName;
        private static string Version => TestSettings.Version;
        private const string TestName = "A95";
        private const string FlowName = "Default";

        [TestMethod]
        public void Compare_Latest_Datalog_To_FlowSheet()
        {
            CompareCapture(ProjectName, Version, TestName, FlowName);
        }

        // Simulates the saved flow sheet and checks the saved datalog agrees with it.
        // subFlowNames: flows reached through "call", whose sheets were saved under the same testName
        public static void CompareCapture(string projectName, string version, string testName, string flowName,
            params string[] subFlowNames)
        {
            var subFlowSheets = new Dictionary<string, FlowSheetData>(StringComparer.OrdinalIgnoreCase);

            foreach (string subFlow in subFlowNames)
            {
                string? subPath = FlowSheetStorage.FindLatest(projectName, version, testName, subFlow);
                Assert.IsNotNull(subPath, $"No saved flow sheet capture found for sub-flow {projectName}/{version}/{testName}/{subFlow}.");

                subFlowSheets[subFlow] = FlowSheetStorage.Load(subPath);
            }

            string? datalogPath = DatalogStorage.FindLatest(projectName, version, testName, flowName);
            Assert.IsNotNull(datalogPath, $"No saved datalog capture found for {projectName}/{version}/{testName}/{flowName}. Run A95 first.");

            string? sheetPath = FlowSheetStorage.FindLatest(projectName, version, testName, flowName);
            Assert.IsNotNull(sheetPath, $"No saved flow sheet capture found for {projectName}/{version}/{testName}/{flowName}. Run A95 first.");

            Console.WriteLine($"Datalog capture : {datalogPath}");
            Console.WriteLine($"Flow sheet      : {sheetPath}");
            Console.WriteLine("");

            DatalogResult datalog = DatalogStorage.Load(datalogPath);
            FlowSheetData sheet = FlowSheetStorage.Load(sheetPath);

            var comparer = new FlowSheetComparer();
            DatalogValidationResult comparison = comparer.Compare(datalog, sheet, subFlowSheets);

            if (!comparison.IsValid)
            {
                Console.WriteLine($"========== {comparison.Issues.Count} ISSUE(S) FOUND ==========");
                foreach (var issue in comparison.Issues)
                    Console.WriteLine(issue.ToString());
            }
            else
            {
                Console.WriteLine("Datalog matches flow sheet configuration - no issues found.");
            }

            Assert.IsTrue(comparison.IsValid,
                $"Datalog vs flow sheet comparison failed with {comparison.Issues.Count} issue(s):\n" +
                string.Join("\n", comparison.Issues.Select(i => i.ToString())));
        }
    }
}