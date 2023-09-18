using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructurePump2D : DefinitionGeneratorTimeSeriesStructure2D
    {
        public override IniSection CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.Pump);

            var pump = (IPump) hydroObject;
            AddCapacityProperty(pump);

            return IniSection;
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