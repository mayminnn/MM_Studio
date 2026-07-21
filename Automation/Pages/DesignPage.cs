using Automation.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using System;
using System.Linq;

namespace Automation.Pages
{
    public class DesignPage
    {
        private readonly Window _window;
        private readonly UIAssistant _assist;

        public DesignPage(AppManager app)
        {
            _window = app.MainWindow;
            _assist = new UIAssistant(app.Automation);
        }

        public void ClickDesign()
        {
            var btn = _assist.FindAs<Button>(_window, "btnDesign");
            btn.Invoke();
        }
    }
}
