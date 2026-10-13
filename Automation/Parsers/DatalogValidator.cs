using Automation.Models;
using System;
using System.Collections.Generic;

namespace Automation.Parsers
{
    public class DatalogValidator
    {
        public DatalogValidationResult Validate(DatalogResult result, Func<string, ExpectedBin> expectedBinLookup = null)
        {
            var validation = new DatalogValidationResult();

            if (result == null)
            {
                validation.Add(-1, null, "DatalogResult was null.");
                return validation;
            }

            int siteCount = result.Summary.Status.Count;

            if (siteCount == 0)
            {
                validation.Add(-1, null, "No per-site summary rows found (DUT/STATUS/HARDBIN/SOFTBIN/BIN NAME).");
                return validation;
            }

            CheckRowLength(validation, "DUT", result.Summary.DUT.Count, siteCount);
            CheckRowLength(validation, "HARDBIN", result.Summary.HardBin.Count, siteCount);
            CheckRowLength(validation, "SOFTBIN", result.Summary.SoftBin.Count, siteCount);
            CheckRowLength(validation, "BIN NAME", result.Summary.BinName.Count, siteCount);

            // No longer skips sites where DUT == -1 - verified against
            // a real capture that DUT==-1 in this app means "debug/
            // simulate mode", not "no data", and the bin/status logic
            // still ran for real and is worth validating.
            for (int site = 0; site < siteCount; site++)
            {
                string actualStatus = SafeGet(result.Summary.Status, site);

                ValidateSiteLimits(result, site, actualStatus, validation);
                ValidateSiteBin(result, site, actualStatus, expectedBinLookup, validation);
            }

            return validation;
        }

        private void ValidateSiteLimits(DatalogResult result, int site, string actualStatus, DatalogValidationResult validation)
        {
            var failingTests = new List<string>();

            foreach (var test in result.TestLimits)
            {
                if (site >= test.SiteValues.Count)
                {
                    validation.Add(site, test.Name, $"No measurement recorded for this site (only {test.SiteValues.Count} site values present).");
                    continue;
                }

                double value = test.SiteValues[site];
                bool withinLimits = value >= test.LowLimit && value <= test.HighLimit;

                // A failing measurement by itself is expected, normal
                // data (a flow can legitimately fail some sites) - it's
                // only reported as part of the STATUS-consistency
                // message below, not as its own issue.
                if (!withinLimits)
                    failingTests.Add(test.Name);
            }

            string expectedStatus = failingTests.Count > 0 ? "Fail" : "Pass";

            bool actualIsPassOrFail =
                string.Equals(actualStatus, "Pass", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(actualStatus, "Fail", StringComparison.OrdinalIgnoreCase);

            // STATUS can legitimately be something other than Pass/Fail
            // (e.g. "TestNoPF" in debug/simulate mode) without that
            // being a discrepancy - only compare when it's actually
            // reporting a final Pass/Fail verdict.
            if (actualIsPassOrFail &&
                !string.Equals(expectedStatus, actualStatus, StringComparison.OrdinalIgnoreCase))
            {
                string detail = failingTests.Count > 0 ? $"failing tests: {string.Join(", ", failingTests)}" : "all tests within limits";
                validation.Add(site, null, $"STATUS mismatch: datalog reports '{actualStatus}' but limits imply '{expectedStatus}' ({detail}).");
            }
        }

        private void ValidateSiteBin(DatalogResult result, int site, string actualStatus, Func<string, ExpectedBin> expectedBinLookup, DatalogValidationResult validation)
        {
            if (expectedBinLookup == null)
                return;

            var expected = expectedBinLookup(actualStatus);

            if (expected == null)
                return;

            int actualHardBin = SafeGet(result.Summary.HardBin, site);
            int actualSoftBin = SafeGet(result.Summary.SoftBin, site);

            if (actualHardBin != expected.HardBin)
                validation.Add(site, null, $"HARDBIN mismatch: expected {expected.HardBin} for status '{actualStatus}', got {actualHardBin}.");

            if (actualSoftBin != expected.SoftBin)
                validation.Add(site, null, $"SOFTBIN mismatch: expected {expected.SoftBin} for status '{actualStatus}', got {actualSoftBin}.");
        }

        private static void CheckRowLength(DatalogValidationResult validation, string rowName, int actualCount, int expectedCount)
        {
            if (actualCount != expectedCount)
                validation.Add(-1, null, $"{rowName} row has {actualCount} values, expected {expectedCount} (one per site).");
        }

        private static T SafeGet<T>(List<T> list, int index)
        {
            return index < list.Count ? list[index] : default;
        }
    }
}