using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;
using SharpMap.Editors.Interactors.Network;
using SharpMap.Rendering;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors
{
    public class StructureInteractor<T> : PointInteractor,IBranchMaintainableInteractor,  INetworkFeatureInteractor where T : class, IStructure1D, new()
    {
        private bool moving;
        private bool addingNew;
        public INetwork Network { get; set; }

        public StructureInteractor(ILayer layer, IFeature feature, VectorStyle vectorStyle, IEditableObject editableObject)
            : base(layer, feature, vectorStyle, editableObject)
        {
        }

        private Coordinate CalculateCoordinate(IGeometry geometry)
        {
            if (Layer == null)
            {
                return (Coordinate)geometry.Coordinates[0].Clone();
            }

            var structure = (T) (moving ? TargetFeature : SourceFeature);

            int index = 0;
            int structureCount = 1;

            if (structure.ParentStructure != null)
            {
                var grouping = structure.ParentStructure.Structures.GroupBy(s => s.GetType()).ToList();
                index = grouping.Select(g => g.Key).ToList().IndexOf(structure.GetType());
                structureCount = grouping.Count;
            }
            
            var org = Layer.Map.ImageToWorld(new PointF(0, 0));
            var range = Layer.Map.ImageToWorld(new PointF(VectorStyle.Symbol.Width, VectorStyle.Symbol.Height));
            var anchor = geometry.Coordinates[0];
            
            var halfWidth = (range.X - org.X) / 2;
            var halfHeight = (range.Y - org.Y) / 2;

            var pointFeature = (IPointFeature) structure;
            var upwardTranslationFactor = -1;
            var downwardTranslationFactor = 1;
            
            if (pointFeature.ParentPointFeature != null)
            {
                var numberOfFeatures = pointFeature.ParentPointFeature?.GetPointFeatures().Count() ?? 1;

                var networkFeatureType = pointFeature.ParentPointFeature.NetworkFeatureType;
                PointFeatureRenderingHelper.DetermineTranslationFactorForStructures(networkFeatureType, numberOfFeatures, out upwardTranslationFactor, out downwardTranslationFactor);
            }

            var verticalTranslationFactor = (upwardTranslationFactor + downwardTranslationFactor) / 2.0;
            return new Coordinate(
                anchor.X - (halfWidth * (structureCount - 1)) + (2 * halfWidth * index),
                anchor.Y + verticalTranslationFactor * halfHeight);
        }

        public override void UpdateTracker(IGeometry geometry)
        {
            var coordinate = CalculateCoordinate(geometry);
            Trackers[0].Geometry.Coordinates[0].X = coordinate.X;
            Trackers[0].Geometry.Coordinates[0].Y = coordinate.Y;
        }

        public override void Start()
        {
            var structure = (T)SourceFeature;

            var geometry = ((IGeometry)structure.Geometry.Clone());

            TargetFeature = new T
            {
                Name = structure.Name,
                Geometry = geometry
            };
        }

        public override void Add(IFeature feature)
        {
            addingNew = true;
            Start();
            Stop();
            addingNew = false;
        }

        public override void Delete()
        {
            T t = (T)SourceFeature;
            HydroNetworkHelper.RemoveStructure(t);
            Layer.RenderRequired = true;
        }

        public override void Stop()
        {
            Stop(null);
        }

        public override void Stop(SnapResult snapResult)
        {
            Stop(snapResult,false);
        }

        public void Stop(SnapResult snapResult,bool stayOnSameBranch)
        {
            if (!addingNew && snapResult == null) // cancel
            {
                TargetFeature = null;
                moving = false;
                return;
            }
            
            var hydroNetwork = (IHydroNetwork)Network;

            if (Layer != null)
            {
                for (int i = 0; i < Layer.CustomRenderers.Count; i++)
                {
                    if (!(Layer.CustomRenderers[i] is StructureRenderer))
                        continue;
                    var structureRenderer = (StructureRenderer)Layer.CustomRenderers[i];
                    structureRenderer.Reset();
                }
            }

            var branchFeature = SourceFeature as IBranchFeature;
            if (branchFeature == null) return;

            if (moving)
            {
                branchFeature.SetBeingMoved(true);
            }

            if (!stayOnSameBranch)
            {
                var channel = (IChannel)branchFeature.Branch;
                if (null != channel)
                {
                    hydroNetwork = (IHydroNetwork)channel.Network;
                    channel.BranchFeatures.Remove(branchFeature);
                    branchFeature.Branch = null;
                }
            }

            base.Stop();

            var tolerance = Layer == null ? Tolerance : MapHelper.ImageToWorld(Layer.Map, 1);

            if (!stayOnSameBranch)
            {
                var branch = NetworkHelper.AddBranchFeatureToNearestBranch(hydroNetwork.Branches, branchFeature, tolerance);
                if (branch == null)
                {
                    return;
                }
            }

            NetworkHelper.UpdateBranchFeatureChainageFromGeometry(branchFeature);

            var structureToCompositeStructureTopology = new StructureToCompositeStructureTopology<IStructure1D>
                                                                                                          {
                                                                                                              Network = hydroNetwork,
                                                                                                              CompositeStructures = hydroNetwork.CompositeBranchStructures,
                                                                                                              Layer = Layer,
                                                                                                              Tolerance = Tolerance
                                                                                                          };
            structureToCompositeStructureTopology.OnStructureAdded((IStructure1D)SourceFeature, snapResult);

            if (moving)
            {
                branchFeature.SetBeingMoved(false);
                moving = false;
            }
        }

        public override bool MoveTracker(TrackerFeature trackerFeature, double deltaX, double deltaY, SnapResult snapResult = null)
        {
            moving = true;
            var moveTracker = base.MoveTracker(trackerFeature, deltaX, deltaY, snapResult);
            UpdateTracker(TargetFeature.Geometry);
            return moveTracker;
        }

        protected override bool AllowMoveCore()
        {
            return true;
        }

        protected override bool AllowDeletionCore()
        {
            return true;
        }

        public override bool AllowSingleClickAndMove()
        {
            return true;
        }
    }
}