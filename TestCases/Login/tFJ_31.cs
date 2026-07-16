using Automation.Core;
using Automation.Pages;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test_Cases.Attributes;

namespace Test_Cases.Login
{
    [TestClass]
    [TestTags("Login", "Negative", "Automated")]
    public class tFJ_31 : BaseTest
    {
        protected override bool AutoLogin => false;

        [TestMethod]
        public void Login_With_Valid_Username_Blank_Password()
        {
            LoginPage.Login("a", "");

            var errorText = Retry.WhileNull(() =>
            {
                return AppManager.MainWindow.FindAllDescendants()
                    .FirstOrDefault(e =>
                        e.Name != null &&
                        e.Name.Contains("Username / password cannot be empty!"));

            }, TimeSpan.FromSeconds(10)).Result;

            Assert.IsNotNull(errorText, "Error message not found");

            // validate message
            Assert.AreEqual(
                "Username / password cannot be empty!",
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
