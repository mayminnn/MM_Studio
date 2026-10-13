using Automation.Models;
using System;
using System.IO;
using System.Text.Json;

namespace Automation.Core
{
    public static class FlowSheetStorage
    {
        private static string RootFolder =>
            Environment.GetEnvironmentVariable("MM_STUDIO_DATALOG_DIR")
            ?? @"C:\Aemulus\DatalogCaptures";

        private static readonly JsonSerializerOptions JsonOptions =
            new JsonSerializerOptions { WriteIndented = true };

        public static string Save(string projectName, string version, string testName, FlowSheetData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            string flowName = string.IsNullOrWhiteSpace(data.FlowName) ? "UnknownFlow" : Sanitize(data.FlowName);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            string folder = Path.Combine(RootFolder, Sanitize(projectName), Sanitize(version), Sanitize(testName));
            Directory.CreateDirectory(folder);

            string jsonPath = Path.Combine(folder, $"{flowName}_FlowSheet_{timestamp}.json");
            File.WriteAllText(jsonPath, JsonSerializer.Serialize(data, JsonOptions));

            Console.WriteLine($"Flow sheet capture saved: {jsonPath}");

            return jsonPath;
        }

        public static FlowSheetData Load(string jsonPath)
        {
            if (!File.Exists(jsonPath))
                throw new FileNotFoundException("Flow sheet capture not found.", jsonPath);

            var data = JsonSerializer.Deserialize<FlowSheetData>(File.ReadAllText(jsonPath), JsonOptions);

            if (data == null)
                throw new Exception($"Failed to deserialize flow sheet capture at '{jsonPath}'.");

            return data;
        }

        public static string FindLatest(string projectName, string version, string testName, string flowName)
        {
            string folder = Path.Combine(RootFolder, Sanitize(projectName), Sanitize(version), Sanitize(testName));

            if (!Directory.Exists(folder))
                return null;

            string pattern = $"{Sanitize(flowName)}_FlowSheet_*.json";

            string latest = null;
            DateTime latestTime = DateTime.MinValue;

            foreach (var file in Directory.GetFiles(folder, pattern))
            {
                var writeTime = File.GetLastWriteTime(file);
                if (writeTime > latestTime)
                {
                    latestTime = writeTime;
                    latest = file;
                }
            }

            return latest;
        }

        private static string Sanitize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Unknown";

            string result = value;
            foreach (char c in Path.GetInvalidFileNameChars())
                result = result.Replace(c, '_');

            return result.Replace(' ', '_');
        }
    }
}