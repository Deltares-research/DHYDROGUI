using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructurePump2D : DefinitionGeneratorTimeSeriesStructure2D
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
            AddProperty(pump.CanBeTimedependent && pump.UseCapacityTimeSeries,
                        StructureRegion.Capacity.Key,
                        pump.Capacity,
                        StructureRegion.Capacity.Description,
                        StructureRegion.Capacity.Format,
                        pump,
                        pump.CapacityTimeSeries);
        }
    }
}