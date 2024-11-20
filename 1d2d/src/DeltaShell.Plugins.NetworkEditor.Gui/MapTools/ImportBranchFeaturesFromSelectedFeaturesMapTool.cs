using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Providers;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using SharpMap.Api.Layers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.MapTools
{
    public class ImportBranchFeaturesFromSelectedFeaturesMapTool : MapTool
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ImportBranchFeaturesFromSelectedFeaturesMapTool));

        private HydroRegionMapLayer HydroNetworkMapLayer
        {
            get { return (HydroRegionMapLayer)Layers.FirstOrDefault(); }
        }

        public ImportBranchFeaturesFromSelectedFeaturesMapTool(Func<ILayer, bool> layerFilter)
        {
            LayerFilter = layerFilter;
            Tolerance = 10;
        }

        public override bool Enabled { get { return HydroNetworkMapLayer != null && MapControl.SelectedFeatures.Any(s => s.Geometry is Point && !(s is IHydroObject)); } }

        public override bool AlwaysActive
        {
            get { return Enabled; }
        }

        public override void Execute()
        {
            var layerList = new EventedList<ILayer>();
            foreach (var layer in HydroNetworkMapLayer.Layers)
            {
                if (layer.DataSource == null) continue;

                //some comment??
                if ((((HydroNetworkFeatureCollection)layer.DataSource).FeatureType.BaseType) == typeof(BranchStructure) &&
                    layer.DataSource.FeatureType != typeof(CompositeBranchStructure)) // Exclude composite branch structures
                {
                    layerList.Add(layer);
                }
            }
            var importBranchFeatureDialog = new ImportBranchFeatureDialog
                                          {
                                              DataSource = layerList,
                                              DisplayMember = "Name",
                                              Tolerance = 10.0
                                          };

            if (importBranchFeatureDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            Tolerance = importBranchFeatureDialog.Tolerance;

            var branchFeatureLayer = (ILayer)importBranchFeatureDialog.SelectedItem;

            if (branchFeatureLayer == null)
            {
                return;
            }
            
            var cursor = MapControl.Cursor;
            MapControl.Cursor = Cursors.WaitCursor;

            ImportSelectedFeaturesAsBranchFeatures(branchFeatureLayer);

            MapControl.Cursor = cursor;
            MapControl.Refresh();
        }

        

        public double Tolerance { get; set; }

        public void ImportSelectedFeaturesAsBranchFeatures(ILayer branchFeatureLayer)
        {
            if (!MapControl.SelectedFeatures.Any())
            {
                return;
            }

            var network = (IHydroNetwork) HydroNetworkMapLayer.Region;
            if (network.Branches.Count == 0)
            {
                Log.Warn("Network has no branches; selected features can not be connected.");
                return;
            }

            network.BeginEdit(HydroNetwork.ImportBranchesActionName);
            
            // import all selected features as branch features
            foreach (var feature in MapControl.SelectedFeatures)
            {
                // select no features from typeof BranchStructure
                if (!(feature.Geometry is Point))
                {
                    continue; // import only point features
                }

                // note GetNearestBranch uses tolerance in meters
                var nearestBranch = NetworkHelper.GetNearestBranch(network.Branches, feature.Geometry, Tolerance);

                if (nearestBranch != null)
                {
                    var featureProvider = branchFeatureLayer.DataSource;

                    // calculate new branch feature location
                    var coordinate = GeometryHelper.GetNearestPointAtLine((ILineString) nearestBranch.Geometry,
                                                                            feature.Geometry.Coordinate, Tolerance);
                    var newBranchFeatureGeometry = new Point(coordinate);

                    var newBranchFeature = (IBranchFeature) featureProvider.Add(newBranchFeatureGeometry);

                    var nameAttribute = feature.Attributes.Keys.FirstOrDefault(a => a.ToUpper().StartsWith("NAME"));

                    if (nameAttribute != default(string))
                    {
                        newBranchFeature.Name = (string) feature.Attributes[nameAttribute];
                    }
                }
                else
                {
                    Log.WarnFormat(
                        "The selected feature at location {0} can't be connected to the branch, no valid geometry within tolerance found",
                        feature.Geometry);
                }
            }

            network.EndEdit();
        }

        public override IEnumerable<MapToolContextMenuItem> GetContextMenuItems(Coordinate worldPosition)
        {
            if (!Enabled) yield break;

            yield return new MapToolContextMenuItem
                {
                    Priority = 5,
                    MenuItem = new ToolStripMenuItem("Import selected features to structure layer", null, ImportStructureEventHandler)
                };
        }

        private void ImportStructureEventHandler(object sender, EventArgs e)
        {
            Execute();
        }
    }
}
