using Automation.Models;
using System.IO;
using System.Text;

namespace Automation.Core
{
    public static class FlowSheetCsvWriter
    {
        private static readonly string[] Columns =
        {
            "Label", "Enable", "Command", "Parameter", "Property", "Value",
            "Test Name", "Test Num", "LimitL", "LimitH", "Pass Path",
            "Fail Path", "Pass Bin", "Fail Bin", "Result", "Assume",
            "Sites", "Remarks"
        };

        public static void Write(FlowSheetData data, string filePath)
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Join(",", Columns));

            foreach (var row in data.Rows)
            {
                sb.AppendLine(string.Join(",", new[]
                {
                    Escape(row.Label),
                    Escape(row.Enable),
                    Escape(row.Command),
                    Escape(row.Parameter),
                    Escape(row.Property),
                    Escape(row.Value),
                    Escape(row.TestName),
                    Escape(row.TestNum),
                    Escape(row.LimitLow?.ToString() ?? ""),
                    Escape(row.LimitHigh?.ToString() ?? ""),
                    Escape(row.PassPath),
                    Escape(row.FailPath),
                    Escape(row.PassBin),
                    Escape(row.FailBin),
                    Escape(row.Result),
                    Escape(row.Assume),
                    Escape(row.Sites),
                    Escape(row.Remarks),
                }));
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
                return "\"" + value.Replace("\"", "\"\"") + "\"";

            return value;
        }
    }
}