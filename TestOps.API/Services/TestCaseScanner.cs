using System.Text.RegularExpressions;

namespace TestOps.API.Services;

public class TestCaseInfo
{
    public string ClassName { get; set; } = "";
    public string MethodName { get; set; } = "";
}

public static class TestCaseScanner
{
    public static List<TestCaseInfo> Scan(string rootFolder)
    {
        var results = new List<TestCaseInfo>();

        if (!Directory.Exists(rootFolder))
            throw new Exception($"Folder not found: {rootFolder}");

        var files = Directory.GetFiles(
            rootFolder,
            "*.cs",
            SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var text = File.ReadAllText(file);

            var classMatch = Regex.Match(
                text,
                @"\[TestClass\][\s\S]*?class\s+(\w+)");

            if (!classMatch.Success)
                continue;

            string className = classMatch.Groups[1].Value;

            // Rule #1 & #2
            if (!className.StartsWith("tFJ_"))
                continue;

            var methodMatches = Regex.Matches(
                text,
                @"\[TestMethod\][\s\S]*?void\s+(\w+)\s*\(");

            // Rule #3
            if (methodMatches.Count == 0)
                continue;

            string firstMethod = methodMatches[0]
                .Groups[1]
                .Value
                .Replace("_", " ")
                .Trim();

            // Rule #4
            results.Add(new TestCaseInfo
            {
                ClassName = className,
                MethodName = firstMethod
            });
        }

        return results
            .OrderBy(x => x.ClassName)
            .ToList();
    }
}