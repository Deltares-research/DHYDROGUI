using System.Linq;
using DelftTools.Hydro.SewerFeatures;

namespace DeltaShell.NGHS.IO.DataObjects
{
    public static class Model1DBoundaryNodeDataExtensions
    {
        public static void SetBoundaryConditionDataForOutlet(this Model1DBoundaryNodeData bc)
        {
            var manhole = bc?.Node as Manhole;
            if (manhole != null && manhole.Compartments.OfType<OutletCompartment>().Any())
            {
                var outlet = manhole.Compartments.OfType<OutletCompartment>().First();
                bc.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
                bc.WaterLevel = outlet.SurfaceWaterLevel;
            }
        }
    }
}