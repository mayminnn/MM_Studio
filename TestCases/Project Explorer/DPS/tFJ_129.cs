using Automation.Components;
using Automation.Core;
using Automation.Pages;
using FlaUI.Core.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Test_Cases.Attributes;
using Test_Cases.Login;

namespace Test_Cases.Project_Explorer.DPS
{
    [TestClass]
    [TestTags("Project_Explorer", "DPS", "Positive", "Automated")]
    public class tFJ_129 : BaseTest
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

            Thread.Sleep(1000);

            _projectExplorer = new ProjectExplorerPane(mainWindow, _assistant);
        }
        
        public void oDPS_Pin_Dropdown_Should_Match_PinMap_PinNames()
        {
            _projectExplorer.OpenPage("Pin Maps");

            var pinMapPage = new PinMapPage(
                AppManager.MainWindow, _assistant);

            //List<string> pinMapPins = pinMapPage.GetUniquePinNames();

            //System.Diagnostics.Debug.WriteLine($"PinMap Count = {pinMapPins.Count}");
            
            _projectExplorer.OpenPage("DPSSets");

            var dpsPage = new DPSPage(
                AppManager.MainWindow,
                _assistant);

            //List<string> dpsDropdownPins = dpsPage.GetPinDropdownValues(4);

            //System.Diagnostics.Debug.WriteLine(
                //$"DPS Dropdown Count = {dpsDropdownPins.Count}");

            //var missingInDps = pinMapPins.Except(dpsDropdownPins,
                //StringComparer.OrdinalIgnoreCase).ToList();

            //var extraInDps = dpsDropdownPins.Except(pinMapPins,
                    //StringComparer.OrdinalIgnoreCase).ToList();

            //Assert.IsFalse(missingInDps.Any() || extraInDps.Any(),
                //$"Missing in DPS: [{string.Join(", ", missingInDps)}]\n" +
               // $"Extra in DPS: [{string.Join(", ", extraInDps)}]");
        }

        [TestMethod]
        public void Verify_Pin_Name_Retrieval_from_Pin_Maps()
        {
            // Open Pin Map
            _projectExplorer.OpenPage("Pin Maps");

            var pinMapPage =
                new PinMapPage(
                    AppManager.MainWindow,
                    _assistant);

            HashSet<string> pinMapValues =
                pinMapPage.GetPinGroupAndPinNames();

            System.Diagnostics.Debug.WriteLine(
                $"Pin Map Values Count = {pinMapValues.Count}");

            foreach (var value in pinMapValues.OrderBy(x => x))
            {
                System.Diagnostics.Debug.WriteLine(
                    $"PinMap : {value}");
            }

            // Open DPS
            _projectExplorer.OpenPage("DPSSets");

            var dpsPage =
                new DPSPage(
                    AppManager.MainWindow,
                    _assistant);

            HashSet<string> dpsValues =
                dpsPage.GetPinDropdownValues(4);

            System.Diagnostics.Debug.WriteLine(
                $"DPS Dropdown Count = {dpsValues.Count}");

            foreach (var value in dpsValues.OrderBy(x => x))
            {
                System.Diagnostics.Debug.WriteLine(
                    $"DPS : {value}");
            }

            // Compare
            var missingInDps =
                pinMapValues
                    .Except(
                        dpsValues,
                        StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList();

            var extraInDps =
                dpsValues
                    .Except(
                        pinMapValues,
                        StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList();

            System.Diagnostics.Debug.WriteLine(
                "====================================");

            if (missingInDps.Any())
            {
                System.Diagnostics.Debug.WriteLine(
                    "Missing In DPS:");

                foreach (var item in missingInDps)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"  {item}");
                }
            }

            if (extraInDps.Any())
            {
                System.Diagnostics.Debug.WriteLine(
                    "Extra In DPS:");

                foreach (var item in extraInDps)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"  {item}");
                }
            }

            System.Diagnostics.Debug.WriteLine(
                "====================================");

            Assert.IsFalse(
                missingInDps.Any() || extraInDps.Any(),
                $"Missing in DPS: [{string.Join(", ", missingInDps)}]{Environment.NewLine}" +
                $"Extra in DPS: [{string.Join(", ", extraInDps)}]");
        }
    }
}
