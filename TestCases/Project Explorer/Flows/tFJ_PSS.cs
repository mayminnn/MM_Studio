using Automation.Core;
using Automation.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Test_Cases.Project_Explorer.Flows
{
    [TestClass]
    public class tFJ_PSS
    {
        // TODO: point these at whatever capture you want to inspect.
        private const string ProjectName = "OX01G10";
        private const string Version = "v1";
        private const string TestName = "tFJ_95";
        private const string FlowName = "Default";

        [TestMethod]
        public void Print_Latest_Capture_By_Site()
        {
            string path = DatalogStorage.FindLatest(
                ProjectName, Version, TestName, FlowName);

            Assert.IsNotNull(
                path,
                $"No saved capture found for {ProjectName}/{Version}/{TestName}/{FlowName}. " +
                "Run tFJ_95 first.");

            Console.WriteLine($"Loaded: {path}");

            DatalogResult result = DatalogStorage.Load(path);

            Console.WriteLine($"Flow: {result.FlowName}");
            Console.WriteLine("");

            if (result.Steps.Count > 0)
            {
                Console.WriteLine("Steps:");
                foreach (var step in result.Steps)
                {
                    Console.WriteLine($"  {step.Description} -> {step.TestTime ?? "(no time recorded)"}");

                    if (step.TestLimit != null)
                    {
                        var test = step.TestLimit;
                        Console.WriteLine($"    Name  : {test.Name}");
                        Console.WriteLine($"    Limits: [{test.LowLimit}, {test.HighLimit}] {test.Unit}");

                        for (int site = 0; site < test.SiteValues.Count; site++)
                        {
                            double value = test.SiteValues[site];
                            bool withinLimits = value >= test.LowLimit && value <= test.HighLimit;
                            string verdict = withinLimits ? "PASS" : "FAIL";

                            Console.WriteLine($"      Site {site}: value={value} {test.Unit} -> {verdict}");
                        }
                    }
                }
                Console.WriteLine("");
            }

            int siteCount = result.Summary.Status.Count;

            if (siteCount == 0)
            {
                Console.WriteLine("No per-site summary data found in this capture.");
                return;
            }

            for (int site = 0; site < siteCount; site++)
            {
                Console.WriteLine($"Site {site}:");
                Console.WriteLine($"  DUT     : {SafeGet(result.Summary.DUT, site)}");
                Console.WriteLine($"  Status  : {SafeGet(result.Summary.Status, site)}");
                Console.WriteLine($"  HardBin : {SafeGet(result.Summary.HardBin, site)}");
                Console.WriteLine($"  SoftBin : {SafeGet(result.Summary.SoftBin, site)}");
                Console.WriteLine($"  BinName : {SafeGet(result.Summary.BinName, site)}");

                if (result.TestLimits.Count > 0)
                {
                    Console.WriteLine($"  Tests:");

                    foreach (var test in result.TestLimits)
                    {
                        if (site >= test.SiteValues.Count)
                        {
                            Console.WriteLine($"    {test.Name}: no value recorded for this site");
                            continue;
                        }

                        double value = test.SiteValues[site];
                        bool withinLimits = value >= test.LowLimit && value <= test.HighLimit;
                        string verdict = withinLimits ? "PASS" : "FAIL";

                        Console.WriteLine(
                            $"    {test.Name}: value={value} {test.Unit} " +
                            $"limits=[{test.LowLimit}, {test.HighLimit}] -> {verdict}");
                    }
                }

                Console.WriteLine("");
            }
        }

        private static string SafeGet<T>(List<T> list, int index)
        {
            return index < list.Count ? list[index]?.ToString() : "(missing)";
        }
    }
}