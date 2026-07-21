using FlaUI.Core.Tools;
using FlaUI.Core.AutomationElements;
using System;
using System.Linq;
using FlaUI.Core.Input;

namespace Automation.Core
{
    public class oldUIHelper
    {
        public AutomationElement Find(AutomationElement parent, string key, int timeout = 10)
        {
            return Retry.WhileNull(() =>
            {
                Wait.UntilInputIsProcessed();

                return parent.FindAllDescendants()
                    .FirstOrDefault(e =>
                        e.AutomationId == key ||
                        e.Name == key);

            }, TimeSpan.FromSeconds(timeout)).Result;
        }
    }

    public class UIHelper
    {
        public AutomationElement Find(
        AutomationElement parent,
        string key,
        int timeout = 10)
            {
                return Retry.WhileNull(() =>
                {
                    Wait.UntilInputIsProcessed();

                    return parent.FindAllDescendants()
                        .FirstOrDefault(e =>
                        {
                            try
                            {
                                return e.AutomationId == key ||
                                       e.Name == key;
                            }
                            catch
                            {
                                return e.Name == key;
                            }
                        });

                }, TimeSpan.FromSeconds(timeout)).Result;
        }
    }
}
