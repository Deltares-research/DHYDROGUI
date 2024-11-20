using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;

namespace DeltaShell.NGHS.IO.DataObjects
{
    // HACK: DOH!!!! REMOVE IT!
    public class FlowWaterLevelTable : Function
    {
        public FlowWaterLevelTable()
        {
            Arguments.Add(new Variable<double>("Water Level", new Unit("m", "m")));
            Components.Add(new Variable<double>("Discharge", new Unit("m³/s", "m³/s")));
            
            Arguments[0].InterpolationType = InterpolationType.Linear;
            Arguments[0].ExtrapolationType = ExtrapolationType.Constant;

            Components[0].Attributes[FunctionAttributes.StandardName] = FunctionAttributes.StandardNames.WaterLevelTable;
            Attributes[FunctionAttributes.LocationType] = FunctionAttributes.QhBoundary;

            Name = "Discharge Water Level Series";
        }
        
    }
}