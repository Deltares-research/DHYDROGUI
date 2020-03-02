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
            var crossSectionDefinitionId = category.ReadProperty<string>(StructureRegion.CsDefId.Key, true); // pillar does not need cs def
            var definition = crossSectionDefinitionId == default(string) ? null : crossSectionDefinitions.FirstOrDefault(cd => string.Equals(cd.Name, crossSectionDefinitionId, StringComparison.CurrentCultureIgnoreCase));

            var standardCrossSectionDefinition = definition as CrossSectionDefinitionStandard;

            return new Bridge
            {
                Name = category.ReadProperty<string>(StructureRegion.Id.Key),
                LongName = category.ReadProperty<string>(StructureRegion.Name.Key, true),
                Branch = branch,
                Chainage = category.ReadProperty<double>(StructureRegion.Chainage.Key),
                BridgeType = definition?.CrossSectionType == CrossSectionType.ZW ? BridgeType.Tabulated : GetBridgeTypeFromShapeType(standardCrossSectionDefinition?.ShapeType),
                FlowDirection = (FlowDirection)category.ReadProperty<string>(StructureRegion.AllowedFlowDir.Key).GetEnumValueFromDisplayName(typeof(FlowDirection)),
                BottomLevel = category.ReadProperty<double>(StructureRegion.BedLevel.Key),
                TabulatedCrossSectionDefinition = standardCrossSectionDefinition == null && definition != null && definition.CrossSectionType == CrossSectionType.ZW ? definition as CrossSectionDefinitionZW : standardCrossSectionDefinition?.Shape?.GetTabulatedDefinition() ?? new CrossSectionDefinitionZW(), 
                Length = category.ReadProperty<double>(StructureRegion.Length.Key, true),
                InletLossCoefficient = category.ReadProperty<double>(StructureRegion.InletLossCoeff.Key, true),
                OutletLossCoefficient = category.ReadProperty<double>(StructureRegion.OutletLossCoeff.Key, true),
                PillarWidth = category.ReadProperty<double>(StructureRegion.PillarWidth.Key, true),
                ShapeFactor = category.ReadProperty<double>(StructureRegion.FormFactor.Key, true),
                FrictionType = (BridgeFrictionType) category.ReadProperty<string>(StructureRegion.FrictionType.Key).GetEnumValueFromDisplayName(typeof(BridgeFrictionType)),
                Friction = category.ReadProperty<double>(StructureRegion.Friction.Key)
            };
        }

        private static BridgeType GetBridgeTypeFromShapeType(CrossSectionStandardShapeType? shapeType)
        {
            switch (shapeType)
            {
                case CrossSectionStandardShapeType.Rectangle:
                    return BridgeType.Rectangle;
                case null:
                    return BridgeType.Pillar;
                default:
                    return BridgeType.Tabulated;
            }
        }
    }
}