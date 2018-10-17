using System;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro.SewerFeatures
{
    public class OutletCompartment : Compartment
    {
        private Feature2D outletCompatmentBoundaryFeature = new Feature2D();

        public OutletCompartment() : this("outletCompartment")
        {
            
        }

        public OutletCompartment(string name) : base(name)
        {
        }

        protected override void OnNameChanged()
        {
            outletCompatmentBoundaryFeature.Name = Name;
        }

        [FeatureAttribute]
        public double SurfaceWaterLevel { get; set; }

        public Feature2D OutletCompatmentBoundaryFeature
        {
            get { return outletCompatmentBoundaryFeature; }
            set { outletCompatmentBoundaryFeature = value; }
        }

        public void SetBoundaryGeometry(Coordinate sourceCoordinate, Coordinate targetCoordinate)
        {
            OutletCompatmentBoundaryFeature.Geometry = CreateBoundaryGeometryForOutletCompartment(sourceCoordinate, targetCoordinate);
        }

        private static LineString CreateBoundaryGeometryForOutletCompartment(Coordinate sourceCoordinate, Coordinate targetCoordinate)
        {
            var deltaX = sourceCoordinate.X - targetCoordinate.X;
            if (Math.Abs(deltaX) > 10e-6)
            {
                var slope = (sourceCoordinate.Y - targetCoordinate.Y) / deltaX;
                var perpendicularSlope = -1 / slope;
                var deltaToBoundaryMiddleX = 0.5 / Math.Sqrt(1 + slope * slope);
                var deltaToBoundaryMiddleY = deltaToBoundaryMiddleX * slope;

                var middleBoundaryCoordinate = sourceCoordinate.X < targetCoordinate.X
                    ? new Coordinate(targetCoordinate.X + deltaToBoundaryMiddleX, targetCoordinate.Y + deltaToBoundaryMiddleY)
                    : new Coordinate(targetCoordinate.X - deltaToBoundaryMiddleX, targetCoordinate.Y - deltaToBoundaryMiddleY);

                var deltaToBoundaryEndsX = 5 / Math.Sqrt(1 + perpendicularSlope * perpendicularSlope);
                var deltaToBoundaryEndsY = deltaToBoundaryEndsX * perpendicularSlope;

                var boundaryStartCoordinate = new Coordinate(middleBoundaryCoordinate.X + deltaToBoundaryEndsX, middleBoundaryCoordinate.Y + deltaToBoundaryEndsY);
                var boundaryEndCoordinate = new Coordinate(middleBoundaryCoordinate.X - deltaToBoundaryEndsX, middleBoundaryCoordinate.Y - deltaToBoundaryEndsY);
                return new LineString(new[] { boundaryStartCoordinate, boundaryEndCoordinate });
            }
            else
            {
                var middleBoundaryCoordinate = sourceCoordinate.Y < targetCoordinate.Y
                    ? new Coordinate(targetCoordinate.X, targetCoordinate.Y + 0.5)
                    : new Coordinate(targetCoordinate.X, targetCoordinate.Y - 0.5);

                var boundaryStartCoordinate = new Coordinate(middleBoundaryCoordinate.X - 5, middleBoundaryCoordinate.Y);
                var boundaryEndCoordinate = new Coordinate(middleBoundaryCoordinate.X + 5, middleBoundaryCoordinate.Y);
                return new LineString(new[] { boundaryStartCoordinate, boundaryEndCoordinate });
            }
        }
    }
}