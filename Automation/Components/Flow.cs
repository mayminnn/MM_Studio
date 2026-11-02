using Automation.Core;
using Automation.Models;
using Automation.Parsers;
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
    public class Flow
    {
        private readonly AutomationElement _window;
        private readonly UIAssistant _assistant;
        private readonly string _sheetName;

        public Flow(AutomationElement window, UIAssistant assistant, string sheetName = "Default")
        {
            if (window == null) throw new ArgumentNullException("window");
            if (assistant == null) throw new ArgumentNullException("assistant");

            _window = window;
            _assistant = assistant;
            _sheetName = sheetName;
        }

        private AutomationElement GetSheetEditor()
        {
            var sheet = SheetLocator.FindDocument(_window, _assistant, $"Flow={_sheetName}");
            return _assistant.FindByAutomationId(sheet, "FlowSheetEditor").OrThrow("FlowSheetEditor");
        }

        private AutomationElement GetFormulaBar()
        {
            var sheetEditor = GetSheetEditor();
            return _assistant.FindByAutomationId(sheetEditor, "formulaBar").OrThrow("formulaBar");
        }

        private AutomationElement SelectCellByFormula(string cellName)
        {
            var formulaBar = GetFormulaBar();

            var combo = formulaBar.FindFirstDescendant(cf => cf.ByControlType(ControlType.ComboBox));
            if (combo == null) throw new Exception("Formula bar ComboBox not found");

            var edit = combo.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit));
            if (edit == null) throw new Exception("Formula bar Edit not found");

            var textBox = edit.AsTextBox();

            combo.Focus();
            Thread.Sleep(100);

            textBox.Focus();
            Thread.Sleep(100);

            textBox.Text = "";
            Thread.Sleep(100);

            Keyboard.Press(VirtualKeyShort.CONTROL);
            Keyboard.Press(VirtualKeyShort.KEY_A);
            Keyboard.Release(VirtualKeyShort.KEY_A);
            Keyboard.Release(VirtualKeyShort.CONTROL);
            Thread.Sleep(100);

            Keyboard.Press(VirtualKeyShort.DELETE);
            Thread.Sleep(100);

            Keyboard.Type(cellName);
            Thread.Sleep(200);

            Keyboard.Press(VirtualKeyShort.ENTER);

            Wait.UntilInputIsProcessed();
            Thread.Sleep(500);

            System.Diagnostics.Debug.WriteLine($"Formula bar updated to {cellName}");

            return GetSheetEditor();
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

        // For dropdown cells (e.g. Command): clear the cell, open its list and click the item
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

            if (string.IsNullOrEmpty(value))
            {
                System.Diagnostics.Debug.WriteLine($"{cell} cleared to blank");
                return;
            }

            Thread.Sleep(300);

            Keyboard.Press(VirtualKeyShort.ALT);
            Keyboard.Press(VirtualKeyShort.DOWN);
            Keyboard.Release(VirtualKeyShort.DOWN);
            Keyboard.Release(VirtualKeyShort.ALT);

            Thread.Sleep(300);

            var dropdown = Retry.WhileNull(() =>
                _window.FindAllDescendants(cf => cf.ByName("DropDown"))
                       .FirstOrDefault(e => !e.IsOffscreen),
                TimeSpan.FromSeconds(5))?.Result;

            if (dropdown == null)
                throw new Exception($"Dropdown container not found for {cell}");

            var list = dropdown.FindFirstChild(cf => cf.ByControlType(ControlType.List));
            if (list == null)
                throw new Exception("Dropdown List not found inside DropDown container");

            var item = list.FindFirstChild(cf => cf.ByName(value).And(cf.ByControlType(ControlType.ListItem)));
            if (item == null)
            {
                string available = string.Join(", ", list.FindAllChildren(cf => cf.ByControlType(ControlType.ListItem))
                    .Select(x => x.Name));

                throw new Exception($"'{value}' not found in {cell} dropdown. Items: [{available}]");
            }

            item.Click();
            Wait.UntilInputIsProcessed();
            Thread.Sleep(300);

            System.Diagnostics.Debug.WriteLine($"{cell} = {value}");
        }

        public List<string> GetFailBinsDropdownItems(int row)
        {
            var sheet = SelectCellByFormula($"O{row}");

            Keyboard.Press(VirtualKeyShort.F2);
            Thread.Sleep(200);
            Keyboard.Press(VirtualKeyShort.ESCAPE);
            Thread.Sleep(200);

            Mouse.MoveTo(sheet.BoundingRectangle.Center());
            Thread.Sleep(200);
            Mouse.Click(MouseButton.Right);
            Thread.Sleep(300);

            var menu = Retry.WhileNull(() => _assistant.FindByName(_window, "Fail Bins"), TimeSpan.FromSeconds(5)).Result;
            if (menu == null) throw new Exception("Fail Bins menu not found.");

            menu.Click();
            Thread.Sleep(500);

            var dropDown = Retry.WhileNull(() => _assistant.FindByName(_window, "Fail BinsDropDown"), TimeSpan.FromSeconds(5)).Result;
            if (dropDown == null) throw new Exception("Fail BinsDropDown not found.");

            return dropDown.FindAllDescendants(cf => cf.ByControlType(ControlType.MenuItem))
                .Select(x => x.Name)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        public List<string> GetFailBinsFromPicker(int row)
        {
            var sheet = SelectCellByFormula($"O{row}");

            Mouse.MoveTo(sheet.BoundingRectangle.Center());
            Thread.Sleep(200);
            Mouse.Click(MouseButton.Right);
            Thread.Sleep(300);

            var failBins = _assistant.FindByName(_window, "Fail Bins").OrThrow("Fail Bins");
            failBins.Click();
            Thread.Sleep(300);

            var pickBin = _assistant.FindByName(_window, "Pick Fail Bin...");
            if (pickBin == null) throw new Exception("Pick Fail Bin... not found.");

            pickBin.Click();
            Thread.Sleep(500);

            var picker = _assistant.FindByAutomationId(_window, "BinPickerForm");
            if (picker == null) throw new Exception("BinPickerForm not found.");

            var list = _assistant.FindByAutomationId(picker, "listBoxBins");
            if (list == null) throw new Exception("listBoxBins not found.");

            return list.FindAllChildren()
                .Select(x => x.Name)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        public void ClickValidate()
        {
            var sheetEditor = GetSheetEditor();
            var validateBtn = _assistant.FindByName(sheetEditor, "Validate");
            if (validateBtn == null) throw new Exception("Validate button not found");

            validateBtn.AsButton().Invoke();

            Wait.UntilInputIsProcessed();
            Thread.Sleep(1000);

            System.Diagnostics.Debug.WriteLine("Clicked Validate");
        }

        public void ClickStartDebug()
        {
            var sheetEditor = GetSheetEditor();

            var startDebugGroup = _assistant.FindByName(sheetEditor, "Start Debug");
            if (startDebugGroup == null) throw new Exception("Start Debug group not found");

            var startDebugButton = startDebugGroup.FindFirstDescendant(cf => cf.ByControlType(ControlType.SplitButton));
            if (startDebugButton == null) throw new Exception("Start Debug SplitButton not found");

            startDebugButton.AsButton().Invoke();

            Wait.UntilInputIsProcessed();
            Thread.Sleep(1000);

            System.Diagnostics.Debug.WriteLine("Clicked Start Debug");
        }

        public bool VerifyValidationSuccessLog(string expectedText)
        {
            var logPanel = _assistant.FindByAutomationId(_window, "logUserControl");
            var panel = _assistant.FindByAutomationId(logPanel, "panel");
            var logList = _assistant.FindByAutomationId(panel, "logListView").OrThrow("logListView");

            var logs = logList.FindAllDescendants();

            return logs.Any(x => !string.IsNullOrEmpty(x.Name) && x.Name.Contains(expectedText));
        }

        // ===== Flow sheet extraction (primary method) =====
        // Navigates to B5, Ctrl+A ("select current region" - expands
        // to the contiguous block including the row 4 header above),
        // then Ctrl+C + clipboard read.
        public FlowSheetData SaveFlow(string projectName, string version, string testName, string flowName = "Default")
        {
            SelectCellByFormula("B5");
            Thread.Sleep(300);

            string rawText = ClipboardHelper.CopyAndGetText(() =>
            {
                Keyboard.Press(VirtualKeyShort.CONTROL);
                Keyboard.Press(VirtualKeyShort.KEY_A);
                Keyboard.Release(VirtualKeyShort.KEY_A);
                Keyboard.Release(VirtualKeyShort.CONTROL);
                Thread.Sleep(300);

                Keyboard.Press(VirtualKeyShort.CONTROL);
                Keyboard.Press(VirtualKeyShort.KEY_C);
                Keyboard.Release(VirtualKeyShort.KEY_C);
                Keyboard.Release(VirtualKeyShort.CONTROL);
            });

            Console.WriteLine($"Flow sheet clipboard length: {rawText.Length}");

            if (string.IsNullOrWhiteSpace(rawText))
                throw new Exception("Flow sheet copy returned empty text.");

            var parser = new FlowSheetParser();
            FlowSheetData data = parser.Parse(rawText);

            if (string.IsNullOrWhiteSpace(data.FlowName))
                data.FlowName = flowName;

            FlowSheetStorage.Save(projectName, version, testName, data);

            Console.WriteLine($"Flow sheet saved as JSON. Rows captured: {data.Rows.Count}");

            return data;
        }

        // ===== Cell-by-cell fallback (slower, use only if Ctrl+A ever
        // stops capturing correctly - e.g. a blank row breaks the
        // current-region expansion) =====
        public string GetCellValue(string cell)
        {
            SelectCellByFormula(cell);
            Thread.Sleep(300);

            return ClipboardHelper.CopyAndGetText(() =>
            {
                Keyboard.Press(VirtualKeyShort.CONTROL);
                Keyboard.Press(VirtualKeyShort.KEY_C);
                Keyboard.Release(VirtualKeyShort.KEY_C);
                Keyboard.Release(VirtualKeyShort.CONTROL);
            }).Trim();
        }

        private static readonly string[] ColumnLetters =
        {
            "B", "C", "D", "E", "F", "G", "H", "I", "J", "K",
            "L", "M", "N", "O", "P", "Q", "R", "S"
        };

        public FlowSheetData GetFlowSheetData(string flowName, int startRow = 5, int maxRows = 200)
        {
            var data = new FlowSheetData { FlowName = flowName };

            for (int row = startRow; row < startRow + maxRows; row++)
            {
                string command = GetCellValue($"D{row}");
                if (string.IsNullOrWhiteSpace(command))
                    break;

                var values = new Dictionary<string, string>();
                foreach (string col in ColumnLetters)
                    values[col] = GetCellValue($"{col}{row}");

                data.Rows.Add(new FlowSheetRow
                {
                    Label = values["B"],
                    Enable = values["C"],
                    Command = values["D"],
                    Parameter = values["E"],
                    Property = values["F"],
                    Value = values["G"],
                    TestName = values["H"],
                    TestNum = values["I"],
                    LimitLow = ParseDouble(values["J"]),
                    LimitHigh = ParseDouble(values["K"]),
                    PassPath = values["L"],
                    FailPath = values["M"],
                    PassBin = values["N"],
                    FailBin = values["O"],
                    Result = values["P"],
                    Assume = values["Q"],
                    Sites = values["R"],
                    Remarks = values["S"],
                });
            }

            return data;
        }

        private static double? ParseDouble(string text)
        {
            if (double.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double value))
                return value;
            return null;
        }
    }
}