using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.Controls.Swf;
using DelftTools.Functions.Filters;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Converters.WellKnownText;
using SharpMap.Layers;
using SharpMap.UI.Forms;
using SharpMap.UI.Tools;
using SharpMap.UI.Tools.Zooming;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.Tools
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class HydroRegionEditorMapToolTest
    {
        private Form geometryEditorForm;
        private Form functionEditorForm;
        private FunctionView functionView;
        private ListBox listBoxTools;
        private PropertyGrid selectionProperties;
        private static HydroRegionMapLayer hydroNetworkLayer;
        private static MapControl mapControl;
        private static IHydroNetwork network;
        private ILayer channelLayer;
        private ILayer pumpLayer;
        private ILayer nodeLayer;
        private ILayer crossSectionLayer;
        private ClipboardMock clipboard;

        [SetUp]
        public void SetUp()
        {
            mapControl = new MapControl();

            network = new HydroNetwork();
            hydroNetworkLayer = (HydroRegionMapLayer) MapLayerProviderHelper.CreateLayersRecursive(network, null,new List<IMapLayerProvider> {new NetworkEditorMapLayerProvider()});
            channelLayer = hydroNetworkLayer.Layers.First(l => l.DataSource != null && l.DataSource.FeatureType == typeof(Channel));
            pumpLayer = hydroNetworkLayer.Layers.First(l => l.DataSource != null && l.DataSource.FeatureType == typeof(Pump));
            nodeLayer = hydroNetworkLayer.Layers.First(l => l.DataSource != null && l.DataSource.FeatureType == typeof(HydroNode));
            crossSectionLayer = hydroNetworkLayer.Layers.First(l => l.DataSource != null && l.DataSource.FeatureType == typeof(CrossSection));

            mapControl.Map.Layers.Add(hydroNetworkLayer);

            HydroRegionEditorHelper.AddHydroRegionEditorMapTool(mapControl);
            if (!GuiTestHelper.IsBuildServer) return;
            clipboard = new ClipboardMock();
            clipboard.GetText_Returns_SetText();
            clipboard.GetData_Returns_SetData();
        }

        [TearDown]
        public void TearDown()
        {
            if(mapControl != null && !mapControl.IsDisposed)
            {
                mapControl.Dispose();
            }
            if (!GuiTestHelper.IsBuildServer) return;
            clipboard.Dispose();
        }

        private void Add2BranchesUsingGeometry()
        {
            // Generate a simple network with 2 branches that have a common start node
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 30 40, 70 40, 100 100)"));
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 30 20, 70 30, 100 80)"));
        }

        [Test]
        public void NodeSnapping()
        {
            SetUp();
            Add2BranchesUsingGeometry();

            var node = network.Nodes.First();
            var branch2 = network.Branches.Skip(1).First();
            var branchCoordinate = branch2.Geometry.Coordinates[2];
            var coordinateNearBranch = new Coordinate(branchCoordinate.X, branchCoordinate.Y + 5);
            
            var worldCoord = new Coordinate(0, 0);
            var resultExistingNode = mapControl.GetToolByType<SnapTool>().ExecuteLayerSnapRules(nodeLayer, node, node.Geometry, worldCoord, 0);
            Assert.AreEqual(resultExistingNode.Location, worldCoord); //snap anywhere

            var result = mapControl.GetToolByType<SnapTool>().ExecuteLayerSnapRules(nodeLayer, null, null, coordinateNearBranch, 0);
            Assert.AreNotEqual(result.Location, coordinateNearBranch); //snap on branch
            Assert.IsTrue(branch2.Geometry.Distance(new Point(result.Location)) < 0.0001);
        }

        [Test]
        public void BranchTopology()
        {
            SetUp();
            Add2BranchesUsingGeometry();
            
            Assert.AreEqual(2, network.Branches.Count);

            // 2 branches added and connected at 0 0; thus 1 node in common makes 3 nodes
            Assert.AreEqual(3, network.Nodes.Count);
            Assert.AreEqual(network.Branches[0].Source, network.Branches[1].Source);
            Assert.AreEqual(0, network.Branches[0].Source.NodeFeatures.Count);
        }

        [Test]
        public void InsertNodeWithContextMenuTools9599()
        {
            SetUp();
            Add2BranchesUsingGeometry();

            var hydroNetworkEditorMapTool = (HydroRegionEditorMapTool)mapControl.Tools.First(t => t is HydroRegionEditorMapTool);

            // select branch
            mapControl.SelectTool.Select(network.Branches.First());
            
            // build context menu
            var items = hydroNetworkEditorMapTool.GetContextMenuItems(new Coordinate(10, 10));

            // grab 'insert node' menu item
            var insertNodeItem = items.Select(i => i.MenuItem).OfType<ToolStripItem>().FirstOrDefault(i => i.Text == "Insert Node");
            Assert.IsNotNull(insertNodeItem);

            // click it
            insertNodeItem.PerformClick();

            // expect the first branch was split
            Assert.AreEqual(3, network.Branches.Count);
        }

        [Test]
        public void RenderingDiscretisationGroupLayerWithNoSegmentLayer()
        {
            SetUp();

            var branch1 = new Channel
            {
                Geometry = new LineString(new[]{new Coordinate(0, 0), new Coordinate(30, 40),new Coordinate(70, 40), new Coordinate(100, 100)})
            };

            network.Branches.Add(branch1);

            var discretisation = new Discretization
                                     {
                                         Network = network,
                                         SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
                                     };

            var testLocation = new NetworkLocation(branch1, 5);
            discretisation[testLocation] = 0.0; //not fixed

            //SharpMapGisService used in application
            var discretisationGroupLayer = SharpMapLayerFactory.CreateMapLayerForCoverage(discretisation, mapControl.Map) as INetworkCoverageGroupLayer;

            mapControl.Map.Layers.Add(discretisationGroupLayer);

            var hydroNetworkEditorMapTool = (HydroRegionEditorMapTool)mapControl.Tools.First(t => t is HydroRegionEditorMapTool);

            hydroNetworkEditorMapTool.ActiveNetworkCoverageGroupLayer = discretisationGroupLayer;

            Assert.IsNull(discretisationGroupLayer.SegmentLayer);

            discretisation.RemoveValues(new VariableValueFilter<INetworkLocation>(discretisation.Locations,testLocation));

            Assert.AreEqual("No SegmentLayer is null exception", "No SegmentLayer is null exception");


        }

        [Test]
        public void CrossSectionTopology()
        {
            SetUp();
            Add2BranchesUsingGeometry();

            // NB A coordinate cross section will be treated as not geometry based.
            crossSectionLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING(50 45, 50 35)"));

            Assert.AreEqual(1, network.CrossSections.Count(), "1 cross-section added to the network");
            Assert.AreEqual(network.Branches[0], network.CrossSections.First().Branch, "cross-section should be connected to the first branch");

            // Cleanup test. If Branch is deleted the connected cross section are also deleted.
            var featureToDelete =channelLayer.DataSource.Features[0];
           channelLayer.DataSource.Features.Remove(featureToDelete);
            Assert.AreEqual(0, network.CrossSections.Count());
        }

        [Test]
        public void AddCrossSectionWithGeometryThatDoesNotOverlapWithBranch()
        {
            //CrossSection.ApplyDefaultValues() uses different component types in assignment hence error.
            Add2BranchesUsingGeometry();

            // Add a second cross section but do not place it at a branch
            // expect exception, geometry doesn't fit, can't find branch
            Assert.Throws<ArgumentException>(()=> crossSectionLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING(150 45, 150 55)"))); 
        }

        [Test]
        public void SimpleStructureTest()
        {
            SetUp();
            // This test checks the implicit adding and deleting of StructureFeature objects
            // when structures are added and deleted.
            Add2BranchesUsingGeometry();

            pumpLayer.DataSource.Add(GeometryFromWKT.Parse("POINT(40 40)"));
            Assert.AreEqual(2, network.Structures.Count());
            Assert.AreEqual(1, network.Pumps.Count());
            Assert.AreEqual(1, network.CompositeBranchStructures.Count());
            Assert.AreEqual(1, network.CompositeBranchStructures.First().Structures.Count);

            // upgrade to compound structure
            pumpLayer.DataSource.Add(GeometryFromWKT.Parse("POINT(40 40)"));
            Assert.AreEqual(2, network.Pumps.Count());
            Assert.AreEqual(3, network.Structures.Count());
            Assert.AreEqual(1, network.CompositeBranchStructures.Count());
            Assert.AreEqual(2, network.CompositeBranchStructures.First().Structures.Count);

            // use feature interactor to modify (also delete) features
            IStructure1D structure = network.Pumps.First();
            IFeatureInteractor featureInteractor = mapControl.SelectTool.GetFeatureInteractor(pumpLayer, structure);

            featureInteractor.Delete();

            Assert.AreEqual(1, network.Pumps.Count());
            Assert.AreEqual(2, network.Structures.Count());
            Assert.AreEqual(1, network.CompositeBranchStructures.Count());
            Assert.AreEqual(1, network.CompositeBranchStructures.First().Structures.Count);

            structure = network.Pumps.First();
            featureInteractor = mapControl.SelectTool.GetFeatureInteractor(pumpLayer, structure);
            featureInteractor.Delete();
            Assert.AreEqual(0, network.Structures.Count());
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Create()
        {
            SetUp();

            InitializeTestForm();
            InitializeFunctionEditorTestForm();

            foreach (IMapTool mapTool in mapControl.Tools)
            {
                if (null != mapTool.Name)
                    listBoxTools.Items.Add(mapTool.Name);
            }

            mapControl.Map.ZoomToFit(new Envelope(500, 500, 500, 500));
            WindowsFormsTestHelper.ShowModal(geometryEditorForm);
            mapControl.Dispose();

        }

        private void InitializeFunctionEditorTestForm()
        {
            functionEditorForm = new Form();
            geometryEditorForm.Size = new Size(800, 600);

            functionView = new FunctionView();
            functionView.Dock = DockStyle.Fill;
            functionEditorForm.Controls.Add(functionView);
            functionEditorForm.Disposed += functionEditorForm_Disposed;
        }

        void functionEditorForm_Disposed(object sender, EventArgs e)
        {
            functionEditorForm = null;
        }

        private void InitializeTestForm()
        {
            mapControl.Resize += delegate { mapControl.Refresh(); };
            mapControl.ActivateTool(mapControl.GetToolByType<PanZoomTool>());
            mapControl.Dock = DockStyle.Fill;
            mapControl.AllowDrop = false; // AllowDrop break test runtime

            geometryEditorForm = new Form();

            geometryEditorForm.Size = new Size(800, 600);

            Button buttonFunctionEdit = new Button();
            buttonFunctionEdit.Text = "Function";
            //buttonFunctionEdit.Width = 100;
            //buttonFunctionEdit.Height = 30;
            buttonFunctionEdit.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            buttonFunctionEdit.Click += LocationSeries;

            // Create listbox to show all registered tools
            listBoxTools = new ListBox();
            listBoxTools.Location = new System.Drawing.Point(0, buttonFunctionEdit.Height);
            listBoxTools.Height = geometryEditorForm.Height - buttonFunctionEdit.Height;
            //listBoxTools.Dock = DockStyle.Left;
            listBoxTools.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            listBoxTools.SelectedIndexChanged += listBoxTools_SelectedIndexChanged;

            // Create property grid for selection
            selectionProperties = new PropertyGrid();
            selectionProperties.Dock = DockStyle.Right;
            selectionProperties.Width = 150;

            // create interactor and connect it to map control
            
            mapControl.SelectedFeaturesChanged += MapControlSelectedFeaturesChanged;
            
            //geometryEditor.SelectionChanged += new System.ComponentModel.PropertyChangedEventHandler(geometryEditor_SelectionChanged);

            mapControl.Map.ZoomToFit(new Envelope(300,300,300,300));

            geometryEditorForm.Controls.Add(listBoxTools);
            geometryEditorForm.Controls.Add(selectionProperties);
            geometryEditorForm.Controls.Add(buttonFunctionEdit);
            geometryEditorForm.Controls.Add(mapControl);
            //geometryEditorForm.Size = new Size(800, 600);

        }
        void MapControlSelectedFeaturesChanged(object sender, EventArgs e)
        {
            if (null != mapControl.SelectedFeatures)
            {
                if (1 == mapControl.SelectedFeatures.Count())
                {
                    selectionProperties.SelectedObject = mapControl.SelectedFeatures.First().Geometry;
                    return;
                }
            }
            selectionProperties.SelectedObject = null;
        }

        private void listBoxTools_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (-1 != listBoxTools.SelectedIndex)
            {
                IMapTool mapTool = mapControl.GetToolByName(listBoxTools.Items[listBoxTools.SelectedIndex].ToString());
                if (mapTool.AlwaysActive)
                    mapTool.Execute();
                else
                    mapControl.ActivateTool(mapTool);
            }
        }

        public void LocationSeries(object sender, EventArgs e)
        {
            if (null == functionEditorForm)
            {
                InitializeFunctionEditorTestForm();
            }
            //FeatureVariable update does not work wordt feature coverage
            //functionView.Data = initialFlow;
            //functionEditorForm.Show();
        }

        [Test]
        public void HydroNetworkMapLayerNetworkCanHaveNetworkNull()
        {
            //see https://issues.deltares.nl/browse/TOOLS-6566
            using (var mapControl = new MapControl())  // do not use field, otherwise it will not be disposed!
            {
                hydroNetworkLayer.Region = null;
                var tool = new HydroRegionEditorMapTool { MapControl = mapControl };
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void BranchSetCustomLengthAndMoveCalculationPoint()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new[] { new Point(0, 0), new Point(100, 0) });
            INetworkCoverage coverage = new Discretization() {Network = network};

            var branch = network.Branches[0];
            branch.IsLengthCustom = true;
            branch.Length *= 2;

            var networkLocation = new NetworkLocation(branch, 120);
            coverage.Locations.AddValues(new[] { networkLocation });
            using (var coverageView = new CoverageView { Data = coverage })
            {
                var mapView = coverageView.ChildViews.OfType<MapView>().First();
                var networkLayer = MapLayerProviderHelper.CreateLayersRecursive(network, null, new List<IMapLayerProvider> {new NetworkEditorMapLayerProvider()});
                networkLayer.ShowInLegend = false;
                mapView.MapControl.Map.Layers.Add(networkLayer);
                HydroRegionEditorHelper.AddHydroRegionEditorMapTool(mapView.MapControl);

                WindowsFormsTestHelper.ShowModal(
                    coverageView,
                    f =>
                        {
                            mapView.MapControl.SelectTool.Select(networkLocation);

                            var moveTool = mapView.MapControl.MoveTool;
                            var args = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);
                            moveTool.OnMouseDown(networkLocation.Geometry.Coordinate, args);
                            moveTool.OnMouseMove(new Coordinate(50, 0), args);
                            var newpos = new Coordinate(50, 0);
                            moveTool.OnMouseUp(newpos, args);

                            Assert.AreEqual(newpos, networkLocation.Geometry.Coordinate);
                            Assert.AreEqual(100, networkLocation.Chainage);
                        });
            }
        }
    }
}

