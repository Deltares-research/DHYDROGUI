using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.TestUtils;
using DeltaShell.IntegrationTestUtils.Builders;
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
    [TestFixture, Apartment(ApartmentState.STA)]
    [Category(TestCategory.WindowsForms)]
    public class WaterFlowFMModelNodePresenterTest
    {
        [Test]
        public void ShowTreeViewForFMModel()
        {
            var mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            var pluginsToAdd = new List<IPlugin>()
            {
                new SharpMapGisApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new ProjectExplorerGuiPlugin(),
                new NetworkEditorGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new FlowFMGuiPlugin(),

            };
            using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build())
            {
                gui.Run();
                
                Action mainWindowShown = delegate
                {
                    Project project = gui.Application.ProjectService.CreateProject();
                    project.RootFolder.Add(model);
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
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
            model.ModelDefinition.GetModelProperty("infiltrationmodel").SetValueFromString("0");
            
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