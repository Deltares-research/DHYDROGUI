using System.Collections.Generic;
using System.Linq;
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

        public CatchmentFeatureInteractor(ILayer layer, IFeature feature, VectorStyle vectorStyle, DrainageBasin basin) : base(layer, feature, vectorStyle, basin)
        {
            DrainageBasin = basin;
        }

        public DrainageBasin DrainageBasin { get; set; }

        public override IEnumerable<IFeatureRelationInteractor> GetFeatureRelationInteractors(IFeature feature)
        {
            yield return new HydroObjectToHydroLinkRelationInteractor();
            yield return new SubCatchmentRelationInteractor();
        }

        public override bool MoveTracker(TrackerFeature trackerFeature, double deltaX, double deltaY,
                                         SnapResult snapResult = null)
        {
            // check that, if we're a sub-catchment, we do not leave the parent geometry
            var catchment = (Catchment) SourceFeature;
            if (catchment.Geometry is IPoint)
            {
                var targetPoint = TargetFeature.Geometry as IPoint;

                //we're moving a sub-catchment directly: keep it within parent
                Catchment parentCatchment = GetParentCatchment(catchment); //SLOW!

                if (parentCatchment != null)
                {
                    var newGeometry = new Point(targetPoint.X + deltaX, targetPoint.Y + deltaY);
                    if (!newGeometry.Intersects(parentCatchment.Geometry))
                    {
                        return false; //do not allow to go outside parent
                    }
                }
            }

            // do the move
            bool movedTracker = base.MoveTracker(trackerFeature, deltaX, deltaY, snapResult);

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
                var catchment = (Catchment) SourceFeature;
                if (catchment.IsGeometryDerivedFromAreaSize)
                {
                    catchment.IsGeometryDerivedFromAreaSize = false;
                }
            }

            base.Stop();
        }

        public override void Delete()
        {
            var catchment = (Catchment) SourceFeature;

            // we get here for both the catchment center and the catchment geometry

            if (DrainageBasin.Catchments.Contains(catchment))
            {
                DrainageBasin.Catchments.Remove(catchment);
            }
            else
            {
                Catchment parentCatchment = GetParentCatchment(catchment);
                if (parentCatchment != null)
                {
                    parentCatchment.SubCatchments.Remove(catchment);
                }
            }
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

        protected override bool AllowDeletionCore()
        {
            return true;
        }

        protected override bool AllowMoveCore()
        {
            return true;
        }

        private Catchment GetParentCatchment(Catchment catchment)
        {
            return DrainageBasin.Catchments.FirstOrDefault(c => c.SubCatchments.Contains(catchment));
        }

        private static Polygon GetBoundingPolygon(IGeometry catchmentGeometry)
        {
            //todo..use concave (!) hull algorithm?
            Coordinate[] boundaryCoordinates = catchmentGeometry.Coordinates;
            return new Polygon(new LinearRing(boundaryCoordinates));
        }
    }
}