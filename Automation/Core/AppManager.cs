using FlaUI.Core.Input;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using System.Diagnostics;

namespace Automation.Core
{
    public class AppManager
    {
        public Application App { get; private set; }
        public UIA3Automation Automation { get; private set; }
        public Window MainWindow { get; private set; }

        public void Launch()
        {
            Automation = new UIA3Automation();

            var startInfo = new ProcessStartInfo()
            {
                FileName = @"C:\Aemulus\techFlowJazz\bin\techFlowJazz.exe",
                WorkingDirectory = @"C:\Aemulus\techFlowJazz\bin"
            };

            App = Application.Launch(startInfo);

            MainWindow = Retry.WhileNull(() =>
            {
                var windows = App.GetAllTopLevelWindows(Automation);
                return windows.FirstOrDefault(w => w.FindAllDescendants().Length > 20);
            }, TimeSpan.FromSeconds(30)).Result;

            MainWindow.Focus();
            Wait.UntilInputIsProcessed();
        }

        public void Close()
        {
            App?.Close();
            Automation?.Dispose();
        }
    }
}
