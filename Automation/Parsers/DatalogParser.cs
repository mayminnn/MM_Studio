using Automation.Models;
using System;
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
                string line = lines[i].Trim();

                // Only parse Start TEST_LIMIT block
                if (!line.StartsWith("Start"))
                    continue;

                if (!line.Contains("TEST_LIMIT ["))
                    continue;

                Match match = Regex.Match(
                    line,
                    @"TEST_LIMIT\s+\[(.*?)\]");

                if (!match.Success)
                    continue;

                var limit = new TestLimitResult();
                limit.Name = match.Groups[1].Value;

                // Find the measurement row
                int valueLine = i + 1;

                while (valueLine < lines.Length)
                {
                    string current = lines[valueLine].Trim();

                    // Measurement row always starts with LowLimit
                    if (Regex.IsMatch(current, @"^\d"))
                        break;

                    valueLine++;
                }

                if (valueLine >= lines.Length)
                    continue;

                string[] tokens = Regex.Split(
                    lines[valueLine].Trim(),
                    @"\s+");

                if (tokens.Length < 4)
                    continue;

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

                result.TestLimits.Add(limit);
            }

            return result;
        }
    }
}