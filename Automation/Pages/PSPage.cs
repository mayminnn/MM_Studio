using Automation.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using System;

namespace Automation.Pages
{
    public class PSPage
    {
        private readonly Window _window;
        private readonly UIAssistant _assistant;

        public PSPage(Window window, UIAssistant assistant)
        {
            _window = window;
            _assistant = assistant;
        }

        public void OpenDesignPage()
        {
            var mainChrome = _assistant.FindByAutomationId(_window, "mainChrome");
            if (mainChrome == null)
                throw new Exception("Main panel 'mainChrome' not found");

            var designButton = _assistant.FindByName(mainChrome, "Design")?.AsButton();
            if (designButton == null)
                throw new Exception("Design button not found");

            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(300);

            designButton.Focus();
            designButton.Click();

            System.Diagnostics.Debug.WriteLine("Clicked Design\n");
        }
    }
}