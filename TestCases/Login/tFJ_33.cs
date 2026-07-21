using Automation.Core;
using Automation.Pages;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlaUI.Core.Input;
using System.Diagnostics;
using Test_Cases.Attributes;

namespace Test_Cases.Login
{
    [TestClass]
    [TestTags("Login", "Smoke", "Positive", "Automated")]
    public class tFJ_33 : BaseTest
    {
        protected override bool AutoLogin => false;

        [TestMethod]
        public void Start_Another_Instance_Of_Jazz()
        {
            //--------------------------------------------------
            // Find login panel
            //--------------------------------------------------

            var helper = new UIHelper();

            var loginPanel = helper.Find(AppManager.MainWindow, "tblLogin");

            Assert.IsNotNull(loginPanel, "Login panel not found.");

            //--------------------------------------------------
            // Find hyperlink
            //--------------------------------------------------

            var link = Retry.WhileNull(() =>
            {
                return loginPanel
                    .FindAllDescendants()
                    .FirstOrDefault(x =>
                        x.Name == "Start Another Instance of Jazz");
            },
            TimeSpan.FromSeconds(5)).Result;

            Assert.IsNotNull(link, "Start another instance link not found.");

            //--------------------------------------------------
            // Click hyperlink
            //--------------------------------------------------

            link.Click();

            Wait.UntilInputIsProcessed();

            System.Threading.Thread.Sleep(2000);

            //--------------------------------------------------
            // Verify another Jazz window appears
            //--------------------------------------------------

            var desktop = AppManager.Automation.GetDesktop();

            var windows = Retry.While(() =>
            {
                return desktop.FindAllChildren()
                            .Where(w => w.Name.Contains("Jazz"))
                            .ToArray();

            },
            result => result.Length < 2,
            TimeSpan.FromSeconds(10)).Result;

            Assert.IsGreaterThanOrEqualTo(2,
                windows.Length, "Second Jazz window was not opened.");

            Debug.WriteLine($"Detected {windows.Length} Jazz windows.");

            Cleanup();
        }
    }
}