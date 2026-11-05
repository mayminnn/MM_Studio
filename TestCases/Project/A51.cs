using Automation.Core;
using Automation.Components;
using Automation.Pages;
using FlaUI.Core.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Test_Cases.Attributes;
using Test_Cases.Login;

namespace Test_Cases.Project
{
    [TestClass]
    [TestTags("Project", "Smoke", "Positive", "Automated")]
    [TestLinkIds("tFJ-51")]
    public class A51 : BaseTest
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

            // Wait for the project to finish loading before closing it
            var projectTree = Retry.WhileNull(() =>
                assistant.FindByAutomationId(mainWindow, "projectAdvTree", 2),
                TimeSpan.FromSeconds(60), ignoreException: true).Result;

            Assert.IsNotNull(projectTree, "Project did not load successfully");

            ribbon.ClickRibbonButton("Close Project");

            // Verify log dynamically - wait for the message in case closing takes a while
            string expectedLog = $"Project '{projectName}' closed.";

            bool found = Retry.WhileFalse(() =>
            {
                var logUserControl = assistant.FindByAutomationId(mainWindow, "logUserControl", 2);
                var panel = assistant.FindByAutomationId(logUserControl, "panel", 2);
                var logListView = assistant.FindByAutomationId(panel, "logListView", 2);

                return logListView != null &&
                       logListView.FindAllDescendants().Any(e => e.Name != null &&
                                                                 e.Name.Contains(expectedLog));
            }, TimeSpan.FromSeconds(30), ignoreException: true).Result;

            Assert.IsTrue(found, $"Expected log not found for project {projectName}");

            Console.WriteLine("Validation passed: Project close log found");
            Console.WriteLine("Project closed successfully");
        }
    }
}