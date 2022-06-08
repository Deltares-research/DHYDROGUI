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
            AddCapacityProperty(pump);

            return IniCategory;
        }

        private void AddCapacityProperty(IPump pump)
        {
            var capacityDescription = StructureRegion.Capacity.Description;
            if (pump.CanBeTimedependent && pump.UseCapacityTimeSeries)
            {
                var timeSeriesFileName = $"{pump.Name}_{StructureRegion.Capacity.Key}{FileSuffices.TimFile}";
                IniCategory.AddProperty(StructureRegion.Capacity.Key, timeSeriesFileName, capacityDescription);
            }
            else
            {
                IniCategory.AddProperty(StructureRegion.Capacity.Key, pump.Capacity, capacityDescription, StructureRegion.Capacity.Format);
            }
        }
    }
}