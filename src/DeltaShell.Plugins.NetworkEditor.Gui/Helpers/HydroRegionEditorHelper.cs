using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using SharpMap.Editors.FallOff;
using SharpMap.UI.Forms;
using GeometryFactory = SharpMap.Converters.Geometries.GeometryFactory;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Helpers
{
    public static class HydroRegionEditorHelper
    {
        /// <summary>
        /// Initializes the network interactor if a network layer is part of the map. This method
        /// is called when a mapview gains or looses focus.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="mapControl"></param>
        /// TODO, HACK: project argument must be removed!
        public static HydroRegionEditorMapTool AddHydroRegionEditorMapTool(IMapControl mapControl)
        {
            if (mapControl == null)
            {
                return null;
            }

            if (mapControl.Map == null)
            {
                return null;
            }

            var hydroRegionEditorMapTool = mapControl.GetToolByType<HydroRegionEditorMapTool>();

            // networkeditor already present; nothing to do.
            if (hydroRegionEditorMapTool != null)
            {
                return hydroRegionEditorMapTool;
            }

            hydroRegionEditorMapTool = new HydroRegionEditorMapTool
            {
                IsActive = true,
                TopologyRulesEnabled = true
            };

            mapControl.Tools.Add(hydroRegionEditorMapTool);

            return hydroRegionEditorMapTool;
        }

        public static void RemoveHydroRegionEditorMapTool(IMapControl mapControl)
        {
            var hydroRegionEditorMapTool = mapControl.GetToolByType<HydroRegionEditorMapTool>();
            if (hydroRegionEditorMapTool != null)
            {
                //set the mapcontrol to null to be sure the tool unsubscribes from map
                hydroRegionEditorMapTool.MapControl = null;
                mapControl.Tools.Remove(hydroRegionEditorMapTool);
            }
        }

        public static void UpdateBranchFeatureGeometry(IBranchFeature branchFeature, double calculationLength)
        {
            object selection = NetworkEditorGuiPlugin.Instance == null || NetworkEditorGuiPlugin.Instance.Gui == null ? null : NetworkEditorGuiPlugin.Instance.Gui.Selection;
            IBranch branch = branchFeature.Branch;
            if (calculationLength < 0)
            {
                throw new ArgumentException("Length can not be negative.", "calculationLength");
            }

            if (calculationLength > branch.Length)
            {
                throw new ArgumentException("Length can not exceed the length of the channel.", "calculationLength");
            }

            if (calculationLength + branchFeature.Chainage > branch.Length)
            {
                throw new ArgumentException("Combined length and chainage can not exceed the length of the channel.", "calculationLength");
            }

            branchFeature.Network.BeginEdit(string.Format("Update Line Geometry of branchfeature {0}", branchFeature));

            branchFeature.Length = calculationLength;

            if (calculationLength > 0)
            {
                NetworkHelper.UpdateLineGeometry(branchFeature, branch.Geometry);
            }
            else if (Math.Abs(calculationLength - 0.0) < double.Epsilon) //== 0.0
            {
                var lengthIndexedLine = new LengthIndexedLine(branch.Geometry);
                double mapOffset = NetworkHelper.MapChainage(branch, branchFeature.Chainage);
                branchFeature.Geometry = new Point((Coordinate) lengthIndexedLine.ExtractPoint(mapOffset).Clone());
            }

            branchFeature.Network.EndEdit();

            if (NetworkEditorGuiPlugin.Instance != null && NetworkEditorGuiPlugin.Instance.Gui != null)
            {
                NetworkEditorGuiPlugin.Instance.Gui.Selection = selection; // HACK: Lourens, THIS IS REALLY UGLY!
            }
        }

        public static IHydroRegion RootGetHydroRegion(MapView view)
        {
            return view.Map.GetAllVisibleLayers(true).OfType<HydroRegionMapLayer>().Select(l => l.Region).FirstOrDefault(r => r != null && r.Parent == null);
        }

        /// <summary>
        /// Move node to a new position
        /// MoveNodeTo uses the HydroNetworkMapLayer of the mapcontrol and
        /// </summary>
        /// <param name="mapControl"></param>
        /// <param name="node"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        [Obsolete("HACK")]
        public static void MoveNodeTo(INode node, double x, double y)
        {
            IPoint newGeometry = GeometryFactory.CreatePoint(x, y);
            foreach (IBranch branch in node.IncomingBranches)
            {
                if (branch.Source.Geometry.Distance(newGeometry) < 1.0e-8)
                {
                    throw new ArgumentException(string.Format(
                                                    "Node movement to ({0:0.##},{1:0.##}) will result in length {2:0.##} of branch {3}; this is not allowed",
                                                    x, y, branch.Source.Geometry.Distance(newGeometry), branch.Name));
                }
            }

            foreach (IBranch branch in node.OutgoingBranches)
            {
                if (branch.Target.Geometry.Distance(newGeometry) < 1.0e-8)
                {
                    throw new ArgumentException(string.Format(
                                                    "Node movement to ({0:0.##},{1:0.##}) will result in length {2:0.##} of branch {3}; this is not allowed",
                                                    x, y, branch.Target.Geometry.Distance(newGeometry), branch.Name));
                }
            }

            try
            {
                node.Network.BeginEdit(string.Format("Move node {0}", node));

                var nodeEditor = new HydroNodeInteractor(null, node, null, null) {FallOffPolicy = new LinearFallOffPolicy()};
                nodeEditor.Start();
                double deltaX = x - node.Geometry.Coordinates[0].X;
                double deltaY = y - node.Geometry.Coordinates[0].Y;
                nodeEditor.MoveTracker(nodeEditor.Trackers[0], deltaX, deltaY);
                nodeEditor.Stop();
            }
            finally
            {
                node.Network.EndEdit();
            }
        }
    }
}