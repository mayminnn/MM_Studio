using Automation.Core;
using Automation.Models;
using Automation.Parsers;
using Test_Cases.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Test_Cases.Project_Explorer.Flows
{
    [TestClass]
    [TestTags("Project_Explorer", "Flows", "Positive", "Automated", "1.0")]
    [TestLinkIds("tFJ-222", "tFJ-223", "tFJ-224", "tFJ-225", "tFJ-226")]
    public class A98
    {
        private static string ProjectName => TestSettings.ProjectName;
        private static string Version => TestSettings.Version;
        private const string TestName = "A95";
        private const string FlowName = "Default";

        [TestMethod]
        public void Compare_Latest_Datalog_To_FlowSheet()
        {
            string datalogPath = DatalogStorage.FindLatest(ProjectName, Version, TestName, FlowName);
            Assert.IsNotNull(datalogPath, $"No saved datalog capture found for {ProjectName}/{Version}/{TestName}/{FlowName}. Run A95 first.");

            string sheetPath = FlowSheetStorage.FindLatest(ProjectName, Version, TestName, FlowName);
            Assert.IsNotNull(sheetPath, $"No saved flow sheet capture found for {ProjectName}/{Version}/{TestName}/{FlowName}. Run A95 first.");

            Console.WriteLine($"Datalog capture : {datalogPath}");
            Console.WriteLine($"Flow sheet      : {sheetPath}");
            Console.WriteLine("");

            DatalogResult datalog = DatalogStorage.Load(datalogPath);
            FlowSheetData sheet = FlowSheetStorage.Load(sheetPath);

            var comparer = new FlowSheetComparer();
            DatalogValidationResult comparison = comparer.Compare(datalog, sheet);

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