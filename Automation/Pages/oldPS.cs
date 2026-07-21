using Automation.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using System;
using System.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Automation.Pages
{
    public class oldPSPage
    {
        private readonly Window _window;
        private readonly UIHelper _helper;

        public oldPSPage(Window window)
        {
            _window = window;
            _helper = new UIHelper();
        }

        // STEP 3: Click Design
        public void OpenDesignPage()
        {
            var mainChrome = _helper.Find(_window, "mainChrome");

            if (mainChrome == null)
                throw new Exception("main panel not found");

            var design = mainChrome.FindAllDescendants()
                .FirstOrDefault(e => e.Name.Contains("Design"))?.AsButton();

            if (design == null)
                throw new Exception("Design button not found");

            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(300);

            design.Focus();
            design.Click();

            System.Diagnostics.Debug.WriteLine("Clicked Design");
        }
    }
}