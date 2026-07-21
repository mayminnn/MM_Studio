using Automation.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Automation.Components
{
    public class BinSheetPage
    {
        private readonly AutomationElement _window;
        private readonly UIAssistant _assistant;

        public BinSheetPage(AutomationElement window, UIAssistant assistant)
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

        private AutomationElement GetFormulaBar()
        {
            var pnlxPage = _assistant.FindByAutomationId(_window, "pnlxPage");
            var designUC = _assistant.FindByAutomationId(pnlxPage, "projectDesignUserControl");
            var dockSite = _assistant.FindByAutomationId(designUC, "mainDocDockSite");
            var bar = _assistant.FindByName(dockSite, "DotNetBar Bar");
            var binDefault = _assistant.FindByName(bar, "Bin=Default");
            var sheetEditor = _assistant.FindByAutomationId(binDefault, "BinSheetEditor");

            return _assistant.FindByAutomationId(sheetEditor, "formulaBar");
        }

        private void SelectCellByFormula(string cellName)
        {
            var formulaBar = GetFormulaBar();

            var combo = formulaBar.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.ComboBox));

            if (combo == null)
                throw new Exception("Formula bar ComboBox not found");

            var edit = combo.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.Edit));

            if (edit == null)
                throw new Exception("Formula bar Edit not found");

            var textBox = edit.AsTextBox();

            combo.Focus();
            Thread.Sleep(100);

            textBox.Focus();
            Thread.Sleep(100);

            // HARD clear old value
            textBox.Text = "";
            Thread.Sleep(100);

            Keyboard.Press(VirtualKeyShort.CONTROL);
            Keyboard.Press(VirtualKeyShort.KEY_A);
            Keyboard.Release(VirtualKeyShort.KEY_A);
            Keyboard.Release(VirtualKeyShort.CONTROL);

            Thread.Sleep(100);

            Keyboard.Press(VirtualKeyShort.DELETE);
            Thread.Sleep(100);

            // Type new cell
            Keyboard.Type(cellName);

            Thread.Sleep(200);

            Keyboard.Press(VirtualKeyShort.ENTER);

            Wait.UntilInputIsProcessed();
            Thread.Sleep(500);

            System.Diagnostics.Debug.WriteLine($"Formula bar updated to {cellName}");
        }

        private void ClearAndEnterCellValue(string cellName, string value)
        {
            SelectCellByFormula(cellName);

            Thread.Sleep(200);

            Keyboard.Press(VirtualKeyShort.F2); // enter edit mode
            Thread.Sleep(100);

            Keyboard.Press(VirtualKeyShort.CONTROL);
            Keyboard.Press(VirtualKeyShort.KEY_A);
            Keyboard.Release(VirtualKeyShort.KEY_A);
            Keyboard.Release(VirtualKeyShort.CONTROL);

            Thread.Sleep(100);

            Keyboard.Press(VirtualKeyShort.DELETE);
            Thread.Sleep(200);

            if (!string.IsNullOrEmpty(value))
            {
                Keyboard.Type(value);
                Thread.Sleep(100);
            }

            Keyboard.Press(VirtualKeyShort.ENTER);

            Wait.UntilInputIsProcessed();
            Thread.Sleep(300);

            System.Diagnostics.Debug.WriteLine($"{cellName} = '{value}'");
        }

        private void ExpandDropdownForCell()
        {
            Keyboard.Press(VirtualKeyShort.ALT);
            Keyboard.Press(VirtualKeyShort.DOWN);
            Keyboard.Release(VirtualKeyShort.DOWN);
            Keyboard.Release(VirtualKeyShort.ALT);

            Thread.Sleep(300);
        }

        private string[] GetVisibleDropdownItems()
        {
            // Find the visible dropdown container
            var dropdown = Retry.WhileNull(() =>
                _window.FindAllDescendants(cf => cf.ByName("DropDown"))
                       .FirstOrDefault(e => !e.IsOffscreen),
                TimeSpan.FromSeconds(5))?.Result;

            if (dropdown == null)
                throw new Exception("Dropdown container not found");

            System.Diagnostics.Debug.WriteLine("Found dropdown container, waiting for List...");

            var list = Retry.WhileNull(() =>
                dropdown.FindFirstChild(cf => cf.ByControlType(ControlType.List)),
                TimeSpan.FromSeconds(5))?.Result;

            if (list == null)
                throw new Exception("Dropdown List not found inside DropDown container");

            var items = Retry.While(() =>
                list.FindAllChildren(cf => cf.ByControlType(ControlType.ListItem))
                    .Where(x => !string.IsNullOrEmpty(x.Name))
                    .ToArray(),
                arr => arr.Length == 0,
                TimeSpan.FromSeconds(5))?.Result;

            if (items == null || items.Length == 0)
                throw new Exception("Dropdown List has no visible items");

            System.Diagnostics.Debug.WriteLine($"Dropdown items found: {string.Join(", ", items.Select(x => x.Name))}");

            return items.Select(x => x.Name).ToArray();
        }

        public void ValidateColumnEDropdowns(int startRow = 7, int endRow = 12)
        {
            string[] expectedItems = { "Pass", "Fail", "Undefined", "Non-Functional", "Alarmed", "Aborted" };

            for (int row = startRow; row <= endRow; row++)
            {
                string cell = $"E{row}";
                System.Diagnostics.Debug.WriteLine("----------------------");
                System.Diagnostics.Debug.WriteLine($"Validating {cell}...");
                System.Diagnostics.Debug.WriteLine("----------------------");

                SelectCellByFormula(cell);
                ExpandDropdownForCell();
                var items = GetVisibleDropdownItems();

                foreach (var expected in expectedItems)
                {
                    if (!items.Contains(expected))
                        throw new Exception($"Dropdown validation failed for {cell}: expected '{expected}', found [{string.Join(", ", items)}]");
                }

                System.Diagnostics.Debug.WriteLine($"{cell} validated successfully");

                Keyboard.Press(VirtualKeyShort.ESCAPE);
                Thread.Sleep(300);
            }
        }

        public string GetCellValue(string cell)
        {
            SelectCellByFormula(cell);

            Thread.Sleep(300);

            Keyboard.Press(VirtualKeyShort.CONTROL);
            Keyboard.Press(VirtualKeyShort.KEY_C);
            Keyboard.Release(VirtualKeyShort.KEY_C);
            Keyboard.Release(VirtualKeyShort.CONTROL);

            Thread.Sleep(300);

            return System.Windows.Forms.Clipboard.GetText()?.Trim();
        }

        public List<string> GetFailBinNames()
        {
            var result = new List<string>();

            int row = 4;

            while (true)
            {
                string binName = GetCellValue($"C{row}");

                if (string.IsNullOrWhiteSpace(binName))
                    break;

                string type = GetCellValue($"E{row}");

                if (type.Equals("Fail",
                        StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(binName);
                }

                row++;
            }

            return result
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        public void ConfigureBinRow(int row, string swBin, string binName,
        string hwBin, string type, string remark)
        {
            ClearAndEnterCellValue($"B{row}", swBin);
            ClearAndEnterCellValue($"C{row}", binName);
            ClearAndEnterCellValue($"D{row}", hwBin);
            EnterDropdownValue($"E{row}", type);
            ClearAndEnterCellValue($"F{row}", remark);
        }

        private void EnterDropdownValue(string cell, string value)
        {
            SelectCellByFormula(cell);
            Thread.Sleep(200);

            Keyboard.Press(VirtualKeyShort.F2);
            Thread.Sleep(100);

            Keyboard.Press(VirtualKeyShort.CONTROL);
            Keyboard.Press(VirtualKeyShort.KEY_A);
            Keyboard.Release(VirtualKeyShort.KEY_A);
            Keyboard.Release(VirtualKeyShort.CONTROL);
            Thread.Sleep(100);

            Keyboard.Press(VirtualKeyShort.DELETE);
            Thread.Sleep(200);

            Keyboard.Press(VirtualKeyShort.ENTER);
            Thread.Sleep(200);

            Keyboard.Press(VirtualKeyShort.UP);

            // If blank, stop here
            if (string.IsNullOrEmpty(value))
            {
                System.Diagnostics.Debug.WriteLine($"{cell} cleared to blank");
                return;
            }

            ExpandDropdownForCell();

            var dropdown = Retry.WhileNull(() =>
                _window.FindAllDescendants(cf => cf.ByName("DropDown"))
                       .FirstOrDefault(e => !e.IsOffscreen),
                TimeSpan.FromSeconds(5))?.Result;

            if (dropdown == null)
                throw new Exception("Dropdown container not found");

            var list = dropdown.FindFirstChild(cf => cf.ByControlType(ControlType.List));
            if (list == null)
                throw new Exception("Dropdown List not found inside DropDown container");

            var item = list.FindFirstChild(cf => cf.ByName(value).And(cf.ByControlType(ControlType.ListItem)));
            if (item == null)
                throw new Exception($"{value} not found in {cell}");

            item.Click();
            Wait.UntilInputIsProcessed();
            Thread.Sleep(300);

            System.Diagnostics.Debug.WriteLine($"{cell} = {value}");
        }

        public void ClickValidate()
        {
            var pnlxPage = _assistant.FindByAutomationId(_window, "pnlxPage");
            var designUC = _assistant.FindByAutomationId(pnlxPage, "projectDesignUserControl");
            var dockSite = _assistant.FindByAutomationId(designUC, "mainDocDockSite");
            var bar = _assistant.FindByAutomationId(dockSite, "Document");
            var binDefault = _assistant.FindByName(bar, "Bin=Default");
            var sheetEditor = _assistant.FindByAutomationId(binDefault, "BinSheetEditor");

            var tableLayout = _assistant.FindByAutomationId(sheetEditor, "tableLayoutPanel");
            var topToolStrip = _assistant.FindByAutomationId(sheetEditor, "mainTopToolStrip");

            var validateBtn = _assistant.FindByName(sheetEditor, "Validate");

            if (validateBtn == null)
                throw new Exception("Validate button not found");

            validateBtn.AsButton().Invoke();

            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(1000);

            System.Diagnostics.Debug.WriteLine("Clicked Validate");
        }

        public bool VerifyValidationSuccessLog(string expectedText)
        {
            var logPanel = _assistant.FindByAutomationId(_window, "logUserControl");
            var panel = _assistant.FindByAutomationId(logPanel, "panel");
            var logList = _assistant.FindByAutomationId(panel, "logListView");

            var logs = logList.FindAllDescendants();

            return logs.Any(x =>
                !string.IsNullOrEmpty(x.Name) &&
                x.Name.Contains(expectedText));
        }
    }
}