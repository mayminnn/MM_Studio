using Automation.Components;
using Automation.Core;
using Automation.Pages;
using FlaUI.Core.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Test_Cases.Login;

namespace Test_Cases.Project_Explorer.Flows
{
    [TestClass]
    public class SelectBin : BaseTest
    {
        private UIAssistant _assistant = null!;
        private PSPage _psPage = null!;
        private ProjectExplorerPane _projectExplorer = null!;
        private Flow _flowPage = null!;
        private DatalogClient _dc = null!;

        [TestInitialize]
        public void Start()
        {
            _assistant = new UIAssistant(AppManager.Automation);

            var mainWindow = AppManager.MainWindow;

            _psPage = new PSPage(mainWindow, _assistant);
            _psPage.OpenDesignPage();

            var ribbon = new RibbonPane(mainWindow, _assistant);
            ribbon.ClickRibbonButton("Open Project");

            var openProjectWindow =
                _assistant.FindByAutomationId(mainWindow, "OpenProjectForm");

            var openDialog =
                new OpenProjectDialog(openProjectWindow, _assistant);

            openDialog.OpenProject(ProjectName, Version);

            _projectExplorer =
                new ProjectExplorerPane(mainWindow, _assistant);

            _projectExplorer.OpenPage("Flows");

            _flowPage = new Flow(mainWindow, _assistant);
            _dc = new DatalogClient(mainWindow, _assistant);
        }

        [TestMethod]
        public void SelectFailBin()
        {
            _projectExplorer.OpenPage("Bins");

            var binPage = new BinSheetPage(AppManager.MainWindow, _assistant);

            List<string> expected =
                binPage.GetFailBinNames();

            Console.WriteLine(
                $"Fail bins = {expected.Count}");

            _projectExplorer.OpenPage("Flows");

            var flow =
                new Flow(AppManager.MainWindow, _assistant);

            Console.WriteLine(
                "test");

            List<string> dropdown =
                flow.GetFailBinsDropdownItems(5);

            Console.WriteLine(
                "test2");

            CollectionAssert.AreEqual(
                expected,
                dropdown,
                "Fail Bins dropdown does not match Bin sheet.");

            List<string> picker =
                flow.GetFailBinsFromPicker(5);

            CollectionAssert.AreEqual(
                expected,
                picker,
                "Bin Picker list does not match Bin sheet.");
        }
    }
}
