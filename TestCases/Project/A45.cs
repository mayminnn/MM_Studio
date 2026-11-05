using Automation.Components;
using Automation.Core;
using Automation.Pages;
using FlaUI.Core.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test_Cases.Attributes;
using Test_Cases.Login;

namespace Test_Cases.Project
{
    [TestClass]
    [TestTags("Project", "Smoke", "Positive", "Automated")]
    [TestLinkIds("tFJ-45")]
    public class A45 : BaseTest
    {        
        [TestMethod]
        public void Open_Project()
        {
            var assistant = new UIAssistant(AppManager.Automation);
            var mainWindow = AppManager.MainWindow;
            var psPage = new PSPage(mainWindow, assistant);
            psPage.OpenDesignPage();
            var ribbon = new RibbonPane(mainWindow, assistant);
            ribbon.ClickRibbonButton("Open Project");
            var openProjectWindow = assistant.FindByAutomationId(mainWindow, "OpenProjectForm");

            if (openProjectWindow == null)
                throw new Exception("Open Project dialog did not appear");

            var openDialog = new OpenProjectDialog(openProjectWindow, assistant);

            //openDialog.OpenProject("MM_TEST", "918.1");

            openDialog.OpenProject(ProjectName, Version);

            // The app stops responding to UI Automation while the project loads,
            // so keep retrying through the "Operation timed out" COMExceptions
            var projectTree = Retry.WhileNull(() =>
                assistant.FindByAutomationId(mainWindow, "projectAdvTree", 2),
                TimeSpan.FromSeconds(60), ignoreException: true).Result;

            Assert.IsNotNull(projectTree, "Project did not load successfully");

            // Verify log - the message appears once loading finishes, so wait for it
            string expectedLog = $"Project '{ProjectName}' loaded successfully";

            bool found = Retry.WhileFalse(() =>
            {
                var logUserControl = assistant.FindByAutomationId(mainWindow, "logUserControl", 2);
                var panel = assistant.FindByAutomationId(logUserControl, "panel", 2);
                var logListView = assistant.FindByAutomationId(panel, "logListView", 2);

                return logListView != null &&
                       logListView.FindAllDescendants().Any(e => e.Name != null &&
                                                                 e.Name.Contains(expectedLog));
            }, TimeSpan.FromSeconds(60), ignoreException: true).Result;

            Assert.IsTrue(found, $"Expected log not found: {expectedLog}");

            Console.WriteLine("Validation passed: Project loaded log found");
            Console.WriteLine("Project opened successfully");
        }
    }
}
