using Automation.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System.Diagnostics;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using System.Collections.Generic;
using System.Linq;

namespace Automation.Components
{
    public class SitePane
    {
        private readonly AutomationElement _root;
        private readonly UIAssistant _assistant;

        public SitePane(AutomationElement root, UIAssistant assistant)
        {
            if (root == null)
            {
                throw new ArgumentNullException("root");
            }

            if (assistant == null)
            {
                throw new ArgumentNullException("assistant");
            }

            _root = root;
            _assistant = assistant;
        }

        private AutomationElement GetSitePanel()
        {
            // Right after a project opens the app can be too busy to answer UI Automation, and the
            // search throws a COM "Operation timed out" instead of returning null; keep retrying
            // through those until the panel shows up
            var panel = Retry.WhileNull(
                () => _root.FindFirstDescendant(cf => cf.ByAutomationId("rbbarSites")),
                TimeSpan.FromSeconds(30),
                TimeSpan.FromMilliseconds(500),
                throwOnTimeout: false,
                ignoreException: true).Result;

            if (panel == null)
                throw new Exception("rbbarSites panel not found");

            return panel;
        }

        private TextBox GetSiteTextBox()
        {
            var panel = GetSitePanel();

            var txtSite = FindSiteEdit(panel);

            // The site count box shows as a button (named after the current count) until it is
            // clicked; clicking it opens the in-place editor, which is the Edit we type into
            if (txtSite == null)
            {
                GetSiteCountButton(panel).Click();

                txtSite = Retry.WhileNull(
                    () => FindSiteEdit(panel),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromMilliseconds(200),
                    throwOnTimeout: false,
                    ignoreException: true).Result;
            }

            if (txtSite == null)
                throw new Exception("Site textbox not found inside rbbarSites");

            return txtSite;
        }

        private static TextBox? FindSiteEdit(AutomationElement panel)
        {
            return panel
                .FindAllDescendants(cf => cf.ByControlType(ControlType.Edit))
                .FirstOrDefault()
                ?.AsTextBox();
        }

        // The button next to the "Total Sites" label, in the same group as Confirm
        private static Button GetSiteCountButton(AutomationElement panel)
        {
            var label = panel.FindFirstDescendant(cf => cf.ByName("Total Sites"));

            var button = label?.Parent
                ?.FindAllChildren(cf => cf.ByControlType(ControlType.Button))
                .FirstOrDefault(b => !string.Equals(b.Name, "Confirm", StringComparison.OrdinalIgnoreCase))
                ?.AsButton();

            if (button == null)
                throw new Exception("Site count box (button next to 'Total Sites') not found inside rbbarSites");

            return button;
        }

        private Button GetConfirmButton()
        {
            var panel = GetSitePanel();

            var btn = panel
                .FindFirstDescendant(cf => cf.ByName("Confirm"))
                ?.AsButton();

            if (btn == null)
                throw new Exception("Confirm button not found inside rbbarSites");

            return btn;
        }

        private void EnterSiteCount(int siteCount)
        {
            var txtSite = GetSiteTextBox();

            txtSite.Focus();
            txtSite.Text = siteCount.ToString();
        }

        private void ClickConfirm()
        {
            var btn = GetConfirmButton();

            btn.Focus();
            btn.Click();
        }

        public void ValidateGallerySites(int siteCount)
        {
            var panel = GetSitePanel();

            var gallery = panel.FindFirstDescendant(cf => cf.ByName("gallerySites"));

            if (gallery == null)
                throw new Exception("gallerySites not found");

            int expected = siteCount - 1;

            var items = gallery.FindAllChildren();

            if (items.Length <= expected)
                throw new Exception($"Expected at least {expected + 1} items but found {items.Length}");

            var target = items[expected];

            if (!target.Name.Contains(expected.ToString()))
                throw new Exception($"Expected site index {expected} not found in gallery");
        }

        public void ChangeSiteCount(int siteCount)
        {
            /*if (siteCount <= 0)
                throw new ArgumentException("Site count must be greater than 0");*/

            EnterSiteCount(siteCount);
            ClickConfirm();
            //ValidateGallerySites(siteCount);
        }

        public void EnterInvalidSiteCount(string invalidInput)
        {
            var txtSite = GetSiteTextBox();

            // txtSite.Focus();
            // txtSite.Text = invalidInput;

            txtSite.Focus();
            txtSite.Text = invalidInput;

            Keyboard.Press(VirtualKeyShort.TAB);
            Keyboard.Release(VirtualKeyShort.TAB);

            Thread.Sleep(500);

            ClickConfirm();
        }

        public void ChangeSiteInvalid(string invalidInput)
        {
            EnterInvalidSiteCount(invalidInput);
            ClickConfirm();
            //ValidateGallerySites(siteCount);
        }

        public bool VerifyValidationSuccessLog(string expectedText)
        {

            var pnlxPage = _assistant.FindByAutomationId(_root, "pnlxPage");
            var designUC = _assistant.FindByAutomationId(pnlxPage, "projectDesignUserControl");
            var dockSite = _assistant.FindByAutomationId(designUC, "mainBottomDockSite");
            var logPanel = _assistant.FindByAutomationId(dockSite, "logUserControl");
            var panel = _assistant.FindByAutomationId(logPanel, "panel");
            var logList = _assistant.FindByAutomationId(panel, "logListView").OrThrow("logListView");

            var logs = logList.FindAllDescendants();

            return logs.Any(x =>
                !string.IsNullOrEmpty(x.Name) &&
                x.Name.Contains(expectedText));
        }
    }
}