using Automation.Models;
using System;
using System.Collections.Generic;

namespace Automation.Parsers
{
    public static class FlowSheetToJazzConverter
    {
        public static JazzProgram Convert(FlowSheetData sheet)
        {
            var program = new JazzProgram { Name = sheet.FlowName };

            foreach (var row in sheet.Rows)
                program.Steps.Add(ConvertRow(row));

            return program;
        }

        public static JazzStepDef ConvertRow(FlowSheetRow row)
        {
            string command = row.Command?.Trim() ?? "";
            string parameter;

            if (string.Equals(command, "test-limit", StringComparison.OrdinalIgnoreCase))
            {
                command = "test";
                parameter = row.TestName;
            }
            else if (string.Equals(command, "call", StringComparison.OrdinalIgnoreCase))
            {
                parameter = row.Parameter;
            }
            else
            {
                parameter = !string.IsNullOrEmpty(row.TestName) ? row.TestName : row.Parameter;
            }

            return new JazzStepDef
            {
                Label = row.Label,
                Command = command,
                Parameter = parameter,
                PassPath = row.PassPath,
                FailPath = row.FailPath,
                PassBin = row.PassBin,
                FailBin = row.FailBin,
                Result = row.Result
            };
        }

        public static Dictionary<string, JazzProgram> ConvertSubFlows(Dictionary<string, FlowSheetData> subFlowSheets)
        {
            var result = new Dictionary<string, JazzProgram>(StringComparer.OrdinalIgnoreCase);

            if (subFlowSheets == null)
                return result;

            foreach (var kv in subFlowSheets)
                result[kv.Key] = Convert(kv.Value);

            return result;
        }
    }
}