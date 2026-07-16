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
    public class DPSPage
    {
        private readonly AutomationElement _window;
        private readonly UIAssistant _assistant;

        public DPSPage(AutomationElement window, UIAssistant assistant)
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
            var DPSsheet = _assistant.FindByName(bar, "DPS=Default");
            var sheetEditor = _assistant.FindByAutomationId(DPSsheet, "DPSSheetEditor");

            return _assistant.FindByAutomationId(sheetEditor, "formulaBar");
        }

        public void SelectCellByFormula(string cellName)
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

        public string GetSelectedCellValueByCopy()
        {
            Thread.Sleep(200);

            // Copy cell content
            Keyboard.Press(VirtualKeyShort.CONTROL);
            Keyboard.Press(VirtualKeyShort.KEY_C);
            Keyboard.Release(VirtualKeyShort.KEY_C);
            Keyboard.Release(VirtualKeyShort.CONTROL);

            Thread.Sleep(300);

            return System.Windows.Forms.Clipboard.GetText()?.Trim();
        }

        public void ClearAndEnterCellValue(string cellName, string value)
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
            Thread.Sleep(300);

            Keyboard.Press(VirtualKeyShort.ALT);
            Keyboard.Press(VirtualKeyShort.DOWN);
            Keyboard.Release(VirtualKeyShort.DOWN);
            Keyboard.Release(VirtualKeyShort.ALT);

            Thread.Sleep(300);
        }

        private string[] GetVisibleDropdownItems()
        {
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

        public void EnterDropdownValue(string cell, string value)
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

        public void configureDPSrow(int row, string set, string seq, string pin, string bypass,
            string mode, float fvalue, string frange, string clamp, string crange, float delay1,
            float delay2, int limitL, int limitH, string remark)
        {
            ClearAndEnterCellValue($"B{row}", set);
            EnterDropdownValue($"C{row}", seq);
            EnterDropdownValue($"E{row}", pin);
            EnterDropdownValue($"F{row}", bypass);
            EnterDropdownValue($"G{row}", mode);
            ClearAndEnterCellValue($"H{row}", fvalue.ToString());
            EnterDropdownValue($"I{row}", frange);
            ClearAndEnterCellValue($"J{row}", clamp);
            EnterDropdownValue($"K{row}", crange);
            ClearAndEnterCellValue($"L{row}", delay1.ToString());
            ClearAndEnterCellValue($"M{row}", delay2.ToString());
            ClearAndEnterCellValue($"N{row}", limitL.ToString());
            ClearAndEnterCellValue($"O{row}", limitH.ToString());
            ClearAndEnterCellValue($"P{row}", remark);
        }

        private AutomationElement GetSheetEditor()
        {
            var pnlxPage = _assistant.FindByAutomationId(_window, "pnlxPage");
            var designUC = _assistant.FindByAutomationId(pnlxPage, "projectDesignUserControl");
            var dockSite = _assistant.FindByAutomationId(designUC, "mainDocDockSite");
            var bar = _assistant.FindByAutomationId(dockSite, "Document");
            var DPSdefault = _assistant.FindByName(bar, "DPS=Default");
            return _assistant.FindByAutomationId(DPSdefault, "DPSSheetEditor");
        }

        public string GetCellValue(string cell)
        {
            SelectCellByFormula(cell);

            Thread.Sleep(300);

            return GetSelectedCellValueByCopy();
        }

        public HashSet<string> GetPinDropdownValues(
    int row = 4)
        {
            var values = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            SelectCellByFormula($"E{row}");

            ExpandDropdownForCell();

            var dropdown = Retry.WhileNull(() =>
                _window.FindAllDescendants(cf => cf.ByName("DropDown"))
                       .FirstOrDefault(e => !e.IsOffscreen),
                TimeSpan.FromSeconds(5))?.Result;

            if (dropdown == null)
                throw new Exception(
                    "Dropdown container not found");

            var list = dropdown.FindFirstChild(
                cf => cf.ByControlType(ControlType.List));

            if (list == null)
                throw new Exception(
                    "Dropdown list not found");

            var items = list.FindAllChildren(
                    cf => cf.ByControlType(ControlType.ListItem))
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Select(x => x.Name.Trim());

            foreach (var item in items)
            {
                values.Add(item);
            }

            Keyboard.Press(VirtualKeyShort.ESCAPE);

            System.Diagnostics.Debug.WriteLine(
                $"DPS dropdown values = {values.Count}");

            return values;
        }

        public List<string> oldGetPinDropdownValues(int row = 4)
        {
            string cell = $"E{row}";

            SelectCellByFormula(cell);

            Thread.Sleep(200);

            ExpandDropdownForCell();

            var items = GetVisibleDropdownItems();

            Keyboard.Press(VirtualKeyShort.ESCAPE);

            return items
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();
        }

        public void SetSequenceAndMode(int row, string sequence, string mode)
        {
            EnterDropdownValue($"C{row}", sequence);
            EnterDropdownValue($"G{row}", mode);

            Wait.UntilInputIsProcessed();
            Thread.Sleep(500);
        }

        public string GetFlowValue(int row)
        {
            string cell = $"D{row}";

            SelectCellByFormula(cell);

            Thread.Sleep(200);

            // Copy cell content
            Keyboard.Press(VirtualKeyShort.CONTROL);
            Keyboard.Press(VirtualKeyShort.KEY_C);
            Keyboard.Release(VirtualKeyShort.KEY_C);
            Keyboard.Release(VirtualKeyShort.CONTROL);

            Thread.Sleep(300);

            string flowValue = System.Windows.Forms.Clipboard.GetText()?.Trim();

            System.Diagnostics.Debug.WriteLine($"{cell} = {flowValue}");

            return flowValue;
        }

        public void VerifyFlowRule(int row, string sequence, string mode, string expectedFlow)
        {
            EnterDropdownValue($"C{row}", sequence);
            EnterDropdownValue($"G{row}", mode);

            Wait.UntilInputIsProcessed();
            Thread.Sleep(500);

            string actualFlow = GetFlowValue(row);

            if (actualFlow != expectedFlow)
            {
                throw new Exception(
                    $"Flow mismatch at D{row}. " +
                    $"Expected '{expectedFlow}', " +
                    $"Actual '{actualFlow}'");
            }

            System.Diagnostics.Debug.WriteLine(
                $"PASS: D{row} = {actualFlow}");
        }

        public string GetFRangeValue(int row)
        {
            string cell = $"I{row}";

            SelectCellByFormula(cell);

            Thread.Sleep(200);

            // Copy cell content
            Keyboard.Press(VirtualKeyShort.CONTROL);
            Keyboard.Press(VirtualKeyShort.KEY_C);
            Keyboard.Release(VirtualKeyShort.KEY_C);
            Keyboard.Release(VirtualKeyShort.CONTROL);

            Thread.Sleep(300);

            string value = System.Windows.Forms.Clipboard.GetText()?.Trim();

            System.Diagnostics.Debug.WriteLine($"{cell} = {value}");

            return value;
        }

        public string GetCRangeValue(int row)
        {
            string cell = $"K{row}";

            SelectCellByFormula(cell);

            Thread.Sleep(200);

            // Copy cell content
            Keyboard.Press(VirtualKeyShort.CONTROL);
            Keyboard.Press(VirtualKeyShort.KEY_C);
            Keyboard.Release(VirtualKeyShort.KEY_C);
            Keyboard.Release(VirtualKeyShort.CONTROL);

            Thread.Sleep(300);

            string value = System.Windows.Forms.Clipboard.GetText()?.Trim();

            System.Diagnostics.Debug.WriteLine($"{cell} = {value}");

            return value;
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
            var logList = _assistant.FindByAutomationId(panel, "logListView");

            var logs = logList.FindAllDescendants();

            return logs.Any(x =>
                !string.IsNullOrEmpty(x.Name) &&
                x.Name.Contains(expectedText));
        }        
    }
}