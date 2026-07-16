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
    public class tFJ_45 : BaseTest
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

            var projectTree = assistant.FindByAutomationId(mainWindow, "projectAdvTree");
            Assert.IsNotNull(projectTree, "Project did not load successfully");

            System.Diagnostics.Debug.WriteLine("Project opened successfully");
        }
    }
}
