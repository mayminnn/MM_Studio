using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using System;

namespace Automation.Core
{
    public class UIAssistant
    {
        //public ConditionFactory ConditionFactory => _cf;
        private readonly ConditionFactory _cf;

        public object FocusedElement { get; set; }

        public UIAssistant(UIA3Automation automation)
        {
            _cf = new ConditionFactory(new UIA3PropertyLibrary());
        }

        // public AutomationElement FindByAutomationId(AutomationElement parent, string automationId, int timeout = 10)
        // {
        //     return Retry.WhileNull(() =>
        //     {
        //         return parent.FindFirstDescendant(_cf.ByAutomationId(automationId));
        //     }, TimeSpan.FromSeconds(timeout)).Result;
        // }

        public AutomationElement FindByAutomationId(AutomationElement parent, string automationId,
    int timeout = 10)
        {
            return Retry.WhileNull(() =>
            {
                if (parent == null)
                    return null;

                return parent.FindFirstDescendant(
                    _cf.ByAutomationId(automationId));

            }, TimeSpan.FromSeconds(timeout)).Result;
        }

        public AutomationElement FindByName(AutomationElement parent, string name, int timeout = 10)
        {
            return Retry.WhileNull(() =>
            {
                return parent.FindFirstDescendant(_cf.ByName(name));
            }, TimeSpan.FromSeconds(timeout)).Result;
        }

        public AutomationElement FindByControlType(AutomationElement parent, FlaUI.Core.Definitions.ControlType controlType, int timeout = 10)
        {
            return Retry.WhileNull(() =>
                parent.FindFirstDescendant(_cf.ByControlType(controlType)),
                TimeSpan.FromSeconds(timeout)).Result;
        }

        public T FindAs<T>(AutomationElement parent, string automationId, int timeout = 10) where T : AutomationElement
        {
            var element = FindByAutomationId(parent, automationId, timeout);

            if (element == null)
                throw new Exception($"Element with AutomationId '{automationId}' not found");

            return element.As<T>();
        }
    }
}
