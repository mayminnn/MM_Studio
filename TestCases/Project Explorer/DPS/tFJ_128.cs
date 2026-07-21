using Automation.Components;
using Automation.Core;
using Automation.Pages;
using FlaUI.Core.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test_Cases.Attributes;
using Test_Cases.Login;

namespace Test_Cases.Project_Explorer.DPS
{
    [TestClass]
    [TestTags("Project_Explorer", "DPS", "Positive", "Automated")]
    public class tFJ_128 : BaseTest
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

        public void SetDPSSetSequenceAndMode(int row, string dpsSet,
            string sequence, string mode)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Row {row} | Sequence={sequence} | Mode={mode}");

            _dpsPage.ClearAndEnterCellValue($"B{row}", dpsSet);
            _dpsPage.EnterDropdownValue($"C{row}", sequence);
            _dpsPage.EnterDropdownValue($"G{row}", mode);

            Wait.UntilInputIsProcessed();

            Thread.Sleep(500);
        }

        [TestMethod]
        public void Flow_Auto_Assignment_Based_on_Sequence_and_Mode()
        {
            int row = 4;

            var rules = new[]
            {
                new { DPSSet = "POWER_I_SEQ_PAR",  Mode = "FI",   Sequence = "Parallel", ExpectedFlow = "Force->On" },
                new { DPSSet = "POWER_I_SEQ_SER",  Mode = "FI",   Sequence = "Serial",   ExpectedFlow = "Force->On->Delay2" },

                new { DPSSet = "FIMI_SEQ_PAR",     Mode = "FIMI", Sequence = "Parallel", ExpectedFlow = "Force->On->Delay1->Meas" },
                new { DPSSet = "FIMI_SEQ_SER",     Mode = "FIMI", Sequence = "Serial",   ExpectedFlow = "Force->On->Delay1->Meas->Delay2" },

                new { DPSSet = "FIMV_SEQ_PAR",     Mode = "FIMV", Sequence = "Parallel", ExpectedFlow = "Force->On->Delay1->Meas" },
                new { DPSSet = "FIMV_SEQ_SER",     Mode = "FIMV", Sequence = "Serial",   ExpectedFlow = "Force->On->Delay1->Meas->Delay2" },

                new { DPSSet = "POWER_UP_SEQ_PAR", Mode = "FV",   Sequence = "Parallel", ExpectedFlow = "Force->On" },
                new { DPSSet = "POWER_UP_SEQ_SER", Mode = "FV",   Sequence = "Serial",   ExpectedFlow = "Force->On->Delay2" },

                new { DPSSet = "FVMI_OPEN_PAR",    Mode = "FVMI", Sequence = "Parallel", ExpectedFlow = "Force->On->Delay1->Meas" },
                new { DPSSet = "FVMI_OPEN_SER",    Mode = "FVMI", Sequence = "Serial",   ExpectedFlow = "Force->On->Delay1->Meas->Delay2" },

                new { DPSSet = "POWER_OPEN_PAR",   Mode = "FVMV", Sequence = "Parallel", ExpectedFlow = "Force->On->Delay1->Meas" },
                new { DPSSet = "POWER_OPEN_SER",   Mode = "FVMV", Sequence = "Serial",   ExpectedFlow = "Force->On->Delay1->Meas->Delay2" },

                new { DPSSet = "POWER_MI_PAR",     Mode = "MI",   Sequence = "Parallel", ExpectedFlow = "Meas" },
                new { DPSSet = "POWER_MI_SER",     Mode = "MI",   Sequence = "Serial",   ExpectedFlow = "Meas->Delay2" },

                new { DPSSet = "POWER_MV_PAR",     Mode = "MV",   Sequence = "Parallel", ExpectedFlow = "Meas" },
                new { DPSSet = "POWER_MV_SER",     Mode = "MV",   Sequence = "Serial",   ExpectedFlow = "Meas->Delay2" }
            };

            foreach (var rule in rules)
            {
                System.Diagnostics.Debug.WriteLine("--------------------------------");
                System.Diagnostics.Debug.WriteLine($"Row      : {row}");
                System.Diagnostics.Debug.WriteLine($"DPS Set  : {rule.DPSSet}");
                System.Diagnostics.Debug.WriteLine($"Mode     : {rule.Mode}");
                System.Diagnostics.Debug.WriteLine($"Sequence : {rule.Sequence}");
                System.Diagnostics.Debug.WriteLine($"Expected : {rule.ExpectedFlow}");
                System.Diagnostics.Debug.WriteLine("--------------------------------");

                SetDPSSetSequenceAndMode(
                    row,
                    rule.DPSSet,
                    rule.Sequence,
                    rule.Mode);

                string actualFlow = _dpsPage.GetFlowValue(row);

                Assert.AreEqual(
                    rule.ExpectedFlow,
                    actualFlow,
                    $"Flow mismatch at D{row}");

                System.Diagnostics.Debug.WriteLine(
                    $"\nFLOW MATCHED -> D{row} = {actualFlow}\n");

                row++;
            }

            System.Diagnostics.Debug.WriteLine("-------------------------------");
            System.Diagnostics.Debug.WriteLine("| FLOW RULE VALIDATION PASSED |");
            System.Diagnostics.Debug.WriteLine("------------------------------");

            //Cleanup();
        }
    }
}