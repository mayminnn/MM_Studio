using Automation.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using System;
using System.Linq;
using System.Threading;
using System.Drawing;

namespace Automation.Components
{
    public class ChannelMapPage
    {
        private readonly AutomationElement _window;
        private readonly UIAssistant _assistant;

        public ChannelMapPage(AutomationElement window, UIAssistant assistant)
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
            var CMDefault = _assistant.FindByName(bar, "Channel Map=Default");
            var sheetEditor = _assistant.FindByAutomationId(CMDefault, "ChannelMapSheetEditor");

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

        public void ValidateTypeDropdowns(int startRow = 4, int endRow = 10)
        {
            string[] expectedItems = { "DPS", "Gnd", "IO", "PMU", "Utility" };

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

        public AutomationElement GetCMRow(int row)
        {
            //SelectCellByFormula($"C{row - 3}");
            //Thread.Sleep(200);

            SelectCellByFormula($"C{row}");
            Thread.Sleep(200);

            return GetSheetEditor();
        }

        public void ConfigureCMRow(int row, string pinName, string pkgPin,
        string type, string remark)
        {
            ClearAndEnterCellValue($"B{row}", pinName);
            ClearAndEnterCellValue($"C{row}", pkgPin);
            EnterDropdownValue($"D{row}", type);
            ClearAndEnterCellValue($"E{row}", remark);
        }

        //private void ClickDropdownButtonByCell(string cellName)
        //{
        //    SelectCellByFormula(cellName);
        //    Thread.Sleep(200);

        //    var workbook = _assistant.FindByAutomationId(_window, "workbookView");

        //    if (workbook == null)
        //        throw new Exception("WorkbookView not found");

        //    var rect = workbook.BoundingRectangle;

        //    char columnLetter = cellName[0];
        //    int rowNumber = int.Parse(cellName.Substring(1));

        //    const int columnWidth = 85;
        //    const int rowHeight = 29;
        //    const int startXOffset = 30;
        //    const int startYOffset = 25;

        //    int columnIndex = columnLetter - 'A';

        //    int x = (int)(rect.Left + startXOffset + (columnIndex * columnWidth) + columnWidth - 8);
        //    int y = (int)(rect.Top + startYOffset + ((rowNumber - 1) * rowHeight) + rowHeight / 2);

        //    var clickPoint = new System.Drawing.Point(x, y);

        //    Mouse.MoveTo(clickPoint);
        //    Thread.Sleep(100);

        //    Mouse.Click();

        //    Thread.Sleep(500);

        //    System.Diagnostics.Debug.WriteLine($"Clicked dropdown at {cellName} ({x},{y})");
        //}

        private void ClickDropdownButtonByCell(string cellName)
        {
            var workbook = _assistant.FindByAutomationId(_window, "workbookView");
            if (workbook == null)
                throw new Exception("WorkbookView not found");

            var rect = workbook.BoundingRectangle;

            char columnLetter = cellName[0];
            int rowNumber = int.Parse(cellName.Substring(1));

            const int columnWidth = 85;
            const int startXOffset = 10;
            const int startYOffset = -10;
            const double rowHeight = 28.5;

            int columnIndex = columnLetter - 'A';

            // If rowNumber > 1, subtract 1 from rowNumber for consistent row height calculation
            int effectiveRow = rowNumber > 1 ? rowNumber - 1 : 0;

            int x = (int)(rect.Left + startXOffset + (columnIndex * columnWidth) + columnWidth - 8);
            int y = (int)(rect.Top + startYOffset + (effectiveRow * rowHeight) + rowHeight / 2);

            var clickPoint = new System.Drawing.Point(x, y);

            Mouse.MoveTo(clickPoint);
            Thread.Sleep(100);
            Mouse.Click();
            Thread.Sleep(500);

            System.Diagnostics.Debug.WriteLine($"Clicked dropdown at {cellName} ({x},{y})");
        }

        private void EnterDropdownValue(string cell, string value)
        {
            SelectCellByFormula(cell);
            Thread.Sleep(300);

            ClickDropdownButtonByCell(cell);

            var items = GetVisibleDropdownItems();

            if (!items.Contains(value))
                throw new Exception($"{value} not found in {cell}");

            ClickDropdownItem(value);

            Wait.UntilInputIsProcessed();
            Thread.Sleep(300);

            System.Diagnostics.Debug.WriteLine($"{cell} = {value}");
        }

        private void ClickDropdownItem(string itemName)
        {
            var dropdown = Retry.WhileNull(() =>
                _window.FindAllDescendants(cf => cf.ByName("DropDown"))
                       .FirstOrDefault(e => !e.IsOffscreen),
                TimeSpan.FromSeconds(5)).Result;

            if (dropdown == null)
                throw new Exception("Dropdown container not found");

            var list = dropdown.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.List));

            if (list == null)
                throw new Exception("Dropdown list not found");

            var item = list.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.ListItem)
                  .And(cf.ByName(itemName)));

            if (item == null)
                throw new Exception($"{itemName} not found in dropdown");

            item.Click();

            Thread.Sleep(300);

            System.Diagnostics.Debug.WriteLine($"Clicked dropdown item: {itemName}");
        }

        private AutomationElement GetSheetEditor()
        {
            var pnlxPage = _assistant.FindByAutomationId(_window, "pnlxPage");
            var designUC = _assistant.FindByAutomationId(pnlxPage, "projectDesignUserControl");
            var dockSite = _assistant.FindByAutomationId(designUC, "mainDocDockSite");
            var bar = _assistant.FindByAutomationId(dockSite, "Document");
            var binDefault = _assistant.FindByName(bar, "Channel Map=Default");
            return _assistant.FindByAutomationId(binDefault, "ChannelMapSheetEditor");
        }

        public void ClickValidate()
        {
            //var pnlxPage = _assistant.FindByAutomationId(_window, "pnlxPage");
            //var designUC = _assistant.FindByAutomationId(pnlxPage, "projectDesignUserControl");
            //var dockSite = _assistant.FindByAutomationId(designUC, "mainDocDockSite");
            //var bar = _assistant.FindByAutomationId(dockSite, "Document");
            //var binDefault = _assistant.FindByName(bar, "Channel Map=Default");
            //var sheetEditor = _assistant.FindByAutomationId(binDefault, "ChannelMapSheetEditor");
            var sheetEditor = GetSheetEditor();
            var validateBtn = _assistant.FindByName(sheetEditor, "Validate");

            if (validateBtn == null)
                throw new Exception("Validate button not found");

            validateBtn.AsButton().Invoke();

            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(1000);

            System.Diagnostics.Debug.WriteLine("Clicked Validate");
        }

        public void ClickSync()
        {
            var sheetEditor = GetSheetEditor();
            var syncBtn = _assistant.FindByName(sheetEditor, "Sync");

            if (syncBtn == null)
                throw new Exception("Sync button not found");

            syncBtn.AsButton().Invoke();

            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(1000);

            System.Diagnostics.Debug.WriteLine("Clicked Sync");
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