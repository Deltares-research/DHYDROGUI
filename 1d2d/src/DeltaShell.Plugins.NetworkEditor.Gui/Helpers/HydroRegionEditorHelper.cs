using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using SharpMap.Api.Editors;
using SharpMap.Editors.FallOff;
using SharpMap.Editors.Interactors.Network;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Helpers
{
    public static class HydroRegionEditorHelper
    {
        /// <summary>
        /// Initializes the network interactor if a network layer is part of the map. This method
        /// is called when a mapview gains or looses focus.
        /// </summary>
        /// <param name="mapControl"></param>
        public static HydroRegionEditorMapTool AddHydroRegionEditorMapTool(IMapControl mapControl)
        {
            if (mapControl == null) return null;
            if (mapControl.Map == null) return null;

            var hydroRegionEditorMapTool = mapControl.GetToolByType<HydroRegionEditorMapTool>();
            
            // networkeditor already present; nothing to do.
            if (hydroRegionEditorMapTool != null) 
                return hydroRegionEditorMapTool;

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

        /// <summary>
        /// Changes <c>Offset</c> of <paramref name="branchFeature"/> and related features as <see cref="GeometryHelper.Distance(ILineString , ICoordinate )"/> 
        /// produces rounded results. Editors, like 'BranchFeatureInteractor', set <paramref name="branchFeature"/> <c>Offset</c> to this rounded value, resulting in
        /// integer values as <paramref name="chainage"/> to end up as doubles, which is unwanted behavior.
        /// </summary>
        /// <param name="branchFeature"></param>
        /// <param name="chainage"></param>
        private static void EnsureInputChainageIsAsExpected(IBranchFeature branchFeature, double chainage)
        {
            branchFeature.Chainage = chainage;

            if (branchFeature is ICompositeBranchStructure)
            {
                foreach (var structure in ((ICompositeBranchStructure)branchFeature).Structures)
                {
                    structure.Chainage = chainage;
                }
            }

            if (branchFeature is IStructure1D && ((IStructure1D)branchFeature).ParentStructure != null)
            {
                ((IStructure1D)branchFeature).ParentStructure.Chainage = chainage;
            }
        }

        private static void ValidateChainage(double chainage, IBranchFeature branchFeature)
        {
            if (chainage < 0)
            {
                throw new ArgumentException("Chainage can not be negative.", "chainage");
            }

            if (chainage > branchFeature.Branch.Length)
            {
                throw new ArgumentException("Chainage can not exceed the length of the channel.", "chainage");
            }
            if (chainage + branchFeature.Length > branchFeature.Branch.Length)
            {
                throw new ArgumentException("Combined length and chainage can not exceed the length of the channel.", "chainage");
            }
        }

        private static Coordinate GetCoordinateForBranchFeature(IBranchFeature branchFeature, double offset)
        {
            if (branchFeature.Branch.IsLengthCustom)
            {
                offset *= (branchFeature.Branch.Geometry.Length / branchFeature.Branch.Length);
            }
            return GeometryHelper.LineStringCoordinate((ILineString)branchFeature.Branch.Geometry,
                                                       offset);
        }

        // HACK: this method looks like a total hack, getting plugin instance, creating editors without layers. It does not use a "normal" way to edit features.
        public static void MoveBranchFeatureTo(IBranchFeature branchFeature, double chainage, bool forceMaintainBranch = true)
        {
            var selection = (NetworkEditorGuiPlugin.Instance == null) ? null : NetworkEditorGuiPlugin.Instance.Gui.Selection;
            chainage = BranchFeature.SnapChainage(branchFeature.Branch.Length, chainage);
            ValidateChainage(chainage, branchFeature);

            branchFeature.Network.BeginEdit(string.Format("Move branchfeature {0}", branchFeature));
            // moving branchFeature will remove it from and add it to collection; save selection and restore 
            // when done.
            try
            {
                var e = new HydroNetworkFeatureEditor(branchFeature.Network);

                var interactor = e.CreateInteractor(null, branchFeature);
                interactor.Tolerance = 0.5;

                var oldCoordinate = (Coordinate)branchFeature.Geometry.Coordinates[0].Clone();
                var newCoordinate = GetCoordinateForBranchFeature(branchFeature, chainage);

                var deltaX = newCoordinate.X - oldCoordinate.X;
                var deltaY = newCoordinate.Y - oldCoordinate.Y;
                interactor.Start();
                var tracker = interactor.Trackers.FirstOrDefault(); // Not OK: what about extended, deformable branch features?
                if (tracker != null)
                {
                    interactor.MoveTracker(tracker, deltaX, deltaY);
                }

                var snapResult = new SnapResult(interactor.TargetFeature.Geometry.Coordinate, branchFeature.Branch, interactor.Layer, branchFeature.Branch.Geometry,0,0);
                //if the interactor can maintain the interactor we want that (when only changing offset)
                if (interactor is IBranchMaintainableInteractor)
                {
                    (interactor as IBranchMaintainableInteractor).Stop(snapResult, forceMaintainBranch);
                }
                else
                {
                    interactor.Stop(snapResult);
                }
                // Ensure offset is as specified (Unaffected by rounding), especially noticeable for integer 'chainage'
                EnsureInputChainageIsAsExpected(branchFeature, chainage);

                if (NetworkEditorGuiPlugin.Instance != null)
                {
                    NetworkEditorGuiPlugin.Instance.Gui.Selection = selection;
                }
            }
            finally
            {
                if (branchFeature.Network != null)
                {
                    branchFeature.Network.EndEdit();
                }
            }
        }

        public static void UpdateBranchFeatureGeometry(IBranchFeature branchFeature, double calculationLength)
        {
            var selection = (NetworkEditorGuiPlugin.Instance == null || NetworkEditorGuiPlugin.Instance.Gui == null) ? null : NetworkEditorGuiPlugin.Instance.Gui.Selection;
            var branch = branchFeature.Branch;
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
                var mapOffset = NetworkHelper.MapChainage(branch, branchFeature.Chainage);
                branchFeature.Geometry = new Point((Coordinate)lengthIndexedLine.ExtractPoint(mapOffset).Clone());
            }

            branchFeature.Network.EndEdit();

            if (NetworkEditorGuiPlugin.Instance != null && NetworkEditorGuiPlugin.Instance.Gui != null)
            {
                NetworkEditorGuiPlugin.Instance.Gui.Selection = selection; // HACK: Lourens, THIS IS REALLY UGLY!
            }
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
            var newGeometry = new Point(x, y);
            foreach (var branch in node.IncomingBranches)
            {
                if (branch.Source.Geometry.Distance(newGeometry) < 1.0e-8)
                {
                    throw new ArgumentException(string.Format(
                        "Node movement to ({0:0.##},{1:0.##}) will result in length {2:0.##} of branch {3}; this is not allowed",
                        x, y, branch.Source.Geometry.Distance(newGeometry), branch.Name));
                }
            }
            foreach (var branch in node.OutgoingBranches)
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

                var nodeEditor = new HydroNodeInteractor(null, node, null, null) { FallOffPolicy = new LinearFallOffPolicy() };
                nodeEditor.Start();
                var deltaX = x - node.Geometry.Coordinates[0].X;
                var deltaY = y - node.Geometry.Coordinates[0].Y;
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