using Microsoft.VisualStudio.TestTools.UnitTesting;
using Automation.Core;
using System;
using System.Linq;
using Automation.Pages;

namespace Test_Cases.Login
{
    [TestClass]
    public class BaseTest
    {
        protected AppManager AppManager;
        protected LoginPage LoginPage;
        protected PSPage MainPage;
        // Set from the TestOps run dialog; see TestSettings for the defaults
        protected virtual string ProjectName => TestSettings.ProjectName;
        protected virtual string Version => TestSettings.Version;
        protected virtual bool AutoLogin => true;
        
        [TestInitialize]
        public void Setup()
        {
            AppManager = new AppManager();
            AppManager.Launch();

            LoginPage = new LoginPage(AppManager.MainWindow);
            //MainPage = new PSPage(AppManager.MainWindow);

            if (AutoLogin) // default true
                LoginPage.Login("a", "a");
        }

        //[TestCleanup]
        public void Cleanup()
        {
            try
            {
                AppManager?.App?.Close();
            }
            catch
            {
                AppManager?.App?.Kill();
            }
            finally
            {
                AppManager?.Automation?.Dispose();
            }
        }
    }
}
