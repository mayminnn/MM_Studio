using System;
using System.Collections.Generic;
using System.Linq;

namespace Automation.Models
{
    public class FlowSheetData
    {
        public string FlowName { get; set; } = "";

        public List<FlowSheetRow> Rows { get; set; } = new List<FlowSheetRow>();

        public FlowSheetRow GetTestLimitRow(string testName)
        {
            return Rows.FirstOrDefault(r =>
                string.Equals(r.Command, "test-limit", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(r.TestName, testName, StringComparison.OrdinalIgnoreCase));
        }

        public List<FlowSheetRow> GetTestLimitRows(string testName)
        {
            return Rows.Where(r =>
                string.Equals(r.Command, "test-limit", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(r.TestName, testName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public string GetExpectedFailBinName(string testName)
        {
            return GetTestLimitRow(testName)?.FailBin;
        }
    }
}