using System.Collections.Generic;
using DelftTools.Hydro;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors
{
    public class CatchmentFeatureInteractor : LinearRingInteractor
    {
        private bool wasMoved;

        public override IEnumerable<IFeatureRelationInteractor> GetFeatureRelationInteractors(IFeature feature)
        {
            yield return new HydroObjectToHydroLinkRelationInteractor();
        }

        public CatchmentFeatureInteractor(ILayer layer, IFeature feature, VectorStyle vectorStyle, IDrainageBasin basin) : base(layer, feature, vectorStyle, basin)
        {
            DrainageBasin = basin;
        }

        public override bool MoveTracker(TrackerFeature trackerFeature, double deltaX, double deltaY,
                                         SnapResult snapResult = null)
        {
            // do the move
            var movedTracker = base.MoveTracker(trackerFeature, deltaX, deltaY, snapResult);

            if (movedTracker && trackerFeature.Index != -1)
            {
                wasMoved = true;
            }

            return movedTracker;
        }

        public override void Stop()
        {
            if (wasMoved)
            {
                wasMoved = false;
                // set catchment default geometry to false: the user has customized it by moving some trackers
                var catchment = ((Catchment) SourceFeature);
                if (catchment.IsGeometryDerivedFromAreaSize)
                    catchment.IsGeometryDerivedFromAreaSize = false;
            }
            base.Stop();
        }

        public override void Delete()
        {
            var catchment = (Catchment) SourceFeature;
            DrainageBasin.Catchments.Remove(catchment);
        }
        
        protected override bool AllowDeletionCore()
        {
            return true;
        }

        protected override bool AllowMoveCore()
        {
            return true;
        }
        
        public override void Add(IFeature feature) // TODO: not used?!?
        {
            base.Add(feature);
            if (!(feature is Catchment))
            {
                return;
            }

            var catchment = feature as Catchment;

            if (catchment.Geometry.Coordinates.Length <= 3)
            {
                catchment.IsGeometryDerivedFromAreaSize = true;
                catchment.SetAreaSize(1000.0);
            }
            else
            {
                catchment.Geometry = GetBoundingPolygon(catchment.Geometry);
            }
        }

        private static Polygon GetBoundingPolygon(IGeometry catchmentGeometry)
        {
            //todo..use concave (!) hull algorithm?
            var boundaryCoordinates = catchmentGeometry.Coordinates;
            return new Polygon(new LinearRing(boundaryCoordinates));
        }

        public IDrainageBasin DrainageBasin { get; set; }
    }
}