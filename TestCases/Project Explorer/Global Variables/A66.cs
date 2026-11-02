using Automation.Components;
using Automation.Core;
using Automation.Pages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Test_Cases.Attributes;
using System.Drawing;
using Test_Cases.Login;
using static System.Net.Mime.MediaTypeNames;

namespace Test_Cases.Project_Explorer.Global_Variables
{
    [TestClass]
    [TestTags("Project_Explorer", "Global_Variables", "Smoke", "Positive", "Automated")]
    [TestLinkIds("tFJ-66")]
    public class A66 : BaseTest
    {
        private UIAssistant _assistant = null!;
        private PSPage _psPage = null!;
        private ProjectExplorerPane _projectExplorer = null!;
        private ChannelMapPage _cmSheet = null!;
        private ScreenCapture _screenCapture = null!;

        [TestInitialize]
        public void Start()
        {
            _assistant = new UIAssistant(AppManager.Automation);
            var mainWindow = AppManager.MainWindow;

            _psPage = new PSPage(mainWindow, _assistant);
            _psPage.OpenDesignPage();

            var ribbon = new RibbonPane(mainWindow, _assistant);
            ribbon.ClickRibbonButton("Open Project");

            var openProjectWindow = _assistant.FindByAutomationId(mainWindow, "OpenProjectForm");
            var openDialog = new OpenProjectDialog(openProjectWindow, _assistant);
            openDialog.OpenProject(ProjectName, Version);

            _projectExplorer = new ProjectExplorerPane(mainWindow, _assistant);
            _projectExplorer.OpenPage("Channel Maps");

            _cmSheet = new ChannelMapPage(mainWindow, _assistant);
        }

        // Global variables defined by A68, referenced from the channel map's Pkg Pin column (C),
        // and the value each cell shows once the reference resolves
        private static readonly (int Row, string PinName, string Formula, string ExpectedValue)[] References =
        {
            (5, "ABC",  "=_BuildVersion", "1"),       // BuildVersion = 1.0
            (6, "SDA",  "=_Delay",        "200*ms"),  // Delay = 200*ms
            (7, "BOOT", "=_BootVoltage",  "3.4"),     // BootVoltage = 3.4
        };

        [TestMethod]
        public void Reference_Global_Variable_in_Another_Sheets()
        {
            var assistant = new UIAssistant(AppManager.Automation);
            var cmSheet = new ChannelMapPage(AppManager.MainWindow, assistant);

            // Only the pin name and the reference: the check is about the value it resolves to
            foreach (var reference in References)
            {
                cmSheet.ClearAndEnterCellValue($"B{reference.Row}", reference.PinName);
                cmSheet.ClearAndEnterCellValue($"C{reference.Row}", reference.Formula);
            }

            var results = new System.Collections.Generic.List<(string Cell, string Formula, string FoundFormula, string Value, string Expected, bool Passed)>();

            foreach (var reference in References)
            {
                string cell = $"C{reference.Row}";

                // The cell keeps the reference: the formula bar shows the formula as typed
                var formulaBar = cmSheet.GetFormulaBarTexts(cell);

                string foundFormula = formulaBar.FirstOrDefault(t =>
                    string.Equals(t, reference.Formula, StringComparison.OrdinalIgnoreCase))
                    ?? $"(not found; formula bar shows [{string.Join(" | ", formulaBar)}])";

                // ...and the cell resolves it to the global variable's value
                string value = cmSheet.GetCellValue(cell);

                bool passed = foundFormula.StartsWith("=") && SameValue(value, reference.ExpectedValue);

                results.Add((cell, reference.Formula, foundFormula, value, reference.ExpectedValue, passed));
            }

            Console.WriteLine();
            Console.WriteLine("========== GLOBAL VARIABLE REFERENCES ==========");

            foreach (var r in results)
            {
                Console.WriteLine($"{r.Cell} {(r.Passed ? "PASS" : "FAIL")}");
                Console.WriteLine($"    Formula bar : {r.FoundFormula}");
                Console.WriteLine($"    Cell value  : '{r.Value}' (expected '{r.Expected}')");
            }

            Console.WriteLine("================================================");

            Assert.IsFalse(results.Any(r => !r.Passed),
                "Global variable references did not resolve as expected: " +
                string.Join(", ", results.Where(r => !r.Passed).Select(r => $"{r.Cell} {r.Formula} = '{r.Value}' (expected '{r.Expected}')")));

            Cleanup();
        }

        // "1" and "1.0" are the same value; anything that isn't a number must match exactly
        private static bool SameValue(string actual, string expected)
        {
            var style = System.Globalization.NumberStyles.Any;
            var culture = System.Globalization.CultureInfo.InvariantCulture;

            if (double.TryParse(actual, style, culture, out double a) &&
                double.TryParse(expected, style, culture, out double e))
                return a == e;

            return string.Equals(actual?.Trim(), expected, StringComparison.OrdinalIgnoreCase);
        }

        // [TestMethod]
        public void Verify_Screenshot_Text_Should_Contain_Value()
        {
            _cmSheet.ConfigureCMRow(5, "ABC", "=_BuildVersion", "Gnd", "");
            _cmSheet.ConfigureCMRow(6, "SDA", "=_TestVersion", "IO", "");

            _screenCapture = new ScreenCapture();

            string row5Path = _screenCapture.ElementCapture(_cmSheet.GetCMRow(5));
            //string row6Path = _screenCapture.ElementCapture(_cmSheet.GetCMRow(6));

            var ocr = new OCRhelper(@"C:\Aemulus\techFlowJazz\bin\Debug\tessdata");

            bool row5Valid = ocr.ContainsText(row5Path, "200");
            //bool row6Valid = ocr.ContainsText(row6Path, "2.2");

            Assert.IsTrue(row5Valid, "Row 5 does not contain '1'");
            //Assert.IsTrue(row6Valid, "Row 6 does not contain '2.2'");
        }
    }
}
