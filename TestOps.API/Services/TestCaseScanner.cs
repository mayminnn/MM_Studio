using System.Text.RegularExpressions;
using TestOps.API.Models;

namespace TestOps.API.Services;

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
            
            List<string> tags = ReadAttributeStrings(text, "TestTags");

            List<string> testLinkIds = ReadAttributeStrings(text, "TestLinkIds");

            // Rule #1 & #2: only classes that declare [TestLinkIds] are listed
            if (testLinkIds.Count == 0)
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
                MethodName = firstMethod,
                Tags = tags,
                TestLinkIds = testLinkIds
            });
        }

        return results
            .OrderBy(x => x.ClassName)
            .ToList();
    }

    // Returns the quoted string arguments of [attributeName(...)], or an empty list
    private static List<string> ReadAttributeStrings(string text, string attributeName)
    {
        var attribute = Regex.Match(
            text,
            $@"\[{attributeName}\b[^\]]*\]",
            RegexOptions.Singleline);

        if (!attribute.Success)
            return new List<string>();

        return Regex.Matches(
                    attribute.Value,
                    "\"([^\"]+)\"")
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .ToList();
    }
}