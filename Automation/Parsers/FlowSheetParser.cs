using Automation.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Automation.Parsers
{
    public class FlowSheetParser
    {
        public FlowSheetData Parse(string tsvText)
        {
            var data = new FlowSheetData();

            if (string.IsNullOrWhiteSpace(tsvText))
                return data;

            string[] lines = tsvText.Split(
                new[] { "\r\n", "\n" },
                StringSplitOptions.None);

            foreach (var line in lines)
            {
                string[] cells = line.Split('\t');
                if (cells.Length >= 2 &&
                    string.Equals(cells[0].Trim(), "Flow", StringComparison.OrdinalIgnoreCase))
                {
                    data.FlowName = cells[1].Trim();
                    break;
                }
            }

            int headerIndex = -1;
            Dictionary<string, int> columnIndex = null;

            for (int i = 0; i < lines.Length; i++)
            {
                string[] cells = lines[i].Split('\t').Select(c => c.Trim()).ToArray();

                bool looksLikeHeader =
                    cells.Contains("Command") && cells.Contains("Value");

                if (looksLikeHeader)
                {
                    headerIndex = i;
                    columnIndex = BuildColumnIndex(cells);
                    break;
                }
            }

            if (headerIndex == -1 || columnIndex == null)
                return data;

            for (int i = headerIndex + 1; i < lines.Length; i++)
            {
                string[] cells = lines[i].Split('\t');

                if (cells.All(c => string.IsNullOrWhiteSpace(c)))
                {
                    if (data.Rows.Count > 0)
                        break;

                    continue;
                }

                data.Rows.Add(ParseRow(cells, columnIndex));
            }

            return data;
        }

        private static Dictionary<string, int> BuildColumnIndex(string[] headerCells)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < headerCells.Length; i++)
            {
                string header = headerCells[i];

                if (string.IsNullOrWhiteSpace(header))
                    continue;

                if (!map.ContainsKey(header))
                    map[header] = i;
            }

            return map;
        }

        private static FlowSheetRow ParseRow(string[] cells, Dictionary<string, int> columnIndex)
        {
            var row = new FlowSheetRow
            {
                Label = Get(cells, columnIndex, "Label"),
                Enable = Get(cells, columnIndex, "Enable"),
                Command = Get(cells, columnIndex, "Command"),
                Parameter = Get(cells, columnIndex, "Parameter"),
                Property = Get(cells, columnIndex, "Property"),
                Value = Get(cells, columnIndex, "Value"),
                TestName = Get(cells, columnIndex, "Test Name"),
                TestNum = Get(cells, columnIndex, "Test Num"),
                PassPath = Get(cells, columnIndex, "Pass Path"),
                FailPath = Get(cells, columnIndex, "Fail Path"),
                PassBin = Get(cells, columnIndex, "Pass Bin"),
                FailBin = Get(cells, columnIndex, "Fail Bin"),
                Result = Get(cells, columnIndex, "Result"),
                Assume = Get(cells, columnIndex, "Assume"),
                Sites = Get(cells, columnIndex, "Sites"),
                Remarks = Get(cells, columnIndex, "Remarks"),
            };

            row.LimitLow = GetDouble(cells, columnIndex, "LimitL");
            row.LimitHigh = GetDouble(cells, columnIndex, "LimitH");

            return row;
        }

        private static string Get(string[] cells, Dictionary<string, int> columnIndex, string header)
        {
            if (!columnIndex.TryGetValue(header, out int idx))
                return "";

            if (idx < 0 || idx >= cells.Length)
                return "";

            return cells[idx].Trim();
        }

        private static double? GetDouble(string[] cells, Dictionary<string, int> columnIndex, string header)
        {
            string text = Get(cells, columnIndex, header);

            if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                return value;

            return null;
        }
    }
}