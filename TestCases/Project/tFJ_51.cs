using Automation.Core;
using Automation.Components;
using Automation.Pages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Test_Cases.Attributes;
using Test_Cases.Login;

namespace Test_Cases.Project
{
    [TestClass]
    [TestTags("Project", "Smoke", "Positive", "Automated")]
    public class tFJ_51 : BaseTest
    {
        [TestMethod]
        public void Close_Project()
        {
            string projectName = ProjectName;
            string version = Version;

            var assistant = new UIAssistant(AppManager.Automation);
            var mainWindow = AppManager.MainWindow;

            var psPage = new PSPage(mainWindow, assistant);
            psPage.OpenDesignPage();

            var ribbon = new RibbonPane(mainWindow, assistant);
            ribbon.ClickRibbonButton("Open Project");

            var openProjectWindow = assistant.FindByAutomationId(mainWindow, "OpenProjectForm");
            var openDialog = new OpenProjectDialog(openProjectWindow, assistant);

            // Use variable
            openDialog.OpenProject(ProjectName, Version);

            ribbon.ClickRibbonButton("Close Project");

            // Verify log dynamically
            var logUserControl = assistant.FindByAutomationId(mainWindow, "logUserControl");
            var panel = assistant.FindByAutomationId(logUserControl, "panel");
            var logListView = assistant.FindByAutomationId(panel, "logListView");
            var logItems = logListView.FindAllDescendants();

            bool found = logItems.Any(e => e.Name != null &&
                                          e.Name.Contains($"Project '{projectName}' closed."));

            Assert.IsTrue(found, $"Expected log not found for project {projectName}");

            System.Diagnostics.Debug.WriteLine("Validation passed: Project close log found");
            System.Diagnostics.Debug.WriteLine("Project closed successfully");
        }
    }
}