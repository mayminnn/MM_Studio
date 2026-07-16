using Automation.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Capturing;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using Tesseract;

namespace Automation.Components
{
    public class ScreenCapture
    {
        private string folderPath = @"C:\Aemulus\techFlowJazz\projects";

        public void FullScreenCapture()
        {
            var fullscreenimg = Capture.Screen();

            string fileName = $"FullSC_{DateTime.Now:yyyyMMdd_HHmmssfff}.png";
            string fullPath = Path.Combine(folderPath, fileName);

            fullscreenimg.ToFile(fullPath);

            System.Diagnostics.Debug.WriteLine($"Screenshot saved: {fullPath}");
        }

        public string ElementCapture(AutomationElement element)
        {
            var elementimg = Capture.Element(element);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fileName = $"ElementSC_{DateTime.Now:yyyyMMdd_HHmmssfff}.png";
            string fullPath = Path.Combine(folderPath, fileName);

            elementimg.ToFile(fullPath);

            System.Diagnostics.Debug.WriteLine($"Screenshot saved: {fullPath}");

            return fullPath;
        }

        //public void RegionCapture(int x, int y, int width, int height)
        //{
        //   var regionimg = Capture.Rectangle(new Rectangle(x, y, width, height));
        //   regionimg.ToFile(@"C:\Aemulus\techFlowJazz\projects\RegionScreenCapture.png");
        //}
    }
}
