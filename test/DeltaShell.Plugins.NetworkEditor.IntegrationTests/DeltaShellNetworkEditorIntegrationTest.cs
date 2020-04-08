using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.PropertyBag.Dynamic;
using DeltaShell.Gui;
using DeltaShell.Gui.Forms.PropertyGrid;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Providers;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.LayerPropertiesEditor;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Layers;
using SharpMap.Rendering.Thematics;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.NetworkEditor.IntegrationTests
{
    [TestFixture]
    public class DeltaShellNetworkEditorIntegrationTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowGatedWeirInPropertyGrid()
        {
            var guiMock = Substitute.For<IGui>();

            var grid = new PropertyGrid(guiMock)
            {
                Data = new DynamicPropertyBag(new WeirProperties
                {
                    Data = new Weir("gated")
                    {
                        WeirFormula = new GatedWeirFormula(),
                        ParentStructure = new CompositeBranchStructure()
                    }
                })
            };

            WindowsFormsTestHelper.ShowModal(grid);
        }

        [Test] //TOOLS-6594
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void DeleteLocationFromCoverageWithoutSegmentLayerDoesNotCauseCrash()
        {
            IHydroNetwork network = HydroNetworkHelper.GetSnakeHydroNetwork(2);

            INetworkCoverage networkCoverage = new NetworkCoverage {Network = network};

            // set values
            var location = new NetworkLocation(network.Branches[0], 0.0);
            networkCoverage[location] = 0.1;
            networkCoverage[new NetworkLocation(network.Branches[0], 100.0)] = 0.2;
            networkCoverage[new NetworkLocation(network.Branches[1], 0.0)] = 0.3;
            networkCoverage[new NetworkLocation(network.Branches[1], 50.0)] = 0.4;
            networkCoverage[new NetworkLocation(network.Branches[1], 200.0)] = 0.5;

            var coverageView = new CoverageView {Data = networkCoverage};

            MapView mapView = coverageView.ChildViews.OfType<MapView>().First();
            NetworkCoverageGroupLayer networkCoverageLayer = mapView.Map.GetAllLayers(true).OfType<NetworkCoverageGroupLayer>().First();

            //remove segment layer
            networkCoverageLayer.LayersReadOnly = false;
            networkCoverageLayer.Layers.Remove(networkCoverageLayer.SegmentLayer);
            networkCoverageLayer.LayersReadOnly = true;

            MapControl mapControl = mapView.MapControl;
            mapControl.Visible = false; //prevent rendering

            var hydroNetworkEditorMapTool = new HydroRegionEditorMapTool {IsActive = true};
            mapControl.Tools.Add(hydroNetworkEditorMapTool);

            hydroNetworkEditorMapTool.ActiveNetworkCoverageGroupLayer = networkCoverageLayer;

            //remove a location
            mapControl.SelectTool.Select(networkCoverage.Locations.Values.First());
            mapControl.DeleteTool.DeleteSelection();

            coverageView.Dispose();
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowThemeEditorForWeirLayer()
        {
            IHydroNetwork hydroNetwork = GetHydroNetworkWithPumpAndWeir();

            var weirLayer = new VectorLayer
            {
                DataSource = new HydroNetworkFeatureCollection
                {
                    Network = hydroNetwork,
                    FeatureType = typeof(Weir)
                }
            };
            var editor = new ThemeEditor
            {
                ThemeType = ThemeType.Categorial,
                Layer = weirLayer
            };
            WindowsFormsTestHelper.ShowModal(editor);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Slow)]
        public void OpenHydroNetworkEditorForDataItemThatWasUnlinked()
        {
            //replays http://issues/browse/TOOLS-2646
            using (var gui = new DeltaShellGui())
            {
                gui.Application.Plugins.Add(new CommonToolsApplicationPlugin());
                gui.Application.Plugins.Add(new NHibernateDaoApplicationPlugin());
                gui.Application.Plugins.Add(new SharpMapGisApplicationPlugin());
                gui.Application.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Application.Plugins.ForEach(p => p.Application = gui.Application);

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Run();

                //add a network to the project
                Project project = gui.Application.Project;
                var hydroNetwork = new HydroNetwork();
                var dataItem = new DataItem(hydroNetwork);
                project.RootFolder.Add(dataItem);

                //open view for the object
                gui.DocumentViewsResolver.OpenViewForData(dataItem.Value);
                //close all views
                gui.DocumentViews.Clear();

                //link unlink the DI
                dataItem.LinkTo(new DataItem(new HydroNetwork()));
                //unlink create a new value for the item..
                dataItem.Unlink();

                //open again a view for the network of the DI
                gui.DocumentViewsResolver.OpenViewForData(dataItem.Value);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void DeleteBranchShouldRemoveBoundaryNodes()
        {
            IHydroNetwork network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(100, 0), new Point(100, 100));
            network.Branches.Remove(network.Branches[0]);
            NetworkHelper.RemoveUnusedNodes(network);
            Assert.AreEqual(2, network.HydroNodes.Count(n => !n.IsConnectedToMultipleBranches));
        }

        private static IHydroNetwork GetHydroNetworkWithPumpAndWeir()
        {
            IHydroNetwork hydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var compositeBranchStructure = new CompositeBranchStructure();
            var pump = new Pump("pump1")
            {
                OffsetY = 1000,
                StopDelivery = 18,
                StartDelivery = 12,
                StopSuction = 12,
                StartSuction = 15
            };
            var weir = new Weir("weri1")
            {
                CrestLevel = 15,
                CrestWidth = 50
            };
            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, hydroNetwork.Branches[0], 50);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, weir);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, pump);
            return hydroNetwork;
        }
    }
}