using System;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro.SewerFeatures
{
    [Entity]
    public class OutletCompartment : Compartment
    {

        public OutletCompartment(string name) : base(name)
        {
        }

        public OutletCompartment(ICompartment compartment) : this("outletCompartment")
        {
            Name = compartment.Name;
            ParentManhole = compartment.ParentManhole;
            ParentManholeName = compartment.ParentManholeName;
            SurfaceLevel = compartment.SurfaceLevel;
            ManholeLength = compartment.ManholeLength;
            ManholeWidth = compartment.ManholeWidth;
            FloodableArea = compartment.FloodableArea;
            BottomLevel = compartment.BottomLevel;
            Geometry = compartment.Geometry;
            Shape = compartment.Shape;
        }

        [FeatureAttribute]
        public double SurfaceWaterLevel { get; set; }

        public void TakeConnectionsOverFrom(ICompartment compartment)
        {
            var hydroNetwork = ParentManhole?.HydroNetwork;
            if (hydroNetwork != null)
            {
                ReconnectSewerConnections(compartment, hydroNetwork);
            }

        }

    }
}