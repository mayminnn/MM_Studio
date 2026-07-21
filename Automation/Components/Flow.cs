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
    public class Flow
    {
        private readonly AutomationElement _window;
        private readonly UIAssistant _assistant;

        public Flow(AutomationElement window, UIAssistant assistant)
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

        private AutomationElement GetSheetEditor()
        {
            var pnlxPage = _assistant.FindByAutomationId(_window, "pnlxPage");
            var designUC = _assistant.FindByAutomationId(pnlxPage, "projectDesignUserControl");
            var dockSite = _assistant.FindByAutomationId(designUC, "mainDocDockSite");
            var bar = _assistant.FindByAutomationId(dockSite, "Document");
            var Flowdefault = _assistant.FindByName(bar, "Flow=Default");
            return _assistant.FindByAutomationId(Flowdefault, "FlowSheetEditor");
        }

        private AutomationElement GetFormulaBar()
        {
            var sheetEditor = GetSheetEditor();

            return _assistant.FindByAutomationId(sheetEditor, "formulaBar");
        }

        private AutomationElement SelectCellByFormula(string cellName)
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

            return GetSheetEditor();
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

            var menu = Retry.WhileNull(() =>
                _assistant.FindByName(_window, "Fail Bins"),
                TimeSpan.FromSeconds(5)).Result;

            if (menu == null)
                throw new Exception("Fail Bins menu not found.");

            menu.Click();

            Thread.Sleep(500);

            var dropDown = Retry.WhileNull(() =>
                _assistant.FindByName(_window, "Fail BinsDropDown"),
                TimeSpan.FromSeconds(5)).Result;

            if (dropDown == null)
                throw new Exception("Fail BinsDropDown not found.");

            var items = dropDown.FindAllDescendants(cf =>
                    cf.ByControlType(ControlType.MenuItem))
                .Select(x => x.Name)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            return items;
        }

        public List<string> GetFailBinsFromPicker(int row)
        {
            var sheet = SelectCellByFormula($"O{row}");

            Mouse.MoveTo(sheet.BoundingRectangle.Center());
            Thread.Sleep(200);
            Mouse.Click(MouseButton.Right);

            Thread.Sleep(300);

            var failBins =
                _assistant.FindByName(_window, "Fail Bins");

            failBins.Click();

            Thread.Sleep(300);

            var pickBin =
                _assistant.FindByName(_window, "Pick Fail Bin...");

            if (pickBin == null)
                throw new Exception("Pick Fail Bin... not found.");

            pickBin.Click();

            Thread.Sleep(500);

            var picker =
                _assistant.FindByAutomationId(_window, "BinPickerForm");

            if (picker == null)
                throw new Exception("BinPickerForm not found.");

            var list =
                _assistant.FindByAutomationId(picker, "listBoxBins");

            if (list == null)
                throw new Exception("listBoxBins not found.");

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

            if (validateBtn == null)
                throw new Exception("Validate button not found");

            validateBtn.AsButton().Invoke();

            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(1000);

            System.Diagnostics.Debug.WriteLine("Clicked Validate");
        }

        public void ClickStartDebug()
        {
            var sheetEditor = GetSheetEditor();

            var startDebugGroup = _assistant.FindByName(sheetEditor, "Start Debug");
            if (startDebugGroup == null)
                throw new Exception("Start Debug group not found");

            var startDebugButton = startDebugGroup.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.SplitButton));

            if (startDebugButton == null)
                throw new Exception("Start Debug SplitButton not found");

            startDebugButton.AsButton().Invoke();

            Wait.UntilInputIsProcessed();
            Thread.Sleep(1000);

            System.Diagnostics.Debug.WriteLine("Clicked Start Debug");
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
