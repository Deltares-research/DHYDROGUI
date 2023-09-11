using System.ComponentModel;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.ComponentModel;
using DHYDRO.Common.Logging;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro.SewerFeatures
{
    [Entity]
    public class OutletCompartment : Compartment
    {
        public OutletCompartment(ILogHandler logHandler, string name) : base(logHandler, name)
        {
        }
        public OutletCompartment() : this("outletCompartment") { }

        public OutletCompartment(string name) : base(name)
        {
        }

        public OutletCompartment(ICompartment compartment) : this("outletCompartment")
        {
            Name = compartment.Name;
            CopyFrom(compartment);
        }
        
        [FeatureAttribute]
        [DisplayName("Surface water level")]
        public double SurfaceWaterLevel { get; set; }
    }
}