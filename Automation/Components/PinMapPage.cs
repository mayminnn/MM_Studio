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
    public class PinMapPage
    {
        private readonly AutomationElement _window;
        private readonly UIAssistant _assistant;

        private readonly string _sheetName;

        public PinMapPage(AutomationElement window, UIAssistant assistant, string sheetName = "Default")
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
            _sheetName = sheetName;
        }

        private AutomationElement GetSheetEditor()
        {
            var sheet = SheetLocator.FindDocument(_window, _assistant, $"Pin Map={_sheetName}");
            return _assistant.FindByAutomationId(sheet, "PinMapSheetEditor").OrThrow("PinMapSheetEditor");
        }

        private AutomationElement GetFormulaBar()
        {
            var sheetEditor = GetSheetEditor();
            return _assistant.FindByAutomationId(sheetEditor, "formulaBar").OrThrow("formulaBar");
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

            // Clear only the selected cell. Do NOT use F2 + Ctrl+A here: if the cell
            // is not in edit mode, Ctrl+A selects the whole sheet and Delete wipes it.
            Keyboard.Type(VirtualKeyShort.DELETE);
            Thread.Sleep(200);

            if (string.IsNullOrEmpty(value))
            {
                System.Diagnostics.Debug.WriteLine($"{cellName} cleared to blank");
                return;
            }

            // Typing into a selected (not editing) cell replaces its content
            Keyboard.Type(value);
            Thread.Sleep(100);

            Keyboard.Type(VirtualKeyShort.ENTER);

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

        public string GetCellValue(string cell)
        {
            SelectCellByFormula(cell);

            Thread.Sleep(300);

            Keyboard.Press(VirtualKeyShort.CONTROL);
            Keyboard.Press(VirtualKeyShort.KEY_C);
            Keyboard.Release(VirtualKeyShort.KEY_C);
            Keyboard.Release(VirtualKeyShort.CONTROL);

            Thread.Sleep(300);

            return System.Windows.Forms.Clipboard.GetText().Trim();
        }
        
        public HashSet<string> GetPinGroupAndPinNames()
        {
            var values = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            int lastRow = GetLastPinRow();

            System.Diagnostics.Debug.WriteLine(
                $"Last Row = {lastRow}");

            //
            // Read Column C (Pin Names)
            //
            SelectCellByFormula("C4");

            Thread.Sleep(300);

            for (int row = 4; row <= lastRow; row++)
            {
                string value = GetSelectedCellValueByCopy();

                if (!string.IsNullOrWhiteSpace(value))
                {
                    values.Add(value.Trim());
                }

                if (row < lastRow)
                {
                    Keyboard.Press(VirtualKeyShort.DOWN);
                    Keyboard.Release(VirtualKeyShort.DOWN);

                    Thread.Sleep(50);
                }
            }

            //
            // Read Column B (Pin Groups)
            //
            SelectCellByFormula("B4");

            Thread.Sleep(300);

            for (int row = 4; row <= lastRow; row++)
            {
                string value = GetSelectedCellValueByCopy();

                if (!string.IsNullOrWhiteSpace(value))
                {
                    values.Add(value.Trim());
                }

                if (row < lastRow)
                {
                    Keyboard.Press(VirtualKeyShort.DOWN);
                    Keyboard.Release(VirtualKeyShort.DOWN);

                    Thread.Sleep(50);
                }
            }

            System.Diagnostics.Debug.WriteLine(
                $"Combined Unique Values = {values.Count}");

            return values;
        }

        public int GetLastPinRow()
        {
            SelectCellByFormula("C4");

            Thread.Sleep(300);

            Keyboard.Press(VirtualKeyShort.CONTROL);
            Keyboard.Press(VirtualKeyShort.DOWN);

            Thread.Sleep(200);

            Keyboard.Release(VirtualKeyShort.DOWN);
            Keyboard.Release(VirtualKeyShort.CONTROL);

            Thread.Sleep(500);

            var formulaBar = GetFormulaBar();

            var combo = formulaBar.FindFirstDescendant(
                cf => cf.ByControlType(ControlType.ComboBox))
                .OrThrow("Formula bar combo box");

            var edit = combo.FindFirstDescendant(
                cf => cf.ByControlType(ControlType.Edit))
                .OrThrow("Formula bar edit box");

            string address =
                (edit.AsTextBox().Text ?? "").Trim();

            System.Diagnostics.Debug.WriteLine(
                $"Current Address = {address}");

            // Extract row number
            string rowText =
                new string(address
                    .SkipWhile(c => !char.IsDigit(c))
                    .ToArray());

            return int.Parse(rowText);
        }

        private string GetSelectedCellValueByCopy()
        {
            Keyboard.Press(VirtualKeyShort.CONTROL);
            Keyboard.Press(VirtualKeyShort.KEY_C);

            Keyboard.Release(VirtualKeyShort.KEY_C);
            Keyboard.Release(VirtualKeyShort.CONTROL);

            Thread.Sleep(200);

            return System.Windows.Forms.Clipboard
                .GetText()
                .Trim();
        }

        // The dropdown is a popup window, so look beside the main window as well as under it
        private AutomationElement? FindOpenDropDown()
        {
            return _window.FindAllDescendants(cf => cf.ByName("DropDown")).FirstOrDefault(e => !e.IsOffscreen)
                ?? _window.Automation.GetDesktop().FindAllChildren(cf => cf.ByName("DropDown")).FirstOrDefault(e => !e.IsOffscreen);
        }

        private string[] GetVisibleDropdownItems()
        {
            var dropdown = Retry.WhileNull(FindOpenDropDown, TimeSpan.FromSeconds(5))?.Result;

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

        public void ValidateColumnDDropdowns(int startRow = 4, int endRow = 10)
        {
            string[] expectedItems = { "Analog", "Gnd", "IO", "Power", "Utility" };

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

        public void ConfigurePMRow(int row, string pinGroup, string pinName,
        string type, string remark)
        {
            ClearAndEnterCellValue($"B{row}", pinGroup);
            ClearAndEnterCellValue($"C{row}", pinName);
            EnterDropdownValue($"D{row}", type);
            ClearAndEnterCellValue($"E{row}", remark);
        }

        private void EnterDropdownValue(string cell, string value)
        {
            SelectCellByFormula(cell);
            Thread.Sleep(200);

            // Clear only the selected cell (no F2 + Ctrl+A, which can select the whole sheet)
            Keyboard.Type(VirtualKeyShort.DELETE);
            Thread.Sleep(200);

            // If blank, stop here
            if (string.IsNullOrEmpty(value))
            {
                System.Diagnostics.Debug.WriteLine($"{cell} cleared to blank");
                return;
            }

            // Let the grid settle back on the cell before opening the dropdown
            Wait.UntilInputIsProcessed();
            Thread.Sleep(300);

            AutomationElement? dropdown = null;
            for (int attempt = 1; attempt <= 3 && dropdown == null; attempt++)
            {
                if (attempt > 1)
                {
                    System.Diagnostics.Debug.WriteLine($"Dropdown for {cell} did not open, retrying ({attempt}/3)...");
                    SelectCellByFormula(cell);
                    Thread.Sleep(300);
                }

                ExpandDropdownForCell();

                dropdown = Retry.WhileNull(FindOpenDropDown, TimeSpan.FromSeconds(3))?.Result;
            }

            if (dropdown == null)
                throw new Exception($"Dropdown container not found for {cell}");

            var list = Retry.WhileNull(() =>
                dropdown.FindFirstDescendant(cf => cf.ByControlType(ControlType.List)),
                TimeSpan.FromSeconds(3))?.Result;
            if (list == null)
                throw new Exception("Dropdown List not found inside DropDown container");

            var item = list.FindFirstDescendant(cf => cf.ByName(value).And(cf.ByControlType(ControlType.ListItem)));
            if (item == null)
            {
                string available = string.Join(", ", list.FindAllDescendants(cf => cf.ByControlType(ControlType.ListItem))
                    .Select(x => x.Name));
                throw new Exception($"'{value}' not found in {cell} dropdown. Items: [{available}]");
            }

            item.Click();
            Wait.UntilInputIsProcessed();
            Thread.Sleep(300);

            System.Diagnostics.Debug.WriteLine($"{cell} = {value}");
        }

        public void ClickValidate()
        {
            var sheetEditor = GetSheetEditor();
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
            var logList = _assistant.FindByAutomationId(panel, "logListView").OrThrow("logListView");

            var logs = logList.FindAllDescendants();

            return logs.Any(x =>
                !string.IsNullOrEmpty(x.Name) &&
                x.Name.Contains(expectedText));
        }
    }
}
