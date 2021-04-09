using System.ComponentModel;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro.SewerFeatures
{
    [Entity]
    public class OutletCompartment : Compartment
    {
        public OutletCompartment() : this("outletCompartment") { }

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
        [DisplayName("Surface water level")]
        public double SurfaceWaterLevel { get; set; }

        #region GUI
        // Hide these properties in the Outlets MDE
        public override CompartmentShape Shape { get => base.Shape; set => base.Shape = value; }
        public override double ManholeWidth { get => base.ManholeWidth; set => base.ManholeWidth = value; }
        public override double FloodableArea { get => base.FloodableArea; set => base.FloodableArea = value; }
        public override double ManholeLength { get => base.ManholeLength; set => base.ManholeLength = value; }
        public override double SurfaceLevel { get => base.SurfaceLevel; set => base.SurfaceLevel = value; }
        public override double BottomLevel { get => base.BottomLevel; set => base.BottomLevel = value; }
        public override bool UseTable { get => base.UseTable; set => base.UseTable = value; }
        public override IFunction Storage { get => base.Storage; set => base.Storage = value; }
        public override InterpolationType InterpolationType { get => base.InterpolationType; set => base.InterpolationType = value; }
        #endregion

    }
}