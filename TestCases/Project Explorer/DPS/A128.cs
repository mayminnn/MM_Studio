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
    [TestLinkIds("tFJ-128")]
    public class A128 : BaseTest
    {
        private UIAssistant _assistant = null!;
        private PSPage _psPage = null!;
        private ProjectExplorerPane _projectExplorer = null!;
        private DPSPage _dpsPage = null!;

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
            Console.WriteLine(
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
                Console.WriteLine("--------------------------------");
                Console.WriteLine($"Row      : {row}");
                Console.WriteLine($"DPS Set  : {rule.DPSSet}");
                Console.WriteLine($"Mode     : {rule.Mode}");
                Console.WriteLine($"Sequence : {rule.Sequence}");
                Console.WriteLine($"Expected : {rule.ExpectedFlow}");
                Console.WriteLine("--------------------------------");

                SetDPSSetSequenceAndMode(
                    row,
                    rule.DPSSet,
                    rule.Sequence,
                    rule.Mode);

                string actualFlow = _dpsPage.GetFlowValue(row);

                if (actualFlow != rule.ExpectedFlow)
                {
                    // Read back the inputs so the failure shows whether
                    // Sequence/Mode were actually committed to the row
                    Assert.Fail(
                        $"Flow mismatch at D{row}. Expected '{rule.ExpectedFlow}', got '{actualFlow}'. " +
                        $"Row state: B{row}='{_dpsPage.GetCellValue($"B{row}")}', " +
                        $"C{row}='{_dpsPage.GetCellValue($"C{row}")}', " +
                        $"G{row}='{_dpsPage.GetCellValue($"G{row}")}'");
                }

                Console.WriteLine(
                    $"\nFLOW MATCHED -> D{row} = {actualFlow}\n");

                row++;
            }

            Console.WriteLine("-------------------------------");
            Console.WriteLine("| FLOW RULE VALIDATION PASSED |");
            Console.WriteLine("------------------------------");

            //Cleanup();
        }
    }
}