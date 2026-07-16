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
    public class tFJ_133 : BaseTest
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

        public void SetDPSSetModeClamp(int row, string dpsSet,
            string mode, string clamp)
        {
            _dpsPage.ClearAndEnterCellValue($"B{row}", dpsSet);
            _dpsPage.EnterDropdownValue($"G{row}", mode);
            _dpsPage.ClearAndEnterCellValue($"J{row}", clamp);

            Wait.UntilInputIsProcessed();

            Thread.Sleep(500);
        }

        [TestMethod]
        public void Verify_Automatic_CRange_Population()
        {
            int row = 4;

            var rules = new[]
            {
                new
                {
                    DPSSet = "POWER_I_SEQ_PAR",
                    Mode = "FI",
                    Clamp = "-4",
                    AllowedRanges = new[]
                    {
                        "-4V~+20V"
                    }
                },

                new
                {
                    DPSSet = "FIMI_SEQ_PAR",
                    Mode = "FIMI",
                    Clamp = "5",
                    AllowedRanges = new[]
                    {
                        "-4V~+20V"
                    }
                },

                new
                {
                    DPSSet = "FIMV_SEQ_PAR",
                    Mode = "FIMV",
                    Clamp = "20",
                    AllowedRanges = new[]
                    {
                        "-4V~+20V"
                    }
                },

                new
                {
                    DPSSet = "POWER_UP_SEQ_PAR",
                    Mode = "FV",
                    Clamp = "0.00002",
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
                    DPSSet = "FVMI_OPEN_PAR",
                    Mode = "FVMI",
                    Clamp = "0.0085",
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
                    DPSSet = "POWER_OPEN_PAR",
                    Mode = "FVMV",
                    Clamp = "1",
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
                    DPSSet = "POWER_MI_PAR",
                    Mode = "MI",
                    Clamp = "1",
                    AllowedRanges = Array.Empty<string>()
                },

                new
                {
                    DPSSet = "POWER_MV_PAR",
                    Mode = "MV",
                    Clamp = "6",
                    AllowedRanges = Array.Empty<string>()
                }
            };

            foreach (var rule in rules)
            {
                System.Diagnostics.Debug.WriteLine("--------------------------------");
                System.Diagnostics.Debug.WriteLine($"Row      : {row}");
                System.Diagnostics.Debug.WriteLine($"DPS Set  : {rule.DPSSet}");
                System.Diagnostics.Debug.WriteLine($"Mode     : {rule.Mode}");
                System.Diagnostics.Debug.WriteLine($"Clamp    : {rule.Clamp}");
                System.Diagnostics.Debug.WriteLine("--------------------------------");

                // Set DPSSet + Mode
                SetDPSSetModeClamp(
                    row,
                    rule.DPSSet,
                    rule.Mode,
                    rule.Clamp);

                Thread.Sleep(500);

                string actualCRange = _dpsPage.GetCRangeValue(row);

                // MI / MV should remain blank
                if (rule.AllowedRanges.Length == 0)
                {
                    Assert.IsTrue(
                        string.IsNullOrEmpty(actualCRange),
                        $"Expected K{row} to be blank but got '{actualCRange}'");
                }
                else
                {
                    Assert.IsTrue(
                        rule.AllowedRanges.Contains(actualCRange),
                        $"Unexpected CRange '{actualCRange}' in K{row}");
                }

                System.Diagnostics.Debug.WriteLine(
                    $"PASS -> K{row} = '{actualCRange}'\n");

                row++;
            }

            System.Diagnostics.Debug.WriteLine("--------------------------------");
            System.Diagnostics.Debug.WriteLine("| CRANGE AUTO POPULATE PASSED |");
            System.Diagnostics.Debug.WriteLine("--------------------------------");

            Cleanup();
        }
    }
}