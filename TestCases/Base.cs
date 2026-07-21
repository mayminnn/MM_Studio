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
        // protected virtual string ProjectName => "OX01G10";
        // protected virtual string Version => "v1";
        protected virtual string ProjectName => "MM_TEST";
        protected virtual string Version => "2656";
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
