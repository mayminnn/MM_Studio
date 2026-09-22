using Automation.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using Automation.Models;
using Automation.Parsers;
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
        private static readonly Mutex ClipboardMutex =
            new Mutex(false, "Global\\MM_Studio_ClipboardMutex");

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

            var dcWindow = _assistant.FindByAutomationId(dockSite, "Datalog");
            if (dcWindow == null)
                throw new Exception("DatalogClient window not found");

            var dcPane = _assistant.FindByAutomationId(dcWindow, "DatalogClientUserControl");
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

            Console.WriteLine("Datalog control found.");
            Console.WriteLine($"Name: {datalog.Name}");
            Console.WriteLine($"AutomationId: {datalog.AutomationId}");
            Console.WriteLine($"ControlType: {datalog.ControlType}");

            // Only one test's copy/paste-cycle runs at a time,
            // across all parallel MSTest threads.
            ClipboardMutex.WaitOne();
            try
            {
                datalog.Focus();
                Thread.Sleep(500);

                Mouse.Click(datalog.BoundingRectangle.Center());
                Thread.Sleep(500);

                Keyboard.Press(VirtualKeyShort.CONTROL);
                Keyboard.Press(VirtualKeyShort.KEY_A);
                Keyboard.Release(VirtualKeyShort.KEY_A);
                Keyboard.Release(VirtualKeyShort.CONTROL);
                Thread.Sleep(500);

                Keyboard.Press(VirtualKeyShort.CONTROL);
                Keyboard.Press(VirtualKeyShort.KEY_C);
                Keyboard.Release(VirtualKeyShort.KEY_C);
                Keyboard.Release(VirtualKeyShort.CONTROL);
                Thread.Sleep(1000);

                string content = GetClipboardTextSTA();

                Console.WriteLine($"Clipboard length: {content?.Length ?? 0}");

                return content;
            }
            finally
            {
                ClipboardMutex.ReleaseMutex();
            }
        }

        public DatalogResult CaptureAndSave(
    string projectName,
    string version,
    string testName)
        {
            string rawText = GetDCResults();

            if (string.IsNullOrWhiteSpace(rawText))
                throw new Exception("Datalog is empty; nothing to save.");

            var parser = new DatalogParser();
            DatalogResult result = parser.Parse(rawText);

            DatalogStorage.Save(projectName, version, testName, result, rawText);

            return result;
        }

        // Clipboard.GetText() requires an STA thread. MSTest's
        // [Parallelize] runs test methods on MTA thread-pool threads,
        // so we hop onto a dedicated STA thread just for the read.
        private static string GetClipboardTextSTA()
        {
            string result = null;
            Exception error = null;

            var thread = new Thread(() =>
            {
                try
                {
                    Retry.WhileTrue(() =>
                    {
                        result = Clipboard.GetText();
                        return string.IsNullOrEmpty(result);
                    }, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(200));
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (error != null)
                throw error;

            return result;
        }

        public DatalogResult GetParsedResults()
        {
            string text = GetDCResults();

            System.Diagnostics.Debug.WriteLine("");
            System.Diagnostics.Debug.WriteLine("========== RAW DATALOG ==========");
            System.Diagnostics.Debug.WriteLine(
                string.IsNullOrEmpty(text) ? "[EMPTY]" : text);
            System.Diagnostics.Debug.WriteLine("========== END RAW DATALOG ==========");

            if (string.IsNullOrWhiteSpace(text))
                throw new Exception("Datalog is empty.");

            var parser = new DatalogParser();

            return parser.Parse(text);
        }
    }
}