using Automation.Core;
using Automation.Pages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test_Cases.Login;

namespace Test_Cases.MenuAfterLogin
{
    //[TestClass]
    public class GotoDesign : BaseTest
    {
        //[TestMethod]
        public void NavigateToDesignPage_ShouldSucceed()
        {
            var assistant = new UIAssistant(AppManager.Automation);

            // from BaseTest/AppManager
            var mainWindow = AppManager.MainWindow;
            var psPage = new PSPage(mainWindow, assistant);
            psPage.OpenDesignPage();

            var designButton = assistant.FindByName(mainWindow, "Design");

            Assert.IsNotNull(designButton, "Failed to navigate to Design page or Design button not found");
            System.Diagnostics.Debug.WriteLine("Validation passed: On Design Page");
        }
    }
}