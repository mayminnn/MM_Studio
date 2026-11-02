using Automation.Components;
using Automation.Core;
using Automation.Pages;
using FlaUI.Core.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using Test_Cases.Attributes;
using Test_Cases.Login;

namespace Test_Cases.Project_Explorer.Bins
{
    [TestClass]
    [TestTags("Project_Explorer", "Bins", "Positive", "Automated")]
    [TestLinkIds("tFJ-281")]
    public class A281 : BaseTest
    {
        private static readonly string SheetName = $"D{DateTime.Now:mmss}";

        private UIAssistant _assistant = null!;
        private PSPage _psPage = null!;
        private ProjectExplorerPane _projectExplorer = null!;
        private BinSheetPage _binSheet = null!;

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

            // Only used for its log window check
            _binSheet = new BinSheetPage(mainWindow, _assistant);
        }

        [TestMethod]
        public void Create_New_Bin_Sheet()
        {
            _projectExplorer.CreateNewSheet("Bins", SheetName);

            string expectedLog =
                $"Worksheet '{SheetName}' of sheet type 'Bin' created.";

            // The log entry can land a moment after the dialog closes
            bool logged = Retry.WhileFalse(
                () => _binSheet.VerifyValidationSuccessLog(expectedLog),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromMilliseconds(500)).Result;

            Assert.IsTrue(logged, $"Log window does not contain: {expectedLog}");

            Console.WriteLine(expectedLog);

            Console.WriteLine("------------------------------");
            Console.WriteLine("| A281 NEW BIN SHEET CREATED |");
            Console.WriteLine("------------------------------");

            Cleanup();
        }
    }
}
