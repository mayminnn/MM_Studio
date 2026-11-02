using Automation.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Automation.Components
{
    public class ProjectExplorerPane
    {
        private readonly AutomationElement _window;
        private readonly UIAssistant _assistant;

        public ProjectExplorerPane(AutomationElement window, UIAssistant assistant)
        {
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }

            if (assistant == null)
            {
                throw new ArgumentNullException("assistant");
            }

            _window = window;
            _assistant = assistant;
        }

        public void OpenPage(string pageName, string sheetName = "Default")
        {
            var tree = FindTree();

            AutomationElement topNode = FindTopNode(tree, pageName, TimeSpan.FromSeconds(100));

            System.Diagnostics.Debug.WriteLine($"Found {pageName} menu");
            System.Diagnostics.Debug.WriteLine($"Details: [ {topNode} ]\n");

            var log = new List<string>();

            Expand(topNode, log);

            // The tree reports a node's children from before it was opened (or none at all),
            // so read the node again once it is open
            var sheetNode = Retry.WhileNull(() =>
                FindChildSheet(FindTopNode(tree, pageName, TimeSpan.FromSeconds(2)), sheetName),
                TimeSpan.FromSeconds(5)).Result;

            topNode = FindTopNode(tree, pageName, TimeSpan.FromSeconds(2));

            int childIndex = sheetNode == null
                ? -1
                : Rows(topNode).FindIndex(x => SameName(x.Name, sheetNode.Name));

            if (sheetNode == null || childIndex < 0)
            {
                throw new InvalidOperationException(
                    $"Sheet '{sheetName}' under '{pageName}' not found. " +
                    $"Steps: [{string.Join("; ", log)}]. " +
                    $"Children of '{pageName}': [{DescribeChildren(topNode)}]");
            }

            SelectRowByKeyboard(tree, pageName, childIndex, log);
            OpenSelectedRow(sheetNode, log);

            System.Diagnostics.Debug.WriteLine($"Opened {pageName} > {sheetName}: [{string.Join("; ", log)}]");
        }

        // Moves the keyboard to the sheet without scrolling or reading positions: Home goes to the
        // first row of the tree, then Down once per row until the sheet. The row count comes from
        // the tree's structure (items above the node, the children of expanded ones, the node,
        // then the sheet's place among its children), so it starts from the top on every call
        private static void SelectRowByKeyboard(AutomationElement tree, string pageName, int childIndex, List<string> log)
        {
            int rowsAbove = 0;

            if (!CountRowsAbove(tree, pageName, ref rowsAbove))
                throw new InvalidOperationException($"'{pageName}' not found while counting the tree's rows");

            int presses = rowsAbove + 1 + childIndex;

            tree.Focus();
            Wait.UntilInputIsProcessed();

            Keyboard.Press(VirtualKeyShort.HOME);
            Wait.UntilInputIsProcessed();
            Thread.Sleep(100);

            for (int i = 0; i < presses; i++)
            {
                Keyboard.Press(VirtualKeyShort.DOWN);
                Wait.UntilInputIsProcessed();
                Thread.Sleep(30);
            }

            Thread.Sleep(200);

            log.Add($"Home, then Down x{presses} ({rowsAbove} rows above '{pageName}', child #{childIndex + 1}); " +
                    $"focus reported on '{tree.Automation.FocusedElement()?.Name}'");
        }

        // Rows shown above the named node: walks the tree top to bottom, going into expanded items only
        private static bool CountRowsAbove(AutomationElement parent, string nodeName, ref int rows)
        {
            foreach (var item in Rows(parent))
            {
                if (SameName(item.Name, nodeName))
                    return true;

                rows++;

                if (IsExpanded(item) && CountRowsAbove(item, nodeName, ref rows))
                    return true;
            }

            return false;
        }

        // A node's rows, without scroll bars or other unnamed parts of the tree control
        private static List<AutomationElement> Rows(AutomationElement parent)
        {
            return parent.FindAllChildren()
                .Where(x => x.ControlType != ControlType.ScrollBar && !string.IsNullOrWhiteSpace(x.Name))
                .ToList();
        }

        // Opens the selected row and checks its document tab appeared. Enter first (keyboard),
        // then the node's MSAA default action, then a double-click as a last resort
        private void OpenSelectedRow(AutomationElement sheetNode, List<string> log)
        {
            string sheetName = sheetNode.Name?.Trim() ?? "";
            var before = OpenDocumentNames();
            bool alreadyOpen = before.Any(x => IsDocumentFor(x, sheetName));

            var attempts = new (string Name, Action Run)[]
            {
                ("Enter", () => Keyboard.Press(VirtualKeyShort.ENTER)),
                ("MSAA default action", () => sheetNode.Patterns.LegacyIAccessible.Pattern.DoDefaultAction()),
                ("double-click", () => Mouse.DoubleClick(sheetNode.BoundingRectangle.Center())),
            };

            foreach (var attempt in attempts)
            {
                try
                {
                    attempt.Run();
                }
                catch (Exception ex) when (ex is COMException || ex is InvalidOperationException || ex is NotSupportedException)
                {
                    log.Add($"{attempt.Name} failed: {ex.Message}");
                    continue;
                }

                Wait.UntilInputIsProcessed();
                Thread.Sleep(300);

                // Can't tell a re-activated tab from an untouched one, so trust the first action
                if (alreadyOpen)
                {
                    log.Add($"'{sheetName}' was already open; used {attempt.Name}");
                    return;
                }

                bool opened = Retry.WhileFalse(() =>
                        OpenDocumentNames().Any(x => IsDocumentFor(x, sheetName) && !before.Contains(x)),
                    TimeSpan.FromSeconds(3)).Result;

                log.Add($"{attempt.Name}: {(opened ? "opened" : "did not open")}");

                if (opened)
                    return;
            }

            throw new InvalidOperationException(
                $"Sheet '{sheetName}' was selected but did not open. Steps: [{string.Join("; ", log)}]. " +
                $"Open documents: [{string.Join(", ", OpenDocumentNames())}]");
        }

        // Document tabs are named "<type>=<sheet>", e.g. "Flow=TestGOTO"
        private static bool IsDocumentFor(string documentName, string sheetName)
        {
            return documentName.EndsWith("=" + sheetName, StringComparison.OrdinalIgnoreCase);
        }

        // Names of the open documents. Only looks a few levels down and not inside a document,
        // whose sheet grid can hold thousands of elements
        private HashSet<string> OpenDocumentNames()
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var pnlxPage = _assistant.FindByAutomationId(_window, "pnlxPage", 2);
            var designUC = _assistant.FindByAutomationId(pnlxPage, "projectDesignUserControl", 2);
            var dockSite = _assistant.FindByAutomationId(designUC, "mainDocDockSite", 2);

            if (dockSite == null)
                return names;

            var level = new List<AutomationElement> { dockSite };

            for (int depth = 0; depth < 4 && level.Count > 0; depth++)
            {
                var next = new List<AutomationElement>();

                foreach (var element in level)
                {
                    foreach (var child in element.FindAllChildren())
                    {
                        string name = child.Name ?? "";

                        if (name.Contains("="))
                            names.Add(name);
                        else
                            next.Add(child);
                    }
                }

                level = next;
            }

            return names;
        }

        private static bool SameName(string? a, string? b)
        {
            return string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        // Right-click a top-level node (e.g. DPSSets) and pick an item from its context menu
        public void ClickContextMenuItem(string nodeName, string menuItemName)
        {
            var tree = FindTree();
            var node = FindTopNode(tree, nodeName, TimeSpan.FromSeconds(100));

            Mouse.RightClick(node.BoundingRectangle.Center());
            Wait.UntilInputIsProcessed();

            var menu = Retry.WhileNull(FindOpenDropDown, TimeSpan.FromSeconds(5)).Result
                .OrThrow($"Context menu (DropDown) for '{nodeName}'");

            var item = _assistant.FindByName(menu, menuItemName, 5)
                .OrThrow($"Context menu item '{menuItemName}'");

            // Click rather than Invoke: Invoke blocks when the item opens a modal dialog
            item.Click();
            Wait.UntilInputIsProcessed();

            System.Diagnostics.Debug.WriteLine($"Clicked '{menuItemName}' on '{nodeName}'");
        }

        // Right-click the node, choose New Sheet, then fill in and confirm the New Sheet dialog
        public void CreateNewSheet(string nodeName, string sheetName)
        {
            SubmitNewSheet(nodeName, sheetName);

            bool closed = Retry.WhileFalse(() => FindNewSheetDialog() == null,
                TimeSpan.FromSeconds(10)).Result;

            if (!closed)
                throw new InvalidOperationException(
                    $"New Sheet dialog did not close after creating '{sheetName}' under '{nodeName}'");

            System.Diagnostics.Debug.WriteLine($"Created sheet '{sheetName}' under '{nodeName}'");
        }

        // Fills in and confirms the New Sheet dialog without waiting for it to close,
        // for cases where the app answers with an error instead
        public void SubmitNewSheet(string nodeName, string sheetName)
        {
            ClickContextMenuItem(nodeName, "New Sheet");

            var dialog = Retry.WhileNull(FindNewSheetDialog, TimeSpan.FromSeconds(10)).Result
                .OrThrow("New Sheet dialog (NewSheetForm)");

            var nameBox = _assistant.FindByAutomationId(dialog, "textBoxSheetName")
                .OrThrow("textBoxSheetName");

            nameBox.AsTextBox().Enter(sheetName);
            Wait.UntilInputIsProcessed();
            Thread.Sleep(200);

            _assistant.FindByAutomationId(dialog, "buttonOk")
                .OrThrow("buttonOk")
                .Click();

            Wait.UntilInputIsProcessed();
        }

        // Reads the Error popup's text, then dismisses it with OK. Returns the text so the
        // caller can assert on it and a mismatch shows what the popup actually said
        public string DismissErrorDialog()
        {
            var errorWindow = Retry.WhileNull(() =>
                    _window.FindFirstDescendant(cf => cf.ByName("Error").And(cf.ByControlType(ControlType.Window))) ??
                    _window.Automation.GetDesktop().FindFirstChild(cf => cf.ByName("Error").And(cf.ByControlType(ControlType.Window))),
                TimeSpan.FromSeconds(10)).Result
                .OrThrow("Error popup");

            string message = string.Join(" | ", errorWindow
                .FindAllDescendants(cf => cf.ByControlType(ControlType.Text))
                .Select(x => x.Name?.Trim())
                .Where(x => !string.IsNullOrEmpty(x)));

            _assistant.FindByName(errorWindow, "OK", 5)
                .OrThrow("OK button on Error popup")
                .Click();

            Wait.UntilInputIsProcessed();
            Thread.Sleep(300);

            System.Diagnostics.Debug.WriteLine($"Dismissed Error popup: {message}");

            return message;
        }

        // Closes the New Sheet dialog if it is still open (e.g. after a rejected name)
        public void CloseNewSheetDialogIfOpen()
        {
            var dialog = FindNewSheetDialog();

            if (dialog == null)
                return;

            dialog.AsWindow().Close();
            Wait.UntilInputIsProcessed();
        }

        private AutomationElement? FindNewSheetDialog()
        {
            return _window.FindFirstDescendant(cf => cf.ByAutomationId("NewSheetForm")) ??
                   _window.Automation.GetDesktop().FindFirstChild(cf => cf.ByAutomationId("NewSheetForm"));
        }

        // The context menu is a popup window, so look beside the main window as well as under it
        private AutomationElement? FindOpenDropDown()
        {
            return _window.FindAllDescendants(cf => cf.ByName("DropDown")).FirstOrDefault(e => !e.IsOffscreen)
                ?? _window.Automation.GetDesktop().FindAllChildren(cf => cf.ByName("DropDown")).FirstOrDefault(e => !e.IsOffscreen);
        }

        private AutomationElement FindTopNode(AutomationElement tree, string pageName, TimeSpan timeout)
        {
            return Retry.WhileNull(() =>
                _assistant.FindByName(tree, pageName),
                timeout).Result.OrThrow($"Top-level item '{pageName}'");
        }

        // The DotNetBar tree has no UIA expand, but its MSAA state is reliable. Every way of expanding
        // here toggles, so each one only runs while the node is still collapsed.
        private static void Expand(AutomationElement topNode, List<string> log)
        {
            var attempts = new (string Name, Action Run)[]
            {
                // MSAA default action is "Expand" on a collapsed node
                ("MSAA default action", () => topNode.Patterns.LegacyIAccessible.Pattern.DoDefaultAction()),
                ("select + Right arrow", () =>
                {
                    topNode.Patterns.SelectionItem.Pattern.Select();
                    topNode.Focus();
                    Wait.UntilInputIsProcessed();
                    Keyboard.Press(VirtualKeyShort.RIGHT);
                }),
                ("double-click", () => Mouse.DoubleClick(topNode.BoundingRectangle.Center())),
            };

            foreach (var attempt in attempts)
            {
                if (IsExpanded(topNode))
                {
                    log.Add("node expanded");
                    return;
                }

                try
                {
                    attempt.Run();
                }
                catch (Exception ex) when (ex is COMException || ex is InvalidOperationException || ex is NotSupportedException)
                {
                    log.Add($"{attempt.Name} failed: {ex.Message}");
                    continue;
                }

                Wait.UntilInputIsProcessed();

                bool expanded = Retry.WhileFalse(() => IsExpanded(topNode), TimeSpan.FromSeconds(2)).Result;

                log.Add($"{attempt.Name}: {(expanded ? "expanded" : "still collapsed")}");

                if (expanded)
                    return;
            }
        }

        private static bool IsExpanded(AutomationElement node)
        {
            try
            {
                return node.Patterns.LegacyIAccessible.IsSupported &&
                       node.Patterns.LegacyIAccessible.Pattern.State.ValueOrDefault.HasFlag(AccessibilityState.STATE_SYSTEM_EXPANDED);
            }
            catch (COMException)
            {
                return false;
            }
        }

        // Children are sorted by name, so the sheet is not necessarily the first one
        private static AutomationElement? FindChildSheet(AutomationElement topNode, string sheetName)
        {
            if (!IsExpanded(topNode))
                return null;

            var children = topNode.FindAllChildren();

            return children.FirstOrDefault(x => string.Equals(x.Name?.Trim(), sheetName, StringComparison.OrdinalIgnoreCase))
                ?? children.FirstOrDefault(x => (x.Name ?? "").Trim().StartsWith(sheetName, StringComparison.OrdinalIgnoreCase));
        }

        // For the failure message, so the next failure shows what the tree actually looked like
        private static string DescribeChildren(AutomationElement topNode)
        {
            string state = topNode.Patterns.LegacyIAccessible.IsSupported
                ? topNode.Patterns.LegacyIAccessible.Pattern.State.ValueOrDefault.ToString()
                : "no MSAA";

            return $"state {state}; " + string.Join(", ", topNode.FindAllChildren()
                .Take(40)
                .Select(x => $"'{x.Name}'@{x.BoundingRectangle.Left},{x.BoundingRectangle.Top}"));
        }

        public void oldOpenPage(string pageName)
        {
            var tree = FindTree();

            // Find top-level node
            AutomationElement topNode = Retry.WhileNull(() =>
                _assistant.FindByName(tree, pageName),
                TimeSpan.FromSeconds(10)).Result.OrThrow($"Top-level item '{pageName}'");

            System.Diagnostics.Debug.WriteLine($"Found {pageName} menu");
            System.Diagnostics.Debug.WriteLine($"Details: [ {topNode} ]\n");
            topNode.DoubleClick();

            // Find Default child
            var defaultNode = Retry.WhileNull(() =>
            {
                var node = _assistant.FindByName(topNode, "Default");
                if (node != null && node.Patterns.ScrollItem.IsSupported)
                    node.Patterns.ScrollItem.Pattern.ScrollIntoView();
                return node;
            }, TimeSpan.FromSeconds(10)).Result;

            if (defaultNode == null)
                throw new Exception($"Default node not found under '{pageName}'");

            defaultNode.Focus();
            var rect = defaultNode.BoundingRectangle;
            // defaultNode.DoubleClick(); 
            Mouse.DoubleClick(rect.Center());
        }

        private AutomationElement FindTree()
        {
            return Retry.WhileNull(
                () =>
                {
                    try
                    {
                        var pnlxPage =
                            _assistant.FindByAutomationId(
                                _window,
                                "pnlxPage",
                                3);

                        if (pnlxPage == null)
                            return null;


                        var designUC =
                            _assistant.FindByAutomationId(
                                pnlxPage,
                                "projectDesignUserControl",
                                3);

                        if (designUC == null)
                            return null;


                        var dockSite =
                            _assistant.FindByAutomationId(
                                designUC,
                                "mainLeftDockSite",
                                3);

                        if (dockSite == null)
                            return null;


                        var leftBar =
                            _assistant.FindByName(
                                dockSite,
                                "DotNetBar Bar");

                        if (leftBar == null)
                            return null;


                        var projectExplorer =
                            _assistant.FindByName(
                                leftBar,
                                "Project Explorer");

                        if (projectExplorer == null)
                            return null;


                        var explorerUC =
                            _assistant.FindByAutomationId(
                                projectExplorer,
                                "projectExplorerUserControl",
                                3);

                        if (explorerUC == null)
                            return null;


                        var tableLayout =
                            _assistant.FindByAutomationId(
                                explorerUC,
                                "tableLayoutPanel",
                                3);

                        if (tableLayout == null)
                            return null;


                        return _assistant.FindByAutomationId(
                            tableLayout,
                            "projectAdvTree",
                            3);
                    }
                    catch (COMException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"UIA timeout while finding Project Explorer: {ex.Message}");

                        return null;
                    }
                },
                TimeSpan.FromSeconds(30),
                TimeSpan.FromMilliseconds(500)
            ).Result.OrThrow("Project Explorer tree");
        }
    }
}
