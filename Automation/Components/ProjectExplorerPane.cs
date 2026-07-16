using Automation.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using FlaUI.Core.Input;
using System;
using System.Linq;
using System.Threading;

namespace Automation.Components
{
    public class ProjectExplorerPane
    {
        private readonly AutomationElement _window;
        private readonly UIAssistant _assistant;

        public ProjectExplorerPane(AutomationElement window, UIAssistant assistant)
        {
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }

            if (assistant == null)
            {
                throw new ArgumentNullException("assistant");
            }

            _window = window;
            _assistant = assistant;
        }

        public void OpenPage(string pageName)
        {
            var tree = FindTree();

            var topNode = Retry.WhileNull(() =>
            _assistant.FindByName(tree, pageName),
            TimeSpan.FromSeconds(10)).Result;

            if (topNode == null)
                throw new Exception($"Top-level item '{pageName}' not found");

            if (topNode != null)
            {
                System.Diagnostics.Debug.WriteLine($"Found {pageName} menu");
                System.Diagnostics.Debug.WriteLine($"Details: [ {topNode} ]\n");
                topNode.Focus();
                Thread.Sleep(200);

                Mouse.DoubleClick(topNode.BoundingRectangle.Center());

                Wait.UntilInputIsProcessed();
                Thread.Sleep(500);
            }

            var defaultNode = Retry.WhileNull(() =>
            {
                return topNode.FindAllDescendants()
                    .Where(x =>
                        x.Name == "Default" &&
                        !x.IsOffscreen &&
                        !x.BoundingRectangle.IsEmpty)
                    .FirstOrDefault();

            }, TimeSpan.FromSeconds(10)).Result;

            defaultNode.Focus();
            Thread.Sleep(200);

            Mouse.DoubleClick(defaultNode.BoundingRectangle.Center());

            Wait.UntilInputIsProcessed();
            Thread.Sleep(500);
            
        }

        public void oldOpenPage(string pageName)
        {
            var tree = FindTree();

            // Find top-level node
            var topNode = Retry.WhileNull(() =>
                _assistant.FindByName(tree, pageName),
                TimeSpan.FromSeconds(10)).Result;

            if (topNode == null)
                throw new Exception($"Top-level item '{pageName}' not found");

            if (topNode != null)
            {
                System.Diagnostics.Debug.WriteLine($"Found {pageName} menu");
                System.Diagnostics.Debug.WriteLine($"Details: [ {topNode} ]\n");
                topNode.DoubleClick();
            }

            // Find Default child
            var defaultNode = Retry.WhileNull(() =>
            {
                var node = _assistant.FindByName(topNode, "Default");
                if (node != null && node.Patterns.ScrollItem.IsSupported)
                    node.Patterns.ScrollItem.Pattern.ScrollIntoView();
                return node;
            }, TimeSpan.FromSeconds(10)).Result;

            if (defaultNode == null)
                throw new Exception($"Default node not found under '{pageName}'");

            defaultNode.Focus();
            var rect = defaultNode.BoundingRectangle;
            // defaultNode.DoubleClick(); 
            Mouse.DoubleClick(rect.Center());
        }

        private AutomationElement FindTree()
        {
            return Retry.WhileNull(() =>
            {
                var pnlxPage = _assistant.FindByAutomationId(_window, "pnlxPage");
                var designUC = _assistant.FindByAutomationId(pnlxPage, "projectDesignUserControl");
                var dockSite = _assistant.FindByAutomationId(designUC, "mainLeftDockSite");
                var leftBar = _assistant.FindByName(dockSite, "DotNetBar Bar");
                var projectExplorer = _assistant.FindByName(leftBar, "Project Explorer");
                var explorerUC = _assistant.FindByAutomationId(projectExplorer, "projectExplorerUserControl");
                var tableLayout = _assistant.FindByAutomationId(explorerUC, "tableLayoutPanel");

                return _assistant.FindByAutomationId(tableLayout, "projectAdvTree");

            }, TimeSpan.FromSeconds(10)).Result;
        }
    }
}