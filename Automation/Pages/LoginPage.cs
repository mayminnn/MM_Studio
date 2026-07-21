using Automation.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using System;
using System.Linq;

namespace Automation.Pages
{
    public class LoginPage
    {
        private readonly Window _window;
        private readonly UIHelper _helper;

        public LoginPage(Window window)
        {
            _window = window;
            _helper = new UIHelper();
        }

        public void Login(string username, string password)
        {
            var pnlLogin = _helper.Find(_window, "tblLogin");
            
            if (pnlLogin == null)
            {
                throw new Exception("Login panel not found");
            }
            
            var userBox = _helper.Find(pnlLogin, "txtUser")?.AsTextBox();
            var pwdBox = _helper.Find(pnlLogin, "txtPwd")?.AsTextBox();
            var loginBtn = _helper.Find(pnlLogin, "btnLogin")?.AsButton();

            if (userBox == null || pwdBox == null || loginBtn == null)
                throw new Exception("Login controls not found");

            userBox.Enter(username);
            pwdBox.Enter(password);

            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(300);

            loginBtn.Click();

            Wait.UntilInputIsProcessed();
        }
    }
}
