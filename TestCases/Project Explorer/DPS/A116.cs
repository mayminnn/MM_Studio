using Automation.Components;
using Automation.Core;
using Automation.Pages;
using FlaUI.Core.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using Test_Cases.Attributes;
using Test_Cases.Login;

namespace Test_Cases.Project_Explorer.DPS
{
    [TestClass]
    [TestTags("Project_Explorer", "DPS", "Negative", "Automated")]
    [TestLinkIds("tFJ-116")]
    public class A116 : BaseTest
    {
        // Every project already has a Default sheet
        private const string SheetName = "Default";

        private UIAssistant _assistant = null!;
        private PSPage _psPage = null!;
        private ProjectExplorerPane _projectExplorer = null!;
        private DPSPage _dpsPage = null!;

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
            _dpsPage = new DPSPage(mainWindow, _assistant);
        }

        [TestMethod]
        public void Create_Duplicate_DPS_Sheet()
        {
            string expectedError =
                $"Sheet '{SheetName}' of type 'DPS' already exists in workbook";

            _projectExplorer.SubmitNewSheet("DPSSets", SheetName);

            string popupText = _projectExplorer.DismissErrorDialog();

            Assert.Contains(expectedError, popupText,
                "Error popup does not show the duplicate-name message");

            string expectedLog =
                $"AddNewSheet() throws an error. Message: {expectedError}.";

            // The log entry can land a moment after the popup closes
            bool logged = Retry.WhileFalse(
                () => _dpsPage.VerifyValidationSuccessLog(expectedLog),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromMilliseconds(500)).Result;

            Assert.IsTrue(logged, $"Log window does not contain: {expectedLog}");

            _projectExplorer.CloseNewSheetDialogIfOpen();

            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("| DUPLICATE DPS SHEET REJECTED |");
            Console.WriteLine("-----------------------------------------");

            Cleanup();
        }
    }
}
