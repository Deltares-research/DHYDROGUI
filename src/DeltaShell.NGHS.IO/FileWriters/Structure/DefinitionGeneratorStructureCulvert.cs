using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters.Structure.StructureFileNameGenerator;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureCulvert : DefinitionGeneratorTimeSeriesStructure
    {
        public DefinitionGeneratorStructureCulvert(IStructureFileNameGenerator structureFileNameGenerator) : base(structureFileNameGenerator) {}
        
        public override IniSection CreateStructureRegion(IHydroObject hydroObject)
        {
            Ensure.NotNull(hydroObject, nameof(hydroObject));

            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.Culvert);

            var culvert = hydroObject as Culvert;
            
            if (culvert == null) return IniSection;
         
            AddCommonCulvertElements(culvert);
           
            return IniSection;
        }

        protected void AddCommonCulvertElements(ICulvert culvert)
        {
            AddAllowedFlowDir(culvert);
            AddLeftLevel(culvert);
            AddRightLevel(culvert);
            AddCsDefId(culvert);
            AddLength(culvert);
            AddInletLossCoeff(culvert);
            AddOutletLossCoeff(culvert);
            AddValveOnOff(culvert);
            AddGateInitialOpen(culvert);
            AddLossCoefficient(culvert);
        }

        private void AddAllowedFlowDir(ICulvert culvert) => 
            IniSection.AddPropertyWithOptionalComment(StructureRegion.AllowedFlowDir.Key, 
                                    culvert.FlowDirection.ToString().ToLower(), 
                                    StructureRegion.AllowedFlowDir.Description);


        private void AddLeftLevel(ICulvert culvert) => 
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.LeftLevel.Key, 
                                    culvert.InletLevel, 
                                    StructureRegion.LeftLevel.Description, 
                                    StructureRegion.LeftLevel.Format);

        private void AddRightLevel(ICulvert culvert) => 
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.RightLevel.Key, 
                                    culvert.OutletLevel, 
                                    StructureRegion.RightLevel.Description, 
                                    StructureRegion.RightLevel.Format);

        private void AddCsDefId(ICulvert culvert) => 
            IniSection.AddPropertyWithOptionalComment(StructureRegion.CsDefId.Key, 
                                    culvert.CrossSectionDefinition.Name, 
                                    StructureRegion.CsDefId.Description);

        private void AddLength(ICulvert culvert) => 
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.Length.Key, 
                                    culvert.Length, 
                                    StructureRegion.Length.Description, 
                                    StructureRegion.Length.Format);

        private void AddInletLossCoeff(ICulvert culvert) => 
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.InletLossCoeff.Key, 
                                    culvert.InletLossCoefficient, 
                                    StructureRegion.InletLossCoeff.Description, 
                                    StructureRegion.InletLossCoeff.Format);

        private void AddOutletLossCoeff(ICulvert culvert) => 
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.OutletLossCoeff.Key, 
                                    culvert.OutletLossCoefficient, 
                                    StructureRegion.OutletLossCoeff.Description, 
                                    StructureRegion.OutletLossCoeff.Format);

        private void AddValveOnOff(ICulvert culvert) => 
            IniSection.AddProperty(StructureRegion.ValveOnOff.Key, 
                                    Convert.ToInt32(culvert.IsGated), 
                                    StructureRegion.ValveOnOff.Description);

        private void AddGateInitialOpen(ICulvert culvert)
        {
            AddProperty(culvert.UseGateInitialOpeningTimeSeries,
                        StructureRegion.IniValveOpen.Key, 
                        culvert.GateInitialOpening, 
                        StructureRegion.IniValveOpen.Description, 
                        StructureRegion.IniValveOpen.Format);
        }

        private void AddLossCoefficient(ICulvert culvert)
        {
            var arguments = culvert.GateOpeningLossCoefficientFunction.Arguments;
            var components = culvert.GateOpeningLossCoefficientFunction.Components;

            if (arguments != null && components != null && arguments[0].Values.Count > 0 && components[0].Values.Count > 0)
            {
                var relOpening = arguments[0].Values.Cast<double>();
                var lossCoeff = components[0].Values.Cast<double>().ToList();

                IniSection.AddProperty(StructureRegion.LossCoeffCount.Key, lossCoeff.Count, StructureRegion.LossCoeffCount.Description);
                IniSection.AddPropertyWithMultipleValuesWithOptionalCommentAndFormat(StructureRegion.RelativeOpening.Key, relOpening, StructureRegion.RelativeOpening.Description, StructureRegion.RelativeOpening.Format);
                IniSection.AddPropertyWithMultipleValuesWithOptionalCommentAndFormat(StructureRegion.LossCoefficient.Key, lossCoeff, StructureRegion.LossCoefficient.Description, StructureRegion.LossCoefficient.Format);
            }
        }
    }
}
