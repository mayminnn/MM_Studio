using Automation.Components;
using Automation.Core;
using Automation.Pages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test_Cases.Attributes;
using Test_Cases.Login;

namespace Test_Cases.Project
{
    [TestClass]
    [TestTags("Project", "Smoke", "Positive", "Automated")]
    public class tFJ_19 : BaseTest
    {
        [TestMethod]
        public void Create_New_Project_with_Valid_Input()
        {
            var assistant = new UIAssistant(AppManager.Automation);
            var mainWindow = AppManager.MainWindow;

            var psPage = new PSPage(mainWindow, assistant);
            psPage.OpenDesignPage();

            var ribbon = new RibbonPane(mainWindow, assistant);
            ribbon.ClickRibbonButton("New Project");

            var newProjectWindow = assistant.FindByAutomationId(mainWindow, "NewProjectForm");

            if (newProjectWindow == null)
                throw new Exception("New Project dialog did not appear");

            var newDialog = new NewProjectDialog(newProjectWindow, assistant);

            string projectName = $"MM_TEST";
            //string version = $"{DateTime.Now:HHmmss}";
            string version = $"{DateTime.Now:mmss}";
            //string version = "984.1";

            newDialog.CreateProject(projectName, version);

            var projectTree = assistant.FindByAutomationId(mainWindow, "projectAdvTree");

            Assert.IsNotNull(projectTree, "New project did not load successfully");

            System.Diagnostics.Debug.WriteLine($"Project '{projectName}' created successfully");

            Cleanup();
        }
    }
}
