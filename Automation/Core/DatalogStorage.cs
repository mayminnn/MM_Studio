using Automation.Models;
using System;
using System.IO;
using System.Text.Json;

namespace Automation.Core
{
    public static class DatalogStorage
    {
        private static string RootFolder =>
    Environment.GetEnvironmentVariable("MM_STUDIO_DATALOG_DIR")
    ?? @"C:\Aemulus\techFlowJazz\DatalogCaptures";

        private static readonly JsonSerializerOptions JsonOptions =
            new JsonSerializerOptions
            {
                WriteIndented = true
            };

        public static string Save(
            string projectName,
            string version,
            string testName,
            DatalogResult result,
            string rawText)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            string flowName = string.IsNullOrWhiteSpace(result.FlowName)
                ? "UnknownFlow"
                : Sanitize(result.FlowName);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            string folder = Path.Combine(
                RootFolder,
                Sanitize(projectName),
                Sanitize(version),
                Sanitize(testName));

            Directory.CreateDirectory(folder);

            string baseName = $"{flowName}_{timestamp}";

            string rawPath = Path.Combine(folder, baseName + ".raw.txt");
            File.WriteAllText(rawPath, rawText ?? string.Empty);

            string jsonPath = Path.Combine(folder, baseName + ".json");
            string json = JsonSerializer.Serialize(result, JsonOptions);
            File.WriteAllText(jsonPath, json);

            Console.WriteLine($"Datalog capture saved: {jsonPath}");

            return jsonPath;
        }

        public static DatalogResult Load(string jsonPath)
        {
            if (!File.Exists(jsonPath))
                throw new FileNotFoundException("Datalog capture not found.", jsonPath);

            string json = File.ReadAllText(jsonPath);

            var result = JsonSerializer.Deserialize<DatalogResult>(json, JsonOptions);

            if (result == null)
                throw new Exception($"Failed to deserialize datalog capture at '{jsonPath}'.");

            return result;
        }

        public static string FindLatest(
            string projectName,
            string version,
            string testName,
            string flowName)
        {
            string folder = Path.Combine(
                RootFolder,
                Sanitize(projectName),
                Sanitize(version),
                Sanitize(testName));

            if (!Directory.Exists(folder))
                return null;

            string pattern = Sanitize(flowName) + "_*.json";

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