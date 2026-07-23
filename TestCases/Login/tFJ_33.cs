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
            var instancelink = helper.Find(loginPanel, "linkLabelNewInstance");

            Assert.IsNotNull(loginPanel, "Login panel not found.");
            Assert.IsNotNull(instancelink, "Start another instance link not found.");

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
            // Count Jazz processes BEFORE click
            //--------------------------------------------------

            var jazzProcesses = Process.GetProcesses()
                .Where(p =>
                {
                    try
                    {
                        return p.ProcessName.IndexOf("Jazz", StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                    catch
                    {
                        return false;
                    }
                })
                .ToList();

            Console.WriteLine("=== Processes BEFORE ===");

            foreach (var p in jazzProcesses)
            {
                Console.WriteLine($"{p.Id} - {p.ProcessName}");
            }

            Console.WriteLine($"Count: {jazzProcesses.Count}");

            int beforeProcesses = jazzProcesses.Count;

            //--------------------------------------------------
            // Click hyperlink
            //--------------------------------------------------

            Console.WriteLine($"Supports Invoke: {link.Patterns.Invoke.IsSupported}");

            if (!link.Patterns.Invoke.IsSupported)
            {
                Assert.Fail("Hyperlink does not support Invoke pattern.");
            }

            link.Patterns.Invoke.Pattern.Invoke();

            Wait.UntilInputIsProcessed();

            // Debug.WriteLine("Clicked Start Another Instance link.");
            Console.WriteLine("Clicked Start Another Instance link.");


            //--------------------------------------------------
            // Wait for another Jazz process
            //--------------------------------------------------

            bool launched = Retry.WhileFalse(() =>
            {
                var count = Process.GetProcesses()
                    .Count(p =>
                    {
                        try
                        {
                            return p.ProcessName.Contains("Jazz");
                        }
                        catch
                        {
                            return false;
                        }
                    });

                return count == beforeProcesses + 1;

            },
            TimeSpan.FromSeconds(15)).Result;

            Console.WriteLine($"Before: {beforeProcesses}");

            var afterProcesses = Process.GetProcesses()
                .Count(p =>
                {
                    try
                    {
                        return p.ProcessName.Contains("Jazz");
                    }
                    catch
                    {
                        return false;
                    }
                });

            Console.WriteLine($"After: {afterProcesses}");

            Assert.IsTrue(
                launched,
                "A new Jazz process was not started.");

            Console.WriteLine("Second Jazz instance launched successfully.");

            Cleanup();
        }
    }
}