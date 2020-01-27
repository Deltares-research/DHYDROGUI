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
        public OutletCompartment() : this("outletCompartment") { }

        public OutletCompartment(string name) : base(name)
        {
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