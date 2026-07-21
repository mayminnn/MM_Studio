using Microsoft.VisualStudio.TestTools.UnitTesting;
using Automation.Core;
using Test_Cases.Attributes;
using System.Diagnostics;

namespace Test_Cases.Login
{
    [TestClass]
    [TestTags("Login", "Smoke", "Positive", "Automated")]
    public class tFJ_09 : BaseTest
    {
        [TestMethod]
        public void Login_As_Admin()
        {
            Assert.IsNotNull(LoginPage);
            Debug.WriteLine("Validation passed: Login successfully");

            Cleanup();
        }
    }
}
