using System;
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
    class BridgeDefinitionReader : IStructureDefinitionReader
    {
        public IStructure1D ReadDefinition(IDelftIniCategory category, IList<ICrossSectionDefinition> crossSectionDefinitions, IBranch branch)
        {
            var crossSectionDefinitionId = category.ReadProperty<string>(StructureRegion.CsDefId.Key);
            var definition = crossSectionDefinitions.FirstOrDefault(cd => cd.Name.ToLower() == crossSectionDefinitionId);

            return new Bridge
            {
                Name = category.ReadProperty<string>(StructureRegion.Id.Key),
                Branch = branch,
                Chainage = category.ReadProperty<double>(StructureRegion.Chainage.Key),
                FlowDirection = (FlowDirection) category.ReadProperty<int>(StructureRegion.AllowedFlowDir.Key),
                BottomLevel = category.ReadProperty<double>(StructureRegion.BedLevel.Key),
                //CrossSectionDefinition = definition, 
                Length = category.ReadProperty<double>(StructureRegion.Length.Key),
                InletLossCoefficient = category.ReadProperty<double>(StructureRegion.InletLossCoeff.Key, true),
                OutletLossCoefficient = category.ReadProperty<double>(StructureRegion.OutletLossCoeff.Key, true),

                PillarWidth = category.ReadProperty<double>(StructureRegion.PillarWidth.Key, true),
                ShapeFactor = category.ReadProperty<double>(StructureRegion.FormFactor.Key, true),
            };
        }
    }
}