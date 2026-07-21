using Automation.Core;
using FlaUI.Core.AutomationElements;
using System;
using System.Linq;

namespace Automation.Components
{
    public class OpenProjectDialog
    {
        private readonly AutomationElement _root;
        private readonly UIAssistant _assistant;

        public OpenProjectDialog(AutomationElement root, UIAssistant assistant)
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

        // Select project
        private void SelectProject(string projectName)
        {
            var panel = _assistant.FindByAutomationId(_root, "panelEx");
            var listBoxProject = _assistant.FindByAutomationId(panel, "listBoxProject")?.AsListBox();
            if (listBoxProject == null)
                throw new Exception("Project list box not found");

            var item = listBoxProject.Items.FirstOrDefault(i => i.Name.Equals(projectName));
            if (item == null)
                throw new Exception($"Project '{projectName}' not found");

            item.Select();
            item.Click();
        }

        // Select version
        private void SelectVersion(string versionName)
        {
            var panel = _assistant.FindByAutomationId(_root, "panelEx");
            var listBoxVersion = _assistant.FindByAutomationId(panel, "listBoxVersion")?.AsListBox();
            if (listBoxVersion == null)
                throw new Exception("Version list box not found");

            var item = listBoxVersion.Items.FirstOrDefault(i => i.Name.Equals(versionName));
            if (item == null)
                throw new Exception($"Version '{versionName}' not found");

            item.Select();
            item.Click();
        }

        private void ClickOpen()
        {
            var panel = _assistant.FindByAutomationId(_root, "panelEx");
            var openButton = _assistant.FindByAutomationId(panel, "buttonOpen")?.AsButton();
            if (openButton == null)
                throw new Exception("Open button not found");

            openButton.Focus();
            openButton.Click();
        }

        public string OpenProject(string projectName, string versionName)
        {
            SelectProject(projectName);
            SelectVersion(versionName);
            ClickOpen();
            return projectName;
        }
    }
}