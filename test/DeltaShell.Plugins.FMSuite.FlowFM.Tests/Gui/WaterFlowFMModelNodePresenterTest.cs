using System;
using System.Linq;
using System.Windows.Controls;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DeltaShell.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class WaterFlowFMModelNodePresenterTest
    {
        [Test]
        public void ShowTreeViewForFMModel()
        {
            var mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());
                
                gui.Run();

                Action mainWindowShown = delegate
                {
                    var project = app.Project;
                    project.RootFolder.Add(model);
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }

/*
        [Test]
        public void JumpToSubTabThroughProjectExplorerWithModelViewNotYetOpen()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
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

                    var project = app.Project;
                    project.RootFolder.Add(model);

                    var modelNodePresenter = new WaterFlowFMModelNodePresenter(null);
                    var childItems = modelNodePresenter.GetChildNodeObjects(model, null);

                    gui.Selection = childItems.OfType<FlowFMTreeShortcut>().First(s => s.Text == "Numerical Parameters");
                    gui.CommandHandler.OpenViewForSelection();

                    Assert.IsNotNull(gui.DocumentViews.ActiveView);
                    Assert.AreEqual("Numerical Parameters", GetSelectedTab(GetActiveFMModelView()).Text);
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        public void JumpToSubTabThroughProjectExplorerWithModelViewAlreadyOpenOpen()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
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

                    var project = app.Project;
                    project.RootFolder.Add(model);

                    var modelNodePresenter = new WaterFlowFMModelNodePresenter(null);
                    var childItems = modelNodePresenter.GetChildNodeObjects(model, null);

                    // open on 'Domain' tab (first tab)
                    gui.Selection = childItems.OfType<FlowFMTreeShortcut>().First(s => s.Text == "General");
                    gui.CommandHandler.OpenViewForSelection();

                    // switch to 'Numerical Parameters' tab
                    gui.Selection = childItems.OfType<FlowFMTreeShortcut>().First(s => s.Text == "Numerical Parameters");
                    gui.CommandHandler.OpenViewForSelection();

                    // assert the 'Numerical Parameters' tab is in front
                    Assert.IsNotNull(gui.DocumentViews.ActiveView);
                    Assert.AreEqual("Numerical Parameters", GetSelectedTab(GetActiveFMModelView()).Text);
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }
*/

        [Test]
        [Category("Quarantine")]
        public void CheckEventLeaksThroughDataItemWrappers()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);
            
            var outputFunction = model.OutputHisFileStore.Functions.First();

            Console.WriteLine("Before:");
            int before = TestReferenceHelper.FindEventSubscriptions(outputFunction, true);

            var nodePresenter = new WaterFlowFMModelNodePresenter(null);
            var outputFolder = nodePresenter.GetChildNodeObjects(model, null).OfType<TreeFolder>().Last();

            // ask first time
            outputFolder.ChildItems.OfType<object>().ToList();

            int afterFirstTime = TestReferenceHelper.FindEventSubscriptions(outputFunction, true);

            for (int i = 0; i < 5; i++)
            {
                // ask for output items
                outputFolder.ChildItems.OfType<object>().ToList();
            }

            int afterManyTimes = TestReferenceHelper.FindEventSubscriptions(outputFunction, true);

            // todo: check for multiple models?

            Assert.AreEqual(before + 4, afterFirstTime); // first time increase by one for each event (PropChing, PropChed, CollChing, CollChed)
            Assert.AreEqual(afterFirstTime, afterManyTimes); // subseqeuent times should not increase
        }

        [Test]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.Integration)]
        [Category("Quarantine")]
        public void CheckEventLeaksThroughDataItemAfterModelRun()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            var outputFunction = model.OutputHisFileStore.Functions.FirstOrDefault();
            
            var nodePresenter = new WaterFlowFMModelNodePresenter(null);
            var outputFolder = nodePresenter.GetChildNodeObjects(model, null).OfType<TreeFolder>().LastOrDefault();

            // ask first time before run; dataitems are filled with output functions
            outputFolder.ChildItems.OfType<object>().ToList();

            Console.WriteLine("Before run:");
            int before = TestReferenceHelper.FindEventSubscriptions(outputFunction, true);

            ActivityRunner.RunActivity(model);
            Assert.AreEqual(ActivityStatus.Cleaned, model.Status);

            //dataitems are filled with new output functions
            outputFolder.ChildItems.OfType<object>().ToList();

            var di = outputFolder.ChildItems.OfType<IDataItem>().ToList().FirstOrDefault(d => d.Tag == outputFunction.Name);
            Assert.That(di.Value, Is.Not.EqualTo(outputFunction));

            Assert.That(TestReferenceHelper.FindEventSubscriptions(outputFunction, true), Is.EqualTo(before-4));// check if old events of dataitem are removed.

            //Filestore was closed and re-openen, so we need to retrieve a new output function.
            outputFunction = model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Name == outputFunction.Name);
            Assert.That(di.Value, Is.EqualTo(outputFunction));

            Console.WriteLine("After run:");
            var after = TestReferenceHelper.FindEventSubscriptions(outputFunction, true);

            Assert.AreEqual(before, after);
        }
        
        [Test]
        public void GetChildNodeObjects_ContainsCorrectObjects()
        {
            // Setup
            var guiPlugin = Substitute.For<GuiPlugin>();
            var graphicsProvider = Substitute.For<IGraphicsProvider>();
            guiPlugin.GraphicsProvider.Returns(graphicsProvider);
            
            var nodePresenter = new WaterFlowFMModelNodePresenter(guiPlugin);
            var model = new WaterFlowFMModel();
            
            // Call
            object[] objects = nodePresenter.GetChildNodeObjects(model, null).Cast<object>().ToArray();
            
            // Assert
            object[] physicalParameters = objects
                                          .OfType<TreeFolder>().First(f => f.Text == "2D").ChildItems
                                          .OfType<FmModelTreeShortcut>().First(f => f.Text == "Physical Parameters")
                                          .ChildObjects.ToArray();

            FmModelTreeShortcut roughness = GetShortCut(physicalParameters, "Roughness");
            Assert.That(roughness.Data, Is.SameAs(model.Roughness));
            
            FmModelTreeShortcut viscosity = GetShortCut(physicalParameters, "Viscosity");
            Assert.That(viscosity.Data, Is.SameAs(model.Viscosity));
            
            FmModelTreeShortcut diffusivity = GetShortCut(physicalParameters, "Diffusivity");
            Assert.That(diffusivity.Data, Is.SameAs(model.Diffusivity));
            
            FmModelTreeShortcut infiltration = GetShortCut(physicalParameters, "Infiltration");
            Assert.That(infiltration.Data, Is.SameAs(model.Infiltration));
        }

        [Test]
        public void GetChildNodeObjects_DoesNotUseInfiltration_DoesNotContainInfiltration()
        {
            // Setup
            var guiPlugin = Substitute.For<GuiPlugin>();
            var graphicsProvider = Substitute.For<IGraphicsProvider>();
            guiPlugin.GraphicsProvider.Returns(graphicsProvider);
            
            var nodePresenter = new WaterFlowFMModelNodePresenter(guiPlugin);
            var model = new WaterFlowFMModel();
            
            // Set to: no infiltration
            model.ModelDefinition.GetModelProperty("infiltrationmodel").SetValueAsString("0");
            
            // Call
            object[] objects = nodePresenter.GetChildNodeObjects(model, null).Cast<object>().ToArray();
            
            // Assert
            object[] physicalParameters = objects
                                          .OfType<TreeFolder>().First(f => f.Text == "2D").ChildItems
                                          .OfType<FmModelTreeShortcut>().First(f => f.Text == "Physical Parameters")
                                          .ChildObjects.ToArray();
            
            FmModelTreeShortcut infiltration = GetShortCut(physicalParameters, "Infiltration");
            Assert.That(infiltration, Is.Null);
        }

        private static FmModelTreeShortcut GetShortCut(object[] objects, string text)
        {
            return objects.OfType<FmModelTreeShortcut>().FirstOrDefault(f => f.Text == text);
        }
    }
}