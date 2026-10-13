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
    
    public class A97
    {
        private static string ProjectName => TestSettings.ProjectName;
        private static string Version => TestSettings.Version;
        private const string TestName = "A95";
        private const string FlowName = "Default";

        [TestMethod]
        public void Validate_Latest_Capture()
        {
            string path = DatalogStorage.FindLatest(ProjectName, Version, TestName, FlowName);

            Assert.IsNotNull(path, $"No saved capture found for {ProjectName}/{Version}/{TestName}/{FlowName}. Run A95 first.");

            Console.WriteLine($"Validating capture: {path}");

            DatalogResult result = DatalogStorage.Load(path);

            Console.WriteLine($"Flow: {result.FlowName}");
            Console.WriteLine($"Test Limits: {result.TestLimits.Count}");
            Console.WriteLine($"Sites: {result.Summary.Status.Count}");

            var validator = new DatalogValidator();
            DatalogValidationResult validation = validator.Validate(result);

            if (!validation.IsValid)
            {
                Console.WriteLine("");
                Console.WriteLine($"========== {validation.Issues.Count} ISSUE(S) FOUND ==========");
                foreach (var issue in validation.Issues)
                    Console.WriteLine(issue.ToString());
            }
            else
            {
                Console.WriteLine("No issues found - all in-limit / STATUS values are consistent.");
            }

            Assert.IsTrue(validation.IsValid,
                $"Datalog validation failed with {validation.Issues.Count} issue(s):\n" +
                string.Join("\n", validation.Issues.Select(i => i.ToString())));
        }
    }
}