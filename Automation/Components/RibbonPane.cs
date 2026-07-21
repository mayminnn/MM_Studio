using Automation.Core;
using FlaUI.Core.AutomationElements;
using System;

namespace Automation.Components
{
    public class RibbonPane
    {
        private readonly AutomationElement _root;
        private readonly UIAssistant _assistant;

        public RibbonPane(AutomationElement root, UIAssistant assistant)
        {
            if (root == null)
            {
                throw new ArgumentNullException("root");
            }

            if (assistant == null)
            {
                throw new ArgumentNullException("assistant");
            }

            _root = root;
            _assistant = assistant;
        }

        public void ClickRibbonButton(string buttonName)
        {
            var ribbonControl = _assistant.FindByAutomationId(_root, "ribbonControl");
            if (ribbonControl == null)
                throw new Exception("ribbonControl not found");

            var ribbonPanel = _assistant.FindByAutomationId(ribbonControl, "ribbonPanel");
            if (ribbonPanel == null)
                throw new Exception("ribbonPanel not found");

            var button = _assistant.FindByName(ribbonPanel, buttonName)?.AsButton();
            if (button == null)
                throw new Exception($"Button '{buttonName}' not found");

            button.Focus();
            button.Click();
        }
    }
}