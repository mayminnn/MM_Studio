using FlaUI.Core.AutomationElements;
using System;
using System.Linq;

namespace Automation.Core
{
    public static class SheetLocator
    {
        // Finds an open sheet document such as "DPS=Default" or "Flow=Testing" anywhere in
        // the document area. Searching the whole area rather than one fixed bar keeps working
        // when several sheets are open at once
        public static AutomationElement FindDocument(AutomationElement window, UIAssistant assistant, string documentName)
        {
            var pnlxPage = assistant.FindByAutomationId(window, "pnlxPage").OrThrow("pnlxPage");
            var designUC = assistant.FindByAutomationId(pnlxPage, "projectDesignUserControl").OrThrow("projectDesignUserControl");
            var dockSite = assistant.FindByAutomationId(designUC, "mainDocDockSite").OrThrow("mainDocDockSite");

            var document = assistant.FindByName(dockSite, documentName);

            if (document != null)
                return document;

            // Name the sheets that are open, so a wrong or missing name is obvious from the failure
            string openSheets = string.Join(", ", dockSite.FindAllDescendants()
                .Select(x => x.Name)
                .Where(x => !string.IsNullOrEmpty(x) && x.Contains("="))
                .Distinct());

            throw new InvalidOperationException(
                $"Sheet '{documentName}' is not open. Open sheets: [{openSheets}]");
        }
    }
}
