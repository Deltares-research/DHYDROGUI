using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.Structures
{
    class CulvertDefinitionReader : IStructureDefinitionReader
    {
        public IStructure1D ReadDefinition(IDelftIniCategory category,
            IList<ICrossSectionDefinition> crossSectionDefinitions, IBranch branch)
        {
            var crossSectionDefinitionId = category.ReadProperty<string>(StructureRegion.CsDefId.Key);
            var definition = crossSectionDefinitions.FirstOrDefault(cd => cd.Name.ToLower() == crossSectionDefinitionId);

            var culvert = new Culvert
            {
                Name = category.ReadProperty<string>(StructureRegion.Id.Key),
                Branch = branch,
                Chainage = category.ReadProperty<double>(StructureRegion.Chainage.Key),
                FlowDirection = (FlowDirection) category.ReadProperty<int>(StructureRegion.AllowedFlowDir.Key),
                InletLevel = category.ReadProperty<double>(StructureRegion.LeftLevel.Key),
                OutletLevel = category.ReadProperty<double>(StructureRegion.RightLevel.Key),
                Length = category.ReadProperty<double>(StructureRegion.Length.Key),
                InletLossCoefficient = category.ReadProperty<double>(StructureRegion.InletLossCoeff.Key),
                OutletLossCoefficient = category.ReadProperty<double>(StructureRegion.OutletLossCoeff.Key),
                IsGated = category.ReadProperty<string>(StructureRegion.ValveOnOff.Key) != "0",
                GateInitialOpening = category.ReadProperty<double>(StructureRegion.IniValveOpen.Key),
                BendLossCoefficient = category.ReadProperty<double>(StructureRegion.BendLossCoef.Key, true),
                SiphonOnLevel = category.ReadProperty<double>(StructureRegion.TurnOnLevel.Key, true),
                SiphonOffLevel = category.ReadProperty<double>(StructureRegion.TurnOffLevel.Key, true),
                //CrossSectionDefinition = definition, 
            };

            var relOpening = category.ReadProperty<string>(StructureRegion.RelativeOpening.Key).ToDoubleArray();
            var lossCoeff = category.ReadProperty<string>(StructureRegion.LossCoefficient.Key).ToDoubleArray();

            culvert.GateOpeningLossCoefficientFunction =
                culvert.GateOpeningLossCoefficientFunction.CreateFunctionFromArrays(relOpening, lossCoeff);
            return culvert;
        }
    }
}