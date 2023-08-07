using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DelftTools.Utils.Reflection;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.Restart;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    public class WaterFlowFMModelNodePresenterTest
    {
        [Test]
        [Category(TestCategory.Wpf)]
        public void ShowTreeViewForFMModel()
        {
            string mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            using (var gui = new DeltaShellGui())
            {
                IApplication app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

                Action mainWindowShown = delegate
                {
                    Project project = app.Project;
                    project.RootFolder.Add(model);
                };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.Wpf)]
        public void JumpToSubTabThroughProjectExplorerWithModelViewNotYetOpen()
        {
            using (var gui = new DeltaShellGui())
            {
                IApplication app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

                Action mainWindowShown = delegate
                {
                    var model = new WaterFlowFMModel();

                    Project project = app.Project;
                    project.RootFolder.Add(model);

                    var modelNodePresenter = new WaterFlowFMModelNodePresenter(null);
                    IEnumerable childItems = modelNodePresenter.GetChildNodeObjects(model, null);

                    gui.Selection = childItems.OfType<FmModelTreeShortcut>().First(s => s.Text == "Numerical Parameters");
                    gui.CommandHandler.OpenViewForSelection();

                    Assert.IsNotNull(gui.DocumentViews.ActiveView);
                    var wpfView = (WpfSettingsView) gui.DocumentViews.ActiveView;
                    ObservableCollection<WpfGuiCategory> categoriesOnActiveView = wpfView.SettingsCategories;
                    Assert.That(categoriesOnActiveView.ElementAt(6).CategoryName, Is.EqualTo("Numerical Parameters"));
                };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.Wpf)]
        public void JumpToSubTabThroughProjectExplorerWithModelViewAlreadyOpenOpen()
        {
            using (var gui = new DeltaShellGui())
            {
                IApplication app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

                Action mainWindowShown = delegate
                {
                    var model = new WaterFlowFMModel();

                    Project project = app.Project;
                    project.RootFolder.Add(model);

                    var modelNodePresenter = new WaterFlowFMModelNodePresenter(null);
                    IEnumerable childItems = modelNodePresenter.GetChildNodeObjects(model, null);

                    // open on 'Domain' tab (first tab)
                    gui.Selection = childItems.OfType<FmModelTreeShortcut>().First(s => s.Text == "General");
                    gui.CommandHandler.OpenViewForSelection();

                    // switch to 'Numerical Parameters' tab
                    gui.Selection = childItems.OfType<FmModelTreeShortcut>().First(s => s.Text == "Numerical Parameters");
                    gui.CommandHandler.OpenViewForSelection();

                    // assert the 'Numerical Parameters' tab is in front
                    Assert.IsNotNull(gui.DocumentViews.ActiveView);
                    var wpfView = (WpfSettingsView) gui.DocumentViews.ActiveView;
                    ObservableCollection<WpfGuiCategory> categoriesOnActiveView = wpfView.SettingsCategories;
                    Assert.That(categoriesOnActiveView.ElementAt(6).CategoryName, Is.EqualTo("Numerical Parameters"));
                };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        public void CheckEventLeaksThroughDataItemWrappers()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.LoadFromMdu(mduPath);

            IFunction outputFunction = model.OutputHisFileStore.Functions.First();

            Console.WriteLine("Before:");
            int before = TestReferenceHelper.FindEventSubscriptions(outputFunction, true);

            var nodePresenter = new WaterFlowFMModelNodePresenter(null);
            TreeFolder outputFolder = nodePresenter.GetChildNodeObjects(model, null).OfType<TreeFolder>().Last();

            // ask first time
            outputFolder.ChildItems.OfType<object>().ToList();

            int afterFirstTime = TestReferenceHelper.FindEventSubscriptions(outputFunction, true);

            for (var i = 0; i < 5; i++)
            {
                // ask for output items
                outputFolder.ChildItems.OfType<object>().ToList();
            }

            int afterManyTimes = TestReferenceHelper.FindEventSubscriptions(outputFunction, true);

            // todo: check for multiple models?

            Assert.AreEqual(before + 4, afterFirstTime);     // first time increase by one for each event (PropChing, PropChed, CollChing, CollChed)
            Assert.AreEqual(afterFirstTime, afterManyTimes); // subseqeuent times should not increase
        }

        [Test]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.Integration)]
        public void CheckEventLeaksThroughDataItemAfterModelRun()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.LoadFromMdu(mduPath);

            IFunction outputFunction = model.OutputHisFileStore.Functions.FirstOrDefault();

            var nodePresenter = new WaterFlowFMModelNodePresenter(null);
            TreeFolder outputFolder = nodePresenter.GetChildNodeObjects(model, null).OfType<TreeFolder>().LastOrDefault();

            // ask first time before run; dataitems are filled with output functions
            outputFolder.ChildItems.OfType<object>().ToList();

            Console.WriteLine("Before run:");
            int before = TestReferenceHelper.FindEventSubscriptions(outputFunction, true);

            ActivityRunner.RunActivity(model);
            Assert.AreEqual(ActivityStatus.Cleaned, model.Status);

            //dataitems are filled with new output functions
            outputFolder.ChildItems.OfType<object>().ToList();

            IDataItem di = outputFolder.ChildItems.OfType<IDataItem>().ToList().FirstOrDefault(d => d.Tag == outputFunction.Name);
            Assert.That(di.Value, Is.Not.EqualTo(outputFunction));

            Assert.That(TestReferenceHelper.FindEventSubscriptions(outputFunction, true), Is.EqualTo(before - 4)); // check if old events of dataitem are removed.

            //Filestore was closed and re-openen, so we need to retrieve a new output function.
            outputFunction = model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Name == outputFunction.Name);
            Assert.That(di.Value, Is.EqualTo(outputFunction));

            Console.WriteLine("After run:");
            int after = TestReferenceHelper.FindEventSubscriptions(outputFunction, true);

            Assert.AreEqual(before, after);
        }

        [Test]
        public void GetChildNodeObjects_ContainsRestartInput()
        {
            // Setup
            var nodePresenter = new WaterFlowFMModelNodePresenter(null);
            var model = new WaterFlowFMModel();

            // Call
            IEnumerable childObjects = nodePresenter.GetChildNodeObjects(model, null);

            // Assert
            FmModelTreeShortcut initialConditionsFolder = childObjects.OfType<FmModelTreeShortcut>().Single(f => f.Text == "Initial Conditions");
            WaterFlowFMRestartFile[] inputRestart = initialConditionsFolder.ChildObjects.OfType<WaterFlowFMRestartFile>().ToArray();
            Assert.That(inputRestart.Length, Is.EqualTo(1));
            Assert.That(inputRestart[0], Is.SameAs(model.RestartInput));
        }

        [Test]
        public void GetChildNodeObjects_ContainsRestartOutputTreeFolder()
        {
            // Setup
            var nodePresenter = new WaterFlowFMModelNodePresenter(null);
            var model = new WaterFlowFMModel();

            // Call
            IEnumerable childObjects = nodePresenter.GetChildNodeObjects(model, null);

            // Assert
            TreeFolder outputTreeFolder = childObjects.OfType<TreeFolder>().Single(f => f.Text == "Output");
            TreeFolder[] restartTreeFolders = outputTreeFolder.ChildItems.OfType<TreeFolder>().Where(f => f.Text == "Restart").ToArray();

            Assert.That(restartTreeFolders.Length, Is.EqualTo(1));
        }
        
        [Test]
        [Category(TestCategory.Wpf)]
        [Category(TestCategory.Slow)]
        public void GivenARunningGui_WhenSavingClosingAndOpeningTheSameProject_ShouldReturnAProjectTreeWithOutputReferencedToTheLastInstanceOfTheModel()
        {
            using (var tempDirectory = new TemporaryDirectory())
            using (var gui = new DeltaShellGui())
            {
                IApplication app = gui.Application;
                
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());

                gui.Run();
                
                string mapFilePath = TestHelper.GetTestFilePath(@"Model\Output\FlowFM");

                string modelFolder = tempDirectory.CopyDirectoryToTempDirectory(mapFilePath);
                
                WpfTestHelper.ShowModal((Control)gui.MainWindow, () => ReopenANewCreatedProjectAndCheckItsProjectTree(tempDirectory, modelFolder, gui));
            }
        }

        private void ReopenANewCreatedProjectAndCheckItsProjectTree(TemporaryDirectory tempDirectory, string modelFolder, IGui gui)
        {
            IApplication app = gui.Application;

            CreateModelWithOutputCollapsed(modelFolder, gui, out int nrOfDataItems);

            string savePath = Path.Combine(tempDirectory.Path, "SaveLocation", "TestProject.dsproj");
            app.SaveProjectAs(savePath);

            app.CloseProject();

            app.OpenProject(savePath);

            CheckProjectTree(gui, nrOfDataItems);
        }

        private static void CheckProjectTree(IGui gui, int nrOfDataItems)
        {
            gui.MainWindow.ProjectExplorer.TreeView.CollapseAll();

            var dataItemsAfterOpening = (IList<DataItem>)TypeUtils.GetField(gui.MainWindow.ProjectExplorer.TreeView.NodePresenters.OfType<WaterFlowFMModelNodePresenter>().First(), "DataItems");
            Assert.AreEqual(nrOfDataItems, dataItemsAfterOpening.Count);

            foreach (DataItem dataItem in dataItemsAfterOpening)
            {
                Assert.AreSame(dataItem.Owner, gui.Application.Project.RootFolder.Models.First());
            }
        }

        private static void CreateModelWithOutputCollapsed(string absoluteModelFolderPath, IGui gui, out int nrOfDataItems)
        {
            var model = new WaterFlowFMModel();
            model.ImportFromMdu(Path.Combine(absoluteModelFolderPath, "input", "FlowFM.mdu"));

            gui.Application.Project.RootFolder.Items.Add(model);

            ActivityRunner.RunActivity(model);
            Assert.AreEqual(model.Status, ActivityStatus.Cleaned);

            gui.MainWindow.ProjectExplorer.TreeView.CollapseAll();
            var dataItemsBeforeOpening = (IList<DataItem>) TypeUtils.GetField(gui.MainWindow.ProjectExplorer.TreeView.NodePresenters.OfType<WaterFlowFMModelNodePresenter>().First(), "DataItems");

            foreach (DataItem dataItem in dataItemsBeforeOpening)
            {
                Assert.AreSame(dataItem.Owner, gui.Application.Project.RootFolder.Models.First());
            }

            nrOfDataItems = dataItemsBeforeOpening.Count;

            Assert.IsTrue(nrOfDataItems != 0);
        }
    }

}