using Automation.Components;
using Automation.Core;
using Automation.Pages;
using FlaUI.Core.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Test_Cases.Login;

namespace Test_Cases.Project_Explorer.Bins
{
    //[TestClass]
    public class OpenDefaultBin : BaseTest
    {
        private UIAssistant _assistant;
        private PSPage _psPage;
        private ProjectExplorerPane _projectExplorer;

        [TestInitialize]
        public void Start()
        {
            _assistant = new UIAssistant(AppManager.Automation);
            var mainWindow = AppManager.MainWindow;

            _psPage = new PSPage(mainWindow, _assistant);
            _psPage.OpenDesignPage();

            var ribbon = new RibbonPane(mainWindow, _assistant);
            ribbon.ClickRibbonButton("Open Project");

            var openProjectWindow = _assistant.FindByAutomationId(mainWindow, "OpenProjectForm");
            var openDialog = new OpenProjectDialog(openProjectWindow, _assistant);
            openDialog.OpenProject(ProjectName, Version);

            _projectExplorer = new ProjectExplorerPane(mainWindow, _assistant);
        }

        [TestMethod]
        public void Open_Default_Bins()
        {
            OpenAndVerify("Bins");
        }

        private void OpenAndVerify(string pageName)
        {
            var mainWindow = AppManager.MainWindow;

            _projectExplorer.OpenPage(pageName);

            var tree = _assistant.FindByAutomationId(mainWindow, "projectAdvTree");
            Assert.IsNotNull(tree, "Tree not found");

            var node = Retry.WhileNull(() =>
                _assistant.FindByName(tree, pageName, timeout: 2),
                TimeSpan.FromSeconds(10)).Result;

            Assert.IsNotNull(node, $"{pageName} node not found");

            System.Diagnostics.Debug.WriteLine($"{pageName} opened successfully");
        }
    }
}