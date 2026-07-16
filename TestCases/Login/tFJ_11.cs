using Automation.Core;
using Automation.Pages;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Test_Cases.Attributes;

namespace Test_Cases.Login
{
    [TestClass]
    [TestTags("Login", "Negative", "Automated")]
    public class tFJ_11 : BaseTest
    {
        protected override bool AutoLogin => false;

        [TestMethod]
        public void Login_With_Invalid_Username_Valid_Password()
        {
            LoginPage.Login("user", "a");

            var errorText = Retry.WhileNull(() =>
            {
                return AppManager.MainWindow.FindAllDescendants()
                    .FirstOrDefault(e =>
                        e.Name != null &&
                        e.Name.Contains("Invalid username or password."));

            }, TimeSpan.FromSeconds(10)).Result;

            Assert.IsNotNull(errorText, "Error message not found");

            // validate message
            Assert.AreEqual(
                "Invalid username or password.",
                errorText.Name);

            // debug/logging
            System.Diagnostics.Debug.WriteLine($"Error popup observed: '{errorText.Name}'");
            System.Diagnostics.Debug.WriteLine("Validation passed: Invalid login detected successfully");

            /*var okButton = Retry.WhileNull(() =>
            {
                return AppManager.MainWindow.FindAllDescendants()
                    .FirstOrDefault(e =>
                        e.Name == "OK")?.AsButton();
            }, TimeSpan.FromSeconds(5)).Result;

            Assert.IsNotNull(okButton, "OK button not found on popup");

            okButton.Focus();
            okButton.Click(); // close the dialog
            System.Diagnostics.Debug.WriteLine("Popup closed by clicking OK");
            */
            Cleanup();
        }
    }
}
