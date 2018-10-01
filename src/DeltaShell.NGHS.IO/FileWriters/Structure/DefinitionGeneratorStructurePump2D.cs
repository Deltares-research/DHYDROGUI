using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructurePump2D : DefinitionGeneratorStructure2D
    {
        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.Pump);

            var pump = (IPump) hydroObject;
            var capacityKey = StructureRegion.Capacity.Key;
            var capacityDescription = StructureRegion.Capacity.Description;

            if (pump.CanBeTimedependent && pump.UseCapacityTimeSeries)
            {
                var timeSeriesFileName = $"{pump.Name}_{capacityKey}.tim";
                IniCategory.AddProperty(capacityKey, timeSeriesFileName, capacityDescription);
            }
            else
            {
                IniCategory.AddProperty(capacityKey, pump.Capacity, capacityDescription, "F");
            }

            return IniCategory;
        }
    }
}