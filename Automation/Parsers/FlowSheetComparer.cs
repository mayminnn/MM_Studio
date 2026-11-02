using Automation.Models;
using System;
using System.Collections.Generic;

namespace Automation.Parsers
{
    public class FlowSheetComparer
    {
        private const double LimitTolerance = 0.0001;

        public DatalogValidationResult Compare(DatalogResult datalog, FlowSheetData sheet, Dictionary<string, FlowSheetData>? subFlowSheets = null)
        {
            var result = new DatalogValidationResult();

            if (datalog == null)
            {
                result.Add(-1, null, "DatalogResult was null.");
                return result;
            }

            if (sheet == null)
            {
                result.Add(-1, null, "FlowSheetData was null.");
                return result;
            }

            var occurrenceCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var test in datalog.TestLimits)
            {
                // Check each limit against the sheet of the flow it ran in, so a sub-flow
                // reusing a test name isn't compared with the main flow's row
                string? subFlow = GetSubFlowName(test.FlowPath);
                FlowSheetData owningSheet = sheet;

                if (subFlow != null)
                {
                    if (subFlowSheets == null || !subFlowSheets.TryGetValue(subFlow, out FlowSheetData? subSheet) || subSheet == null)
                    {
                        result.Add(-1, test.Name, $"Ran in sub-flow '{subFlow}', but that flow's sheet was not provided for comparison.");
                        continue;
                    }

                    owningSheet = subSheet;
                }

                var matchingRows = owningSheet.GetTestLimitRows(test.Name);

                string key = $"{subFlow}|{test.Name}";
                int occurrence = occurrenceCount.TryGetValue(key, out int c) ? c : 0;
                occurrenceCount[key] = occurrence + 1;

                if (occurrence >= matchingRows.Count)
                {
                    result.Add(-1, test.Name, $"No matching row in flow sheet '{subFlow ?? sheet.FlowName}' (occurrence #{occurrence + 1} of this test name).");
                    continue;
                }

                CompareLimits(test, matchingRows[occurrence], result);
            }

            CompareBinsPerSiteUsingSimulation(datalog, sheet, subFlowSheets, result);

            return result;
        }

        private void CompareLimits(TestLimitResult test, FlowSheetRow row, DatalogValidationResult result)
        {
            if (row.LimitLow.HasValue && !NearlyEqual(row.LimitLow.Value, test.LowLimit))
                result.Add(-1, test.Name, $"LowLimit mismatch: datalog used {test.LowLimit}, sheet specifies {row.LimitLow.Value}.");

            if (row.LimitHigh.HasValue && !NearlyEqual(row.LimitHigh.Value, test.HighLimit))
                result.Add(-1, test.Name, $"HighLimit mismatch: datalog used {test.HighLimit}, sheet specifies {row.LimitHigh.Value}.");
        }

        // No longer skips sites where DUT == -1: whether the flow's
        // bin/result logic behaved correctly is independent of whether
        // a physical device was present - DUT==-1 in this app appears
        // to just mean "simulate/debug mode", not "no data to check".
        // Verified against a real capture: predictions matched actual
        // BinName on all 8 sites, including cases where an earlier
        // failing test's bin was correctly overwritten by a later one.
        private void CompareBinsPerSiteUsingSimulation(DatalogResult datalog, FlowSheetData sheet, Dictionary<string, FlowSheetData>? subFlowSheets, DatalogValidationResult result)
        {
            JazzProgram program = FlowSheetToJazzConverter.Convert(sheet);
            Dictionary<string, JazzProgram> subPrograms = FlowSheetToJazzConverter.ConvertSubFlows(subFlowSheets);
            var simulator = new JazzFlowSimulator(subPrograms);

            int siteCount = datalog.Summary.Status.Count;

            for (int site = 0; site < siteCount; site++)
            {
                Dictionary<string, List<bool>> outcomes = BuildOutcomesForSite(datalog, site);

                JazzSiteResult simulated;
                try
                {
                    simulated = simulator.Run(program, outcomes);
                }
                catch (Exception ex)
                {
                    result.Add(site, null, $"Simulation could not run: {ex.Message}");
                    continue;
                }

                string? actualStatus = site < datalog.Summary.Status.Count ? datalog.Summary.Status[site] : null;
                string? actualBinName = site < datalog.Summary.BinName.Count ? datalog.Summary.BinName[site] : null;

                if (!string.IsNullOrEmpty(simulated.Result) &&
                    !string.Equals(simulated.Result, actualStatus, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(site, null, $"Simulated Result '{simulated.Result}' does not match datalog STATUS '{actualStatus}'.");
                }

                if (simulated.Bin is { Length: > 0 } simulatedBin)
                {
                    bool matches = actualBinName is { Length: > 0 } &&
                        (simulatedBin.StartsWith(actualBinName, StringComparison.OrdinalIgnoreCase) ||
                         actualBinName.StartsWith(simulatedBin, StringComparison.OrdinalIgnoreCase));

                    if (!matches)
                        result.Add(site, null, $"Simulated Bin '{simulated.Bin}' does not match datalog BinName '{actualBinName}'.");
                }
            }
        }

        private static Dictionary<string, List<bool>> BuildOutcomesForSite(DatalogResult datalog, int site)
        {
            var outcomes = new Dictionary<string, List<bool>>(StringComparer.OrdinalIgnoreCase);

            foreach (var test in datalog.TestLimits)
            {
                if (site >= test.SiteValues.Count)
                    continue;

                double value = test.SiteValues[site];

                // The site never reached this test, so it has no outcome to feed the simulation
                if (double.IsNaN(value))
                    continue;

                bool passed = value >= test.LowLimit && value <= test.HighLimit;

                if (!outcomes.TryGetValue(test.Name, out List<bool> list))
                {
                    list = new List<bool>();
                    outcomes[test.Name] = list;
                }

                list.Add(passed);
            }

            return outcomes;
        }

        // "Default>ATSPEED_SCAN" -> "ATSPEED_SCAN"; null for the top-level flow (or an older capture without a path)
        private static string? GetSubFlowName(string flowPath)
        {
            if (string.IsNullOrEmpty(flowPath) || !flowPath.Contains(">"))
                return null;

            return flowPath.Substring(flowPath.LastIndexOf('>') + 1).Trim();
        }

        private static bool NearlyEqual(double a, double b)
        {
            return Math.Abs(a - b) <= LimitTolerance;
        }
    }
}