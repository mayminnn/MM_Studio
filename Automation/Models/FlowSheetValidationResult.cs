using System.Collections.Generic;
using System.Linq;

namespace Automation.Models
{
    public enum FlowValidationSeverity
    {
        Error,
        Warning
    }

    public class FlowValidationIssue
    {
        public int RowIndex { get; set; }
        public FlowValidationSeverity Severity { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            string prefix = RowIndex >= 0 ? $"Row {RowIndex + 1}" : "Sheet";
            return $"[{Severity}] {prefix}: {Message}";
        }
    }

    public class FlowSheetValidationResult
    {
        public List<FlowValidationIssue> Issues { get; } = new List<FlowValidationIssue>();
        public bool HasErrors => Issues.Any(i => i.Severity == FlowValidationSeverity.Error);
        public bool IsValid => !HasErrors;

        public void AddError(int rowIndex, string message)
        {
            Issues.Add(new FlowValidationIssue { RowIndex = rowIndex, Severity = FlowValidationSeverity.Error, Message = message });
        }

        public void AddWarning(int rowIndex, string message)
        {
            Issues.Add(new FlowValidationIssue { RowIndex = rowIndex, Severity = FlowValidationSeverity.Warning, Message = message });
        }
    }
}