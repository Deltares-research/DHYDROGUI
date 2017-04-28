using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.LayerPropertiesEditor;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Geometries;
using NUnit.Framework;
using SharpMap;
using SharpMap.Api;
using SharpMap.Api.Editors;
using SharpMap.Data.Providers;
using SharpMap.Editors.Snapping;
using SharpMap.Layers;
using SharpMap.Styles;
using SharpMap.UI.Forms;
using SharpMap.UI.Tools;
using SharpMap.UI.Tools.Zooming;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms
{
    [TestFixture]
    public class NetworkLocationEditorMapControlTest
    {
        private Form geometryEditorForm;
        private MapControl mapControl;
        private ListBox listBoxTools;
        readonly INetwork network = new HydroNetwork();


        private void InitializeControls()
        {
            geometryEditorForm = new Form();
            // Create map and map control
            Map map = new Map();

            mapControl = new MapControl { Map = map };
            mapControl.Resize += delegate { mapControl.Refresh(); };
            mapControl.ActivateTool(mapControl.GetToolByType<PanZoomTool>());
            mapControl.Dock = DockStyle.Fill;
            // disable dragdrop because it breaks the test runtime
            mapControl.AllowDrop = false;

            // Create listbox to show all registered tools
            listBoxTools = new ListBox { Dock = DockStyle.Left };
            listBoxTools.SelectedIndexChanged += listBoxTools_SelectedIndexChanged;

            map.ZoomToExtents();

            mapControl.MoveTool.FallOffPolicy = FallOffType.Linear;

            geometryEditorForm.Controls.Add(listBoxTools);
            geometryEditorForm.Controls.Add(mapControl);
        }

        private void listBoxTools_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (-1 != listBoxTools.SelectedIndex)
            {
                mapControl.ActivateTool(mapControl.GetToolByName(listBoxTools.Items[listBoxTools.SelectedIndex].ToString()));
            }
        }

        readonly VectorLayer branchLayer = new VectorLayer("branches");
        readonly NetworkCoverageGroupLayer networkCoverageGroupLayer = new NetworkCoverageGroupLayer();

        private void AddBranchLayerAndTool()
        {
            branchLayer.DataSource = new FeatureCollection((IList)network.Branches, typeof(Channel));
            branchLayer.Visible = true;
            branchLayer.Style = new VectorStyle
            {
                Fill = new SolidBrush(Color.Tomato),
                Symbol = null,
                Line = new Pen(Color.Turquoise, 3)
            };
            mapControl.Map.Layers.Add(branchLayer);

            var newLineTool = new NewLineTool(l => l.Equals(branchLayer), "new branch") { AutoCurve = true, MinDistance = 0 };
            mapControl.Tools.Add(newLineTool);
            //return newLineTool;
        }

        private void AddNetworkCoverageAndTool()
        {

            INetworkCoverage networkCoverage = new NetworkCoverage { Network = network };
            networkCoverageGroupLayer.NetworkCoverage = networkCoverage;

            networkCoverageGroupLayer.LocationLayer.DataSource.AddNewFeatureFromGeometryDelegate =
                AddFeatureFromGeometryDelegate;

            mapControl.MouseDoubleClick += delegate
            {
                var dialog = new LayerPropertiesEditorDialog(networkCoverageGroupLayer.SegmentLayer);
                dialog.Show(mapControl);
            };
            mapControl.Map.Layers.Add(networkCoverageGroupLayer);

            var networkCoverageTool = new NewPointFeatureTool(l => l.Equals(networkCoverageGroupLayer.LocationLayer), "new location");
            mapControl.Tools.Add(networkCoverageTool);
            
            networkCoverageGroupLayer.LocationLayer.FeatureEditor.SnapRules.Add(new SnapRule
            {
                SnapRole = SnapRole.FreeAtObject,
                Obligatory = true,
                PixelGravity = 40
            });
            //return networkCoverageTool;
        }

        private IFeature AddFeatureFromGeometryDelegate(IFeatureProvider provider, IGeometry geometry)
        {
            var branch = (IBranch)mapControl.GetToolByType<SnapTool>().SnapResult.SnappedFeature;
            var offset = GeometryHelper.Distance((ILineString)branch.Geometry, geometry.Coordinates[0]);
            var location = new NetworkLocation(branch, offset) { Geometry = geometry };
            var networkCoverageFeatureCollection = (NetworkCoverageFeatureCollection)provider;
            networkCoverageFeatureCollection.NetworkCoverage.Locations.Values.Add(location);
            return location;
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void NewLineToolAndNetworkCoverageTool()
        {
            // same test as NewLineTool but adds the possibility to networklocation to a branch.
            // This test does not support topologyrles that update networklocations in response
            // to a branch geometry change.
            // A theme editor is available via a double click in the canvas.
            InitializeControls();

            AddBranchLayerAndTool();
            AddNetworkCoverageAndTool();

            mapControl.ActivateTool(mapControl.SelectTool);

            foreach (IMapTool tool in mapControl.Tools)
            {
                if (null != tool.Name)
                    listBoxTools.Items.Add(tool.Name);
            }

            WindowsFormsTestHelper.ShowModal(geometryEditorForm);
        }
    }
}
