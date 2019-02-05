using System;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class ExtraResistanceConverter : StructureConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new ExtraResistance();
        }

        protected override void SetStructureProperties()
        {
            if (!(Structure is ExtraResistance extraResistance)) return;

            var numValues = Category.ReadProperty<int>(StructureRegion.NumValues.Key);
            var argumentsLevels = Category.ReadPropertiesToListOfType<double>(StructureRegion.Levels.Key);
            var componentsKsi = Category.ReadPropertiesToListOfType<double>(StructureRegion.Ksi.Key);

            if (numValues != argumentsLevels.Count || numValues != componentsKsi.Count)
            {
                throw new Exception($"For extra resistance {extraResistance.Name} the friction table contains an error");
            }

            extraResistance.FrictionTable.Clear();

            for (var i = 0; i < numValues; i++)
            {
                extraResistance.FrictionTable[argumentsLevels[i]] = componentsKsi[i];
            }
        }
    }
}
