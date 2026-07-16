using Automation.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using System;
using System.Windows;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace Automation.Components
{
    public class DatalogClient
    {
        private readonly AutomationElement _window;
        private readonly UIAssistant _assistant;

        public DatalogClient(AutomationElement window, UIAssistant assistant)
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

        private AutomationElement GetDatalogWindow()
        {
            var pnlxPage = _assistant.FindByAutomationId(_window, "pnlxPage");
            if (pnlxPage == null)
                throw new Exception("pnlxPage not found");

            var designUC = _assistant.FindByAutomationId(pnlxPage, "projectDesignUserControl");
            if (designUC == null)
                throw new Exception("projectDesignUserControl not found");

            var dockSite = _assistant.FindByAutomationId(designUC, "mainDocDockSite");
            if (dockSite == null)
                throw new Exception("mainDocDockSite not found");

            var dcWindow = _assistant.FindByAutomationId(dockSite, "DatalogClient");
            if (dcWindow == null)
                throw new Exception("DatalogClient window not found");

            var dcPane = _assistant.FindByName(dcWindow, "DatalogClient");
            if (dcPane == null)
                throw new Exception("DatalogClient pane not found");

            var richText = _assistant.FindByAutomationId(dcPane, "rtxtDataLog");
            if (richText == null)
                throw new Exception("rtxtDataLog not found");

            return richText;
        }

        public string GetDCResults()
        {
            var datalog = GetDatalogWindow();

            if (datalog == null)
                throw new Exception("Datalog window not found.");

            datalog.Focus();
            Thread.Sleep(200);

            Keyboard.TypeSimultaneously(
                VirtualKeyShort.CONTROL,
                VirtualKeyShort.KEY_A);

            Thread.Sleep(100);

            Keyboard.TypeSimultaneously(
                VirtualKeyShort.CONTROL,
                VirtualKeyShort.KEY_C);

            Thread.Sleep(300);

            string content = System.Windows.Forms.Clipboard.GetText();

            System.Diagnostics.Debug.WriteLine(content);

            return content;
        }

        public class TestResult
        {
            public string Name { get; set; }

            public string Flow { get; set; }

            public double LowLimit { get; set; }

            public double HighLimit { get; set; }

            public string Unit { get; set; }

            public List<double> SiteValues { get; set; }
                = new List<double>();
        }

        public class FlowResult
        {
            public string Name { get; set; }

            public List<TestResult> Tests { get; set; }
                = new List<TestResult>();
        }

        public class FinalSummary
        {
            public List<string> Status { get; set; }

            public List<int> HardBin { get; set; }

            public List<int> SoftBin { get; set; }

            public List<string> BinName { get; set; }
        }

        public class DatalogResult
        {
            public List<FlowResult> Flows
                = new List<FlowResult>();

            public FinalSummary Summary
                = new FinalSummary();
        }
    }
}
