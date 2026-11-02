using Automation.Core;
using Automation.Pages;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using Test_Cases.Attributes;

namespace Test_Cases.Login
{
    [TestClass]
    [TestTags("Login", "Negative", "Automated")]
    [TestLinkIds("tFJ-11")]
    public class A11 : BaseTest
    {
        protected override bool AutoLogin => false;

        [TestMethod]
        public void Login_With_Invalid_Username_Valid_Password()
        {
            LoginPage.Login("user", "a");

            string expectedMessage = TestSettings.InvalidCredentialsMessage;

            var errorText = Retry.WhileNull(() =>
            {
                return AppManager.MainWindow.FindAllDescendants()
                    .FirstOrDefault(e =>
                        e.Name != null &&
                        e.Name == expectedMessage);

            }, TimeSpan.FromSeconds(10)).Result;

            Assert.IsNotNull(errorText, "Error message not found");

            // validate message
            Assert.AreEqual(
                expectedMessage,
                errorText.Name);

            // debug/logging
            Console.WriteLine($"Error popup observed: '{errorText.Name}'");
            Console.WriteLine("Validation passed: Invalid login detected successfully");

            /*var okButton = Retry.WhileNull(() =>
            {
                return AppManager.MainWindow.FindAllDescendants()
                    .FirstOrDefault(e =>
                        e.Name == "OK")?.AsButton();
            }, TimeSpan.FromSeconds(5)).Result;

            Assert.IsNotNull(okButton, "OK button not found on popup");

            okButton.Focus();
            okButton.Click(); // close the dialog
            Console.WriteLine("Popup closed by clicking OK");
            */
            Cleanup();
        }
    }
}
