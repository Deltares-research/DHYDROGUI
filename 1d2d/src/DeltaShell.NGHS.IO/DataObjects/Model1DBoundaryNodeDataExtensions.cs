using System.Linq;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.DataObjects
{
    public static class Model1DBoundaryNodeDataExtensions
    {
        public static void SetBoundaryConditionDataForOutlet(this Model1DBoundaryNodeData bc)
        {
            var manhole = bc?.Node as Manhole;
            var outlet = manhole?.Compartments.OfType<OutletCompartment>().FirstOrDefault();
         
            if (outlet == null)
            {
                return;
            }

            bc.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
            bc.WaterLevel = outlet.SurfaceWaterLevel;
            bc.OutletCompartment = outlet;
        }

        public static void UpdateManholeWithOutletData(this Model1DBoundaryNodeData flowBoundaryConditionData, INode node)
        {
            var manhole = node as Manhole;
            if (manhole != null && flowBoundaryConditionData.DataType == Model1DBoundaryNodeDataType.WaterLevelConstant)
            {
                //var outletCandidate = manhole.GetOutletCandidate(); // is not working. incomming branches are not set, but should be the method
                ICompartment outletCandidate = manhole.Compartments.OfType<OutletCompartment>().FirstOrDefault(o => o.Name.StartsWith("tmp"));
                if (outletCandidate != null)
                {
                    ((OutletCompartment) outletCandidate).SurfaceWaterLevel = flowBoundaryConditionData.WaterLevel;
                }
                else
                {
                    outletCandidate = manhole.Compartments.LastOrDefault();
                    if (outletCandidate != null)
                    {
                        var outlet = manhole.UpdateCompartmentToOutletCompartment(outletCandidate);
                        outlet.SurfaceWaterLevel = flowBoundaryConditionData.WaterLevel;
                        outlet.SurfaceLevel = flowBoundaryConditionData.WaterLevel + 0.25; //FM1D2D-1308 : empirical determination by Didrik
                        outlet.BottomLevel = flowBoundaryConditionData.WaterLevel - 1.0;   //FM1D2D-1308 : empirical determination by Didrik
                    }
                }
            }
        }
    }
}