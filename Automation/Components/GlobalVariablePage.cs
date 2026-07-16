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
    public class GlobalVariablePage
    {
        private readonly AutomationElement _window;
        private readonly UIAssistant _assistant;

        public GlobalVariablePage(AutomationElement window, UIAssistant assistant)
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

        private AutomationElement GetGVFormulaBar()
        {
            var pnlxPage = _assistant.FindByAutomationId(_window, "pnlxPage");
            var designUC = _assistant.FindByAutomationId(pnlxPage, "projectDesignUserControl");
            var dockSite = _assistant.FindByAutomationId(designUC, "mainDocDockSite");
            var bar = _assistant.FindByName(dockSite, "DotNetBar Bar");
            var GVDefault = _assistant.FindByName(bar, "Global Variable=Default");
            var sheetEditor = _assistant.FindByAutomationId(GVDefault, "GlobalVariableSheetEditor");

            return _assistant.FindByAutomationId(sheetEditor, "formulaBar");
        }

        private void SelectGVCellByFormula(string cellName)
        {
            var formulaBar = GetGVFormulaBar();

            var combo = formulaBar.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.ComboBox));

            if (combo == null)
                throw new Exception("Formula bar ComboBox not found");

            var edit = combo.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.Edit));

            if (edit == null)
                throw new Exception("Formula bar Edit not found");

            var textBox = edit.AsTextBox();

            // Always focus formula bar first
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

        public Dictionary<string, string> GetAllGlobalVariables()
        {
            var result = new Dictionary<string, string>();
            int row = 4;

            while (true)
            {
                string varName = GetCellValue($"B{row}");
                string varValue = GetCellValue($"C{row}");

                if (string.IsNullOrEmpty(varName))
                    break; // stop when empty name

                result[varName] = varValue;
                row++;
            }

            return result;
        }

        private string GetCellValue(string cell)
        {
            var formulaBar = GetGVFormulaBar();

            Keyboard.Type(cell);
            Keyboard.Press(VirtualKeyShort.ENTER);
            Thread.Sleep(200);

            // Now the formula bar value contains the cell content
            var combo = formulaBar.FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.ComboBox));
            var edit = combo.FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit));
            var textBox = edit.AsTextBox();
            return textBox.Text;
        }

        public void ConfigureGVRow(int row, string variableName, string GVvalue)
        {
            ClearAndEnterCellValue($"B{row}", variableName);
            ClearAndEnterCellValue($"C{row}", GVvalue);
        }

        private void ClearAndEnterCellValue(string cellName, string value)
        {
            SelectGVCellByFormula(cellName);

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

        public void ClickValidate()
        {
            var pnlxPage = _assistant.FindByAutomationId(_window, "pnlxPage");
            var designUC = _assistant.FindByAutomationId(pnlxPage, "projectDesignUserControl");
            var dockSite = _assistant.FindByAutomationId(designUC, "mainDocDockSite");
            var bar = _assistant.FindByAutomationId(dockSite, "Document");
            var GVDefault = _assistant.FindByName(bar, "Global Variable=Default");
            var sheetEditor = _assistant.FindByAutomationId(GVDefault, "GlobalVariableSheetEditor");

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
