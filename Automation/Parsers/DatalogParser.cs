using Automation.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Automation.Parsers
{
    public class DatalogParser
    {
        public DatalogResult Parse(string text)
        {
            var result = new DatalogResult();

            string[] lines = text.Split(
                new[] { "\r\n", "\n" },
                StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                string rawLine = lines[i];
                string line = rawLine.Trim();

                if (line.StartsWith("Flow") && line.Contains(":"))
                {
                    result.FlowName = ExtractAfterColon(line);
                    continue;
                }

                if (line.StartsWith("Start"))
                {
                    DatalogStep step = ParseStep(lines, i, result);

                    if (Regex.IsMatch(line, @"TEST_LIMIT(#\d+)?\s*\["))
                    {
                        TestLimitResult limit = ParseTestLimit(lines, i);

                        if (limit != null)
                        {
                            step.TestLimit = limit;
                            result.TestLimits.Add(limit);
                        }
                    }

                    continue;
                }

                if (Regex.IsMatch(line, @"^DUT\b"))
                {
                    result.Summary.DUT.AddRange(ParseIntRow(rawLine));
                    continue;
                }

                if (Regex.IsMatch(line, @"^STATUS\b"))
                {
                    result.Summary.Status.AddRange(ParseStringRow(rawLine));
                    continue;
                }

                if (Regex.IsMatch(line, @"^HARDBIN\b"))
                {
                    result.Summary.HardBin.AddRange(ParseIntRow(rawLine));
                    continue;
                }

                if (Regex.IsMatch(line, @"^SOFTBIN\b"))
                {
                    result.Summary.SoftBin.AddRange(ParseIntRow(rawLine));
                    continue;
                }

                if (Regex.IsMatch(line, @"^BIN NAME\b"))
                {
                    result.Summary.BinName.AddRange(ParseStringRow(rawLine));
                    continue;
                }
            }

            return result;
        }

        private DatalogStep ParseStep(string[] lines, int startIndex, DatalogResult result)
        {
            string line = lines[startIndex].Trim();
            string description = ExtractAfterColon(line);

            string testTime = null;

            for (int j = startIndex + 1; j < lines.Length; j++)
            {
                string current = lines[j].Trim();

                if (current.StartsWith("Test Time"))
                {
                    testTime = ExtractAfterColon(current);
                    break;
                }

                if (current.StartsWith("Start"))
                    break;
            }

            var step = new DatalogStep
            {
                Description = description,
                TestTime = testTime
            };

            result.Steps.Add(step);

            return step;
        }

        private TestLimitResult ParseTestLimit(string[] lines, int startIndex)
        {
            string line = lines[startIndex].Trim();

            Match match = Regex.Match(line, @"TEST_LIMIT(?:#\d+)?\s*\[(.*?)\]");

            if (!match.Success)
                return null;

            var limit = new TestLimitResult();
            limit.Name = match.Groups[1].Value;

            int valueLine = startIndex + 1;

            while (valueLine < lines.Length)
            {
                string current = lines[valueLine].Trim();

                if (Regex.IsMatch(current, @"^\d"))
                    break;

                if (current.StartsWith("End") && current.Contains("TEST_LIMIT"))
                    return null;

                valueLine++;
            }

            if (valueLine >= lines.Length)
                return null;

            string[] tokens = Regex.Split(lines[valueLine].Trim(), @"\s+");

            if (tokens.Length < 4)
                return null;

            double value;

            if (double.TryParse(
                tokens[0],
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out value))
            {
                limit.LowLimit = value;
            }

            if (double.TryParse(
                tokens[1],
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out value))
            {
                limit.HighLimit = value;
            }

            limit.Unit = tokens[2];

            for (int s = 3; s < tokens.Length; s++)
            {
                if (double.TryParse(
                    tokens[s],
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out value))
                {
                    limit.SiteValues.Add(value);
                }
            }

            return limit;
        }

        private static string[] SplitFixedWidthRow(string rawLine)
        {
            string[] tokens = Regex.Split(rawLine.TrimEnd(), @"\s{2,}");

            var trimmed = new List<string>();
            foreach (var t in tokens)
            {
                if (!string.IsNullOrEmpty(t))
                    trimmed.Add(t.Trim());
            }

            return trimmed.ToArray();
        }

        private static List<int> ParseIntRow(string rawLine)
        {
            string[] tokens = SplitFixedWidthRow(rawLine);
            var values = new List<int>();

            for (int t = 1; t < tokens.Length; t++)
            {
                int value;
                if (int.TryParse(
                    tokens[t],
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out value))
                {
                    values.Add(value);
                }
            }

            return values;
        }

        private static List<string> ParseStringRow(string rawLine)
        {
            string[] tokens = SplitFixedWidthRow(rawLine);
            var values = new List<string>();

            for (int t = 1; t < tokens.Length; t++)
            {
                values.Add(tokens[t]);
            }

            return values;
        }

        private static string ExtractAfterColon(string line)
        {
            int idx = line.IndexOf(':');
            return idx >= 0 ? line.Substring(idx + 1).Trim() : line.Trim();
        }
    }
}