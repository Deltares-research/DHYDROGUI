using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureCulvert : DefinitionGeneratorStructure
    {
        public DefinitionGeneratorStructureCulvert(CompoundStructureInfo compoundStructureInfo)
            : base(compoundStructureInfo)
        {
        }

        public override DelftIniCategory CreateStructureRegion(IStructure structure)
        {
            AddCommonRegionElements(structure, StructureRegion.StructureTypeName.Culvert);

            var culvert = structure as Culvert;
            if (culvert == null) return IniCategory;
            
            AddCommonCulvertElements(culvert);
           
            return IniCategory;
        }

        protected void AddCommonCulvertElements(ICulvert culvert)
        {
            IniCategory.AddProperty(StructureRegion.AllowedFlowDir.Key, (int)culvert.FlowDirection, StructureRegion.AllowedFlowDir.Description);
            IniCategory.AddProperty(StructureRegion.LeftLevel.Key, culvert.InletLevel, StructureRegion.LeftLevel.Description, StructureRegion.LeftLevel.Format);
            IniCategory.AddProperty(StructureRegion.RightLevel.Key, culvert.OutletLevel, StructureRegion.RightLevel.Description, StructureRegion.RightLevel.Format);
            IniCategory.AddProperty(StructureRegion.CsDefId.Key, culvert.CrossSectionDefinition.Name, StructureRegion.CsDefId.Description);
            IniCategory.AddProperty(StructureRegion.Length.Key, culvert.Length, StructureRegion.Length.Description, StructureRegion.Length.Format);
            IniCategory.AddProperty(StructureRegion.InletLossCoeff.Key, culvert.InletLossCoefficient, StructureRegion.InletLossCoeff.Description, StructureRegion.InletLossCoeff.Format);
            IniCategory.AddProperty(StructureRegion.OutletLossCoeff.Key, culvert.OutletLossCoefficient, StructureRegion.OutletLossCoeff.Description, StructureRegion.OutletLossCoeff.Format);
            IniCategory.AddProperty(StructureRegion.ValveOnOff.Key, Convert.ToInt32(culvert.IsGated), StructureRegion.ValveOnOff.Description);
            IniCategory.AddProperty(StructureRegion.IniValveOpen.Key, culvert.GateInitialOpening, StructureRegion.IniValveOpen.Description, StructureRegion.IniValveOpen.Format);

            var arguments = culvert.GateOpeningLossCoefficientFunction.Arguments;
            var components = culvert.GateOpeningLossCoefficientFunction.Components;

            if (arguments != null && components != null && arguments[0].Values.Count > 0 && components[0].Values.Count > 0)
            {
                var relOpening = arguments[0].Values.Cast<double>();
                var lossCoeff = components[0].Values.Cast<double>().ToList();

                IniCategory.AddProperty(StructureRegion.LossCoeffCount.Key, lossCoeff.Count, StructureRegion.LossCoeffCount.Description);
                IniCategory.AddProperty(StructureRegion.RelativeOpening.Key, relOpening, StructureRegion.RelativeOpening.Description, StructureRegion.RelativeOpening.Format);
                IniCategory.AddProperty(StructureRegion.LossCoefficient.Key, lossCoeff, StructureRegion.LossCoefficient.Description, StructureRegion.LossCoefficient.Format);
            }
            
        }

    }
}
