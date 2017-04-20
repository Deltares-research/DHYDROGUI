using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;
using SharpMap;
using SharpMap.Api.Layers;
using SharpMap.UI.Forms;
using Control = System.Windows.Controls.Control;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class FlowFMMapLayerProviderTest
    {
        [Test]
        public void ShowLayersForFMModel()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            ShowModelLayers(new WaterFlowFMModel(mduPath));
        }
        
        [Test]
        public void ShowLayersForIvoorkust()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"mdu_ivoorkust\ivk.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            ShowModelLayers(new WaterFlowFMModel(mduPath));
        }

        [Test]
        public void ShowLayersAdjustedModel()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            model.Area.DredgingLocations.Add(new Feature2D
                {
                    Geometry = new Polygon(new LinearRing(new[]
                        {
                            new Coordinate(-135, -105), new Coordinate(-85, -100), 
                            new Coordinate(-75, -205), new Coordinate(-125, -200),  
                            new Coordinate(-135, -105)
                        }))
                });

            ShowModelLayers(model);
        }

        [Test]
        public void CheckLayerIsSetCorrectlyWhenOpeningFMItems()
        {
            var mduPath = TestHelper.GetTestFilePath(@"roughness\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            using (var gui = new DeltaShellGui())
            {
                var fmGuiPlugin = new FlowFMGuiPlugin();

                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(fmGuiPlugin);

                gui.Run();

                Action mainWindowShown = delegate
                {
                    var project = app.Project;
                    project.RootFolder.Add(model);

                    var modelNodePresenter = new WaterFlowFMModelNodePresenter(fmGuiPlugin);
                    var fmTreeShortcut = modelNodePresenter.GetChildNodeObjects(model, null)
                                          .OfType<FlowFMTreeShortcut>()
                                          .First(s => s.Text == "General");

                    gui.CommandHandler.OpenView(fmTreeShortcut);

                    var activeView = (ProjectItemMapView)gui.DocumentViews.ActiveView;
                    var activeTab = activeView.MapView.TabControl.ActiveView;

                    Assert.IsInstanceOf<WaterFlowFMModelView>(activeTab);
                    Assert.IsNotNull(((ILayerEditorView) activeTab).Layer);
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }

        private static void ShowModelLayers(WaterFlowFMModel model)
        {
            var providers = new IMapLayerProvider[] { new FlowFMMapLayerProvider(), new SharpMapLayerProvider() };

            var layer = (IGroupLayer) MapLayerProviderHelper.CreateLayersRecursive(model, null, providers);

            layer.Layers.ForEach(l => l.Visible = true);

            var map = new Map {Layers = {layer}, Size = new Size {Width = 800, Height = 800}};
            map.ZoomToExtents();

            var mapControl = new MapControl {Map = map, Dock = DockStyle.Fill};

            WindowsFormsTestHelper.ShowModal(mapControl);
        }
    }
}