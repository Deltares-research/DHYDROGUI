using GeoAPI.Extensions.Feature;
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
        protected override void OnGeometryChanged()
        {
            OutletCompatmentBoundaryFeature.Geometry = new Point(Geometry.Coordinate.X, Geometry.Coordinate.Y);
            //mick will make geometry; if not connected make point else if connected my nice 90 degree line
        }

        public OutletCompartment(string uniqueId) : base(uniqueId)
        {
        }

        [FeatureAttribute]
        public double SurfaceWaterLevel { get; set; }

        public Feature2D OutletCompatmentBoundaryFeature
        {
            get { return outletCompatmentBoundaryFeature; }
            set { outletCompatmentBoundaryFeature = value; }
        }
    }
}