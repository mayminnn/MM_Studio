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
    [TestTags("Project_Explorer", "Bins", "Negative", "Automated")]
    [TestLinkIds("tFJ-399")]
    
    public class A399 : BaseTest
    {
        // Every project already has a Default sheet
        private const string SheetName = "Default";

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
        public void Create_Duplicate_Bin_Sheet()
        {
            string expectedError =
                $"Sheet '{SheetName}' of type 'Bin' already exists in workbook";

            _projectExplorer.SubmitNewSheet("Bins", SheetName);

            string popupText = _projectExplorer.DismissErrorDialog();

            Assert.Contains(expectedError, popupText,
                "Error popup does not show the duplicate-name message");

            string expectedLog =
                $"AddNewSheet() throws an error. Message: {expectedError}.";

            // The log entry can land a moment after the popup closes
            bool logged = Retry.WhileFalse(
                () => _binSheet.VerifyValidationSuccessLog(expectedLog),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromMilliseconds(500)).Result;

            Assert.IsTrue(logged, $"Log window does not contain: {expectedLog}");

            _projectExplorer.CloseNewSheetDialogIfOpen();

            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("| DUPLICATE BIN SHEET REJECTED |");
            Console.WriteLine("-----------------------------------------");

            Cleanup();
        }
    }
}
