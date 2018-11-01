using System;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro.SewerFeatures
{
    public class OutletCompartment : Compartment
    {
        private Feature2D outletCompartmentBoundaryFeature = new Feature2D();

        public OutletCompartment() : this("outletCompartment")
        {
            
        }

        public OutletCompartment(string name) : base(name)
        {
        }

        protected override void OnNameChanged()
        {
            outletCompartmentBoundaryFeature.Name = Name;
        }

        [FeatureAttribute]
        public double SurfaceWaterLevel { get; set; }

        public Feature2D OutletCompartmentBoundaryFeature
        {
            get { return outletCompartmentBoundaryFeature; }
            set { outletCompartmentBoundaryFeature = value; }
        }

        public void SetBoundaryGeometry(Coordinate sourceCoordinate, Coordinate targetCoordinate)
        {
            OutletCompartmentBoundaryFeature.Geometry = CreateBoundaryGeometryForOutletCompartment(sourceCoordinate, targetCoordinate);
        }

        private static LineString CreateBoundaryGeometryForOutletCompartment(Coordinate sourceCoordinate, Coordinate targetCoordinate)
        {
            Coordinate boundaryStartCoordinate;
            Coordinate boundaryEndCoordinate;

            var deltaX = sourceCoordinate.X - targetCoordinate.X;
            var deltaY = sourceCoordinate.Y - targetCoordinate.Y;
            if (Math.Abs(deltaX) > 10e-6 && Math.Abs(deltaY) > 10e-6)
            {
                var slope = deltaY / deltaX;
                var perpendicularSlope = -1 / slope;
                var deltaToBoundaryMiddleX = 0.5 / Math.Sqrt(1 + slope * slope);
                var deltaToBoundaryMiddleY = deltaToBoundaryMiddleX * slope;

                var middleBoundaryCoordinate = sourceCoordinate.X < targetCoordinate.X
                    ? new Coordinate(targetCoordinate.X + deltaToBoundaryMiddleX, targetCoordinate.Y + deltaToBoundaryMiddleY)
                    : new Coordinate(targetCoordinate.X - deltaToBoundaryMiddleX, targetCoordinate.Y - deltaToBoundaryMiddleY);

                var deltaToBoundaryEndsX = 5 / Math.Sqrt(1 + perpendicularSlope * perpendicularSlope);
                var deltaToBoundaryEndsY = deltaToBoundaryEndsX * perpendicularSlope;

                boundaryStartCoordinate = new Coordinate(middleBoundaryCoordinate.X + deltaToBoundaryEndsX, middleBoundaryCoordinate.Y + deltaToBoundaryEndsY);
                boundaryEndCoordinate = new Coordinate(middleBoundaryCoordinate.X - deltaToBoundaryEndsX, middleBoundaryCoordinate.Y - deltaToBoundaryEndsY);
            }
            else if (Math.Abs(deltaX) < 10e-6)
            {
                var middleBoundaryCoordinate = sourceCoordinate.Y < targetCoordinate.Y
                    ? new Coordinate(targetCoordinate.X, targetCoordinate.Y + 0.5)
                    : new Coordinate(targetCoordinate.X, targetCoordinate.Y - 0.5);

                boundaryStartCoordinate = new Coordinate(middleBoundaryCoordinate.X - 5, middleBoundaryCoordinate.Y);
                boundaryEndCoordinate = new Coordinate(middleBoundaryCoordinate.X + 5, middleBoundaryCoordinate.Y);
            }
            else //(Math.Abs(deltaY) < 10e-6)
            {
                var middleBoundaryCoordinate = sourceCoordinate.X < targetCoordinate.X
                    ? new Coordinate(targetCoordinate.X + 0.5, targetCoordinate.Y)
                    : new Coordinate(targetCoordinate.X - 0.5, targetCoordinate.Y);

                boundaryStartCoordinate = new Coordinate(middleBoundaryCoordinate.X, middleBoundaryCoordinate.Y - 5);
                boundaryEndCoordinate = new Coordinate(middleBoundaryCoordinate.X, middleBoundaryCoordinate.Y + 5);
            }

            return new LineString(new[] { boundaryStartCoordinate, boundaryEndCoordinate });
        }
    }
}