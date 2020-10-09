using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui.Swf;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DeltaShell.Gui;
using DeltaShell.NGHS.Common.IO.RestartFiles;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
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
            RestartFile[] inputRestart = initialConditionsFolder.ChildObjects.OfType<RestartFile>().ToArray();
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
    }
}