using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.Utils;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.Structures
{
    class BridgeDefinitionReader : IStructureDefinitionReader
    {
        public IStructure1D ReadDefinition(IDelftIniCategory category, IList<ICrossSectionDefinition> crossSectionDefinitions, IBranch branch)
        {
            var crossSectionDefinitionId = category.ReadProperty<string>(StructureRegion.CsDefId.Key, true); // pillar does not need cs def
            var definition = crossSectionDefinitionId == default(string) ? null : crossSectionDefinitions.FirstOrDefault(cd => string.Equals(cd.Name, crossSectionDefinitionId, StringComparison.CurrentCultureIgnoreCase));

            var standardCrossSectionDefinition = definition as CrossSectionDefinitionStandard;

            var name = category.ReadProperty<string>(StructureRegion.Id.Key);
            double bottomLevel;
            if (category.ContainsProperty(StructureRegion.Shift.Key))
            {
                bottomLevel = category.ReadProperty<double>(StructureRegion.Shift.Key);
            } else
            {
                // no 'shift', get the deprecated 'bedLevel', or set to zero
                bottomLevel = category.ReadProperty<double>(StructureRegion.BedLevel.Key, isOptional: true, defaultValue: 0.0d);
            }

            var tabulatedCrossSectionDefinition = standardCrossSectionDefinition?.Shape?.GetTabulatedDefinition();
            var width = tabulatedCrossSectionDefinition?.Width ?? 50;
            var height = tabulatedCrossSectionDefinition?.ZWDataTable.Max(t => t.Z) - tabulatedCrossSectionDefinition?.ZWDataTable.Min(t => t.Z) ?? 3;
            return new Bridge
            {
                Name = name,
                LongName = category.ReadProperty<string>(StructureRegion.Name.Key, true),
                Branch = branch,
                Chainage = branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(category.ReadProperty<double>(StructureRegion.Chainage.Key)),
                BridgeType = definition?.CrossSectionType == CrossSectionType.ZW ? BridgeType.Tabulated : definition?.CrossSectionType == CrossSectionType.YZ ? BridgeType.YzProfile : BridgeType.Rectangle,
                FlowDirection = (FlowDirection)category.ReadProperty<string>(StructureRegion.AllowedFlowDir.Key).GetEnumValueFromDisplayName(typeof(FlowDirection)),
                BottomLevel = bottomLevel,
                Width = width,
                Height = height,
                TabulatedCrossSectionDefinition = standardCrossSectionDefinition == null && definition != null && definition.CrossSectionType == CrossSectionType.ZW 
                    ? definition as CrossSectionDefinitionZW 
                    : tabulatedCrossSectionDefinition 
                      ?? CrossSectionDefinitionZW.CreateDefault(crossSectionDefinitionId).SetAsRectangle(bottomLevel,width,height), 
                YZCrossSectionDefinition = standardCrossSectionDefinition == null && definition != null 
                    ? definition.CrossSectionType == CrossSectionType.YZ  
                        ? definition as CrossSectionDefinitionYZ
                        : definition.CrossSectionType == CrossSectionType.ZW 
                            ? CrossSectionDefinitionYZ.CreateDefault(crossSectionDefinitionId).ConvertZWDataTableToYZ(((CrossSectionDefinitionZW)definition).ZWDataTable)
                            : CrossSectionDefinitionYZ.CreateDefault(crossSectionDefinitionId).SetAsRectangle(bottomLevel, width, height)
                    : CrossSectionDefinitionYZ.CreateDefault(crossSectionDefinitionId).SetAsRectangle(bottomLevel, width, height), 
                Length = category.ReadProperty<double>(StructureRegion.Length.Key, true),
                InletLossCoefficient = category.ReadProperty<double>(StructureRegion.InletLossCoeff.Key, true),
                OutletLossCoefficient = category.ReadProperty<double>(StructureRegion.OutletLossCoeff.Key, true),
                PillarWidth = category.ReadProperty<double>(StructureRegion.PillarWidth.Key, true),
                ShapeFactor = category.ReadProperty<double>(StructureRegion.FormFactor.Key, true),
                FrictionDataType = (Friction) Enum.Parse(typeof(Friction), category.ReadProperty<string>(StructureRegion.FrictionType.Key), true),
                Friction = category.ReadProperty<double>(StructureRegion.Friction.Key)
            };
        }
    }
}