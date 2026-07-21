using Automation.Core;
using FlaUI.Core.AutomationElements;
using System;

namespace Automation.Components
{
    public class NewProjectDialog
    {
        private readonly AutomationElement _root;
        private readonly UIAssistant _assistant;

        public NewProjectDialog(AutomationElement root, UIAssistant assistant)
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

        private AutomationElement GetPanel()
        {
            return _assistant.FindByAutomationId(_root, "panelEx");
        }

        private void EnterProjectName(string projectName)
        {
            var panel = GetPanel();

            var txtProject = _assistant
                .FindByAutomationId(panel, "textBoxProjectName")
                ?.AsTextBox();

            if (txtProject == null)
                throw new Exception("Project Name textbox not found");

            txtProject.Focus();
            txtProject.Text = projectName;
        }

        private void EnterVersion(string version)
        {
            var panel = GetPanel();

            var txtVersion = _assistant
                .FindByAutomationId(panel, "textBoxVersion")
                ?.AsTextBox();

            if (txtVersion == null)
                throw new Exception("Version textbox not found");

            txtVersion.Focus();
            txtVersion.Text = version;
        }

        private void ClickOktoCreate()
        {
            var panel = GetPanel();

            var btnCreate = _assistant
                .FindByAutomationId(panel, "buttonOk")
                ?.AsButton();

            if (btnCreate == null)
                throw new Exception("Create button not found");

            btnCreate.Focus();
            btnCreate.Click();
        }

        public string CreateProject(string projectName, string version)
        {
            EnterProjectName(projectName);
            EnterVersion(version);
            ClickOktoCreate();

            return projectName;
        }
    }
}