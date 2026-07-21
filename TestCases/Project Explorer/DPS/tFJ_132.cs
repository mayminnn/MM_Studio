using Automation.Components;
using Automation.Core;
using Automation.Pages;
using FlaUI.Core.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Test_Cases.Attributes;
using Test_Cases.Login;

namespace Test_Cases.Project_Explorer.DPS
{
    [TestClass]
    [TestTags("Project_Explorer", "DPS", "Positive", "Automated")]
    public class tFJ_132 : BaseTest
    {
        private UIAssistant _assistant;
        private PSPage _psPage;
        private ProjectExplorerPane _projectExplorer;
        private DPSPage _dpsPage;

        [TestInitialize]
        public void Start()
        {
            _assistant = new UIAssistant(AppManager.Automation);

            var mainWindow = AppManager.MainWindow;

            _psPage = new PSPage(mainWindow, _assistant);
            _psPage.OpenDesignPage();

            var ribbon = new RibbonPane(mainWindow, _assistant);
            ribbon.ClickRibbonButton("Open Project");

            var openProjectWindow =
                _assistant.FindByAutomationId(mainWindow, "OpenProjectForm");

            var openDialog =
                new OpenProjectDialog(openProjectWindow, _assistant);

            openDialog.OpenProject(ProjectName, Version);

            _projectExplorer =
                new ProjectExplorerPane(mainWindow, _assistant);

            _projectExplorer.OpenPage("DPSSets");

            _dpsPage = new DPSPage(mainWindow, _assistant);
        }

        public void SetDPSSetModeFValue(int row, string dpsSet,
            string mode, string fvalue)
        {
            _dpsPage.ClearAndEnterCellValue($"B{row}", dpsSet);
            _dpsPage.EnterDropdownValue($"G{row}", mode);
            _dpsPage.ClearAndEnterCellValue($"H{row}", fvalue);

            Wait.UntilInputIsProcessed();

            Thread.Sleep(500);
        }

        [TestMethod]
        public void Verify_Automatic_FRange_Population()
        {
            int row = 4;

            var rules = new[]
            {
                new
                {
                    DPSSet = "POWER_I_SEQ_PAR",
                    Mode = "FI",
                    FValue = "0.00003",
                    AllowedRanges = new[]
                    {
                        "+-5uA",
                        "+-25uA",
                        "+-250uA",
                        "+-2.5mA",
                        "+-25mA",
                        "+-500mA",
                        "+-1.2A"
                    }
                },

                new
                {
                    DPSSet = "FIMI_SEQ_PAR",
                    Mode = "FIMI",
                    FValue = "0.008",
                    AllowedRanges = new[]
                    {
                        "+-5uA",
                        "+-25uA",
                        "+-250uA",
                        "+-2.5mA",
                        "+-25mA",
                        "+-500mA",
                        "+-1.2A"
                    }
                },

                new
                {
                    DPSSet = "FIMV_SEQ_PAR",
                    Mode = "FIMV",
                    FValue = "1",
                    AllowedRanges = new[]
                    {
                        "+-5uA",
                        "+-25uA",
                        "+-250uA",
                        "+-2.5mA",
                        "+-25mA",
                        "+-500mA",
                        "+-1.2A"
                    }
                },

                new
                {
                    DPSSet = "POWER_UP_SEQ_PAR",
                    Mode = "FV",
                    FValue = "-4",
                    AllowedRanges = new[]
                    {
                        "-4V~+20V"
                    }
                },

                new
                {
                    DPSSet = "FVMI_OPEN_PAR",
                    Mode = "FVMI",
                    FValue = "5",
                    AllowedRanges = new[]
                    {
                        "-4V~+20V"
                    }
                },

                new
                {
                    DPSSet = "POWER_OPEN_PAR",
                    Mode = "FVMV",
                    FValue = "20",
                    AllowedRanges = new[]
                    {
                        "-4V~+20V"
                    }
                },

                new
                {
                    DPSSet = "POWER_MI_PAR",
                    Mode = "MI",
                    FValue = "1",
                    AllowedRanges = Array.Empty<string>()
                },

                new
                {
                    DPSSet = "POWER_MV_PAR",
                    Mode = "MV",
                    FValue = "1",
                    AllowedRanges = Array.Empty<string>()
                }
            };

            foreach (var rule in rules)
            {
                System.Diagnostics.Debug.WriteLine("--------------------------------");
                System.Diagnostics.Debug.WriteLine($"Row      : {row}");
                System.Diagnostics.Debug.WriteLine($"DPS Set  : {rule.DPSSet}");
                System.Diagnostics.Debug.WriteLine($"Mode     : {rule.Mode}");
                System.Diagnostics.Debug.WriteLine($"FValue   : {rule.FValue}");
                System.Diagnostics.Debug.WriteLine("--------------------------------");

                // Set DPSSet + Mode
                SetDPSSetModeFValue(
                    row,
                    rule.DPSSet,
                    rule.Mode,
                    rule.FValue);

                // Read auto populated FRange
                string actualFRange = _dpsPage.GetFRangeValue(row);

                // MI / MV should remain blank
                if (rule.AllowedRanges.Length == 0)
                {
                    Assert.IsTrue(
                        string.IsNullOrEmpty(actualFRange),
                        $"Expected I{row} to be blank but got '{actualFRange}'");
                }
                else
                {
                    Assert.IsTrue(
                        rule.AllowedRanges.Contains(actualFRange),
                        $"Unexpected FRange '{actualFRange}' in I{row}");
                }

                System.Diagnostics.Debug.WriteLine(
                    $"PASS -> I{row} = '{actualFRange}'\n");

                row++;
            }

            System.Diagnostics.Debug.WriteLine("--------------------------------");
            System.Diagnostics.Debug.WriteLine("| FRANGE AUTO POPULATE PASSED |");
            System.Diagnostics.Debug.WriteLine("--------------------------------");

            //Cleanup();
        }
    }
}