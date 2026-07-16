using Automation.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
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
            var panel = _assistant.FindByAutomationId(_root, "rbbarSites");

            if (panel == null)
                throw new Exception("rbbarSites panel not found");

            return panel;
        }

        private TextBox GetSiteTextBox()
        {
            var panel = GetSitePanel();

            var txtSite = panel
                .FindAllDescendants(cf => cf.ByControlType(ControlType.Edit))
                .FirstOrDefault()
                ?.AsTextBox();

            if (txtSite == null)
                throw new Exception("Site textbox not found inside rbbarSites");

            return txtSite;
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
    }
}