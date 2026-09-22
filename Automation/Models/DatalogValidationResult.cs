using System.Collections.Generic;

namespace Automation.Models
{
    public class DatalogIssue
    {
        public int Site { get; set; }

        public string TestName { get; set; }

        public string Description { get; set; }

        public override string ToString()
        {
            string prefix = TestName == null
                ? $"Site {Site}"
                : $"Site {Site} / {TestName}";

            return $"{prefix}: {Description}";
        }
    }

    public class DatalogValidationResult
    {
        public List<DatalogIssue> Issues { get; } = new List<DatalogIssue>();

        public bool IsValid => Issues.Count == 0;

        public void Add(int site, string testName, string description)
        {
            Issues.Add(new DatalogIssue
            {
                Site = site,
                TestName = testName,
                Description = description
            });
        }
    }
}