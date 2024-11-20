using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Extensions.Networks;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;
using SharpMap.Editors.Interactors.Network;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors
{
    /// <summary>
    /// Editor used for lateral sources that are diffuse 
    /// </summary>
    public class DiffuseLateralSourceInteractor : LineStringInteractor, INetworkFeatureInteractor
    {
        public DiffuseLateralSourceInteractor(ILayer layer, IFeature feature, VectorStyle vectorStyle, IEditableObject editableObject)
            : base(layer, feature, vectorStyle, editableObject){}

        public INetwork Network{ get; set; }

        protected override bool AllowMoveCore()
        {
            return true;
        }

        protected override bool AllowDeletionCore()
        {
            return true;
        }

        protected override void CreateTrackers()
        {
            var lateralSource = SourceFeature as ILateralSource;

            if (lateralSource == null)
            {
                return;
            }

            if (!lateralSource.IsDiffuse)
            {
                throw new InvalidOperationException("Attempt to use diffuse lateral source interactor for pointlike lateral");
            }

            base.CreateTrackers();
        }

        public override void UpdateTracker(IGeometry geometry)
        {
            if (geometry.Coordinates.Length != Trackers.Count)
            {
                return;
            }
            var index = 0;
            foreach (var trackerGeometry in Trackers.Select(t => t.Geometry))
            {
                trackerGeometry.Coordinates[0].X = geometry.Coordinates[index].X;
                trackerGeometry.Coordinates[0].Y = geometry.Coordinates[index].Y;
                trackerGeometry.EnvelopeInternal.Init(trackerGeometry.Coordinates[0]);
                ++index;
            }
        }

        public override void Stop()
        {
            var lateralSource = SourceFeature as ILateralSource;
            if (lateralSource == null) 
                return;
            
            lateralSource.SetBeingMoved(true);
            
            base.Stop();

            var targetBranchFeature = (IBranchFeature) SourceFeature;
            if (!Equals(lateralSource.Branch, targetBranchFeature.Branch))
            {
                NetworkHelper.AddBranchFeatureToBranch(lateralSource, targetBranchFeature.Branch, targetBranchFeature.Chainage);
            }
            else
            {
                lateralSource.Chainage = ((IBranchFeature)TargetFeature).Chainage;
            }
            
            lateralSource.SetBeingMoved(false);
        }

        public override bool MoveTracker(TrackerFeature trackerFeature, double deltaX, double deltaY,
                                         SnapResult snapResult = null)
        {
            var lateralSource = TargetFeature as ILateralSource;

            if (lateralSource == null || !lateralSource.IsDiffuse)
            {
                return false;
            }

            var newBranch = snapResult == null ? lateralSource.Branch : (IBranch)snapResult.SnappedFeature;

            var oldLocation = trackerFeature == AllTracker
                                  ? TargetFeature.Geometry.Coordinates[0]
                                  : trackerFeature.Geometry.Coordinates[0];

            var newLocation = snapResult == null
                                  ? new Coordinate(oldLocation.X + deltaX, oldLocation.Y + deltaY, oldLocation.Z)
                                  : snapResult.Location;

            var chainage = GeometryHelper.Distance((ILineString) newBranch.Geometry, newLocation);

            lateralSource.Chainage = BranchFeature.SnapChainage(newBranch.Length,
                                                                NetworkHelper.CalculationChainage(newBranch, chainage));
            
            if (!Equals(lateralSource.Branch, newBranch))
            {
                lateralSource.Branch = newBranch;
            }

            var oldCoordinateCount = lateralSource.Geometry.Coordinates.Length;

            NetworkHelper.UpdateLineGeometry(lateralSource, newBranch.Geometry);

            var newCoordinateCount = lateralSource.Geometry.Coordinates.Length;
            
            if (oldCoordinateCount != newCoordinateCount)
            {
                Trackers.Clear();
                Trackers.AddRange(CreateTrackersForGeometry(TargetFeature.Geometry).ToList());
            }
            else
            {
                UpdateTracker(TargetFeature.Geometry);
            }
            return true;
        }

        // No individual tracker selection supported, 
        // when moving along a branch the geometry coordinate number might change.
        public override TrackerFeature GetTrackerAtCoordinate(Coordinate worldPos)
        {
            var trackerFeature = base.GetTrackerAtCoordinate(worldPos);
            if (trackerFeature == null) return null;
            
            foreach (var tracker in Trackers)
            {
                tracker.Selected = true;
            }
            return AllTracker;
        }
    }
}