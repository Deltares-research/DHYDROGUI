using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureExtraResistance : DefinitionGeneratorStructure
    {
        public DefinitionGeneratorStructureExtraResistance(CompoundStructureInfo compoundStructureInfo)
            : base(compoundStructureInfo)
        {
        }

        public override DelftIniCategory CreateStructureRegion(IStructure1D structure)
        {
            AddCommonRegionElements(structure, StructureRegion.StructureTypeName.ExtraResistanceStructure);

            var extraResistance = structure as IExtraResistance;
            if(extraResistance == null) return IniCategory;

            if (extraResistance.FrictionTable == null) return IniCategory;

            var arguments = extraResistance.FrictionTable.Arguments;
            var components = extraResistance.FrictionTable.Components;
            if (arguments != null && components != null && arguments[0].Values.Count > 0 && components[0].Values.Count > 0)
            {

                var levels = arguments[0].Values.Cast<double>().ToList();
                var ksi = components[0].Values.Cast<double>();

                IniCategory.AddProperty(StructureRegion.NumValues.Key, levels.Count, StructureRegion.NumValues.Description);
                IniCategory.AddProperty(StructureRegion.Levels.Key, levels, StructureRegion.Levels.Description, StructureRegion.Levels.Format);
                IniCategory.AddProperty(StructureRegion.Ksi.Key, ksi, StructureRegion.Ksi.Description, StructureRegion.Ksi.Format);
            }

            return IniCategory;
        }

    }
}
