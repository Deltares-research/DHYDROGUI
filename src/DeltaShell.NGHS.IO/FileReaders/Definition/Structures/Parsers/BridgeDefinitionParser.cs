using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers
{
    /// <summary>
    /// Parser for bridges.
    /// </summary>
    public class BridgeDefinitionParser : CrossSectionDependentStructureParserBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BridgeDefinitionParser));

        /// <summary>
        /// Initializes a new instance of <see cref="BridgeDefinitionParser"/>.
        /// </summary>
        /// <param name="structureType">The structure type.</param>
        /// <param name="category">The <see cref="IDelftIniCategory"/> to parse a structure from.</param>
        /// <param name="crossSectionDefinitions">A collection of cross-section definitions.</param>
        /// <param name="branch">The branch to import the bridge on.</param>
        /// <param name="structuresFilename">The structures filename.</param>
        /// <exception cref="ArgumentNullException">When any argument is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when an invalid <paramref name="structureType"/> is provided.
        /// </exception>
        public BridgeDefinitionParser(StructureType structureType,
                                      IDelftIniCategory category,
                                      ICollection<ICrossSectionDefinition> crossSectionDefinitions,
                                      IBranch branch,
                                      string structuresFilename) 
            : base(structureType, category, crossSectionDefinitions, branch, structuresFilename) { }

        protected override IStructure1D Parse()
        {
            string crossSectionDefinitionId = Category.ReadProperty<string>(StructureRegion.CsDefId.Key, true); // pillar does not need cs def
            var definition = crossSectionDefinitionId == default(string) 
                                 ? null 
                                 : CrossSectionDefinitions.FirstOrDefault(cd => string.Equals(cd.Name, crossSectionDefinitionId, StringComparison.CurrentCultureIgnoreCase));

            var standardCrossSectionDefinition = definition as CrossSectionDefinitionStandard;

            var name = Category.ReadProperty<string>(StructureRegion.Id.Key);
            double shift = 0d;
            if (Category.ContainsProperty(StructureRegion.Shift.Key))
            {
                shift = Category.ReadProperty<double>(StructureRegion.Shift.Key);
            }
            else
            {
                if (Category.ContainsProperty("bedLevel"))
                {
                    Log.Warn($"Bridge {name}: \"bedLevel\" is not supported any more. Please provide the proper shift value (which has been set to 0)");
                }
            }

            var tabulatedCrossSectionDefinition = standardCrossSectionDefinition?.Shape?.GetTabulatedDefinition();
            var width = tabulatedCrossSectionDefinition?.Width ?? 50;
            var height = tabulatedCrossSectionDefinition?.ZWDataTable.Max(t => t.Z) - tabulatedCrossSectionDefinition?.ZWDataTable.Min(t => t.Z) ?? 3;
            return new Bridge
            {
                Name = name,
                LongName = Category.ReadProperty<string>(StructureRegion.Name.Key, true),
                Branch = Branch,
                Chainage = Branch.GetBranchSnappedChainage(Category.ReadProperty<double>(StructureRegion.Chainage.Key)),
                BridgeType = DetermineBridgeType(definition),
                FlowDirection = (FlowDirection) EnumUtils.GetEnumValueFromDisplayName(Category.ReadProperty<string>(StructureRegion.AllowedFlowDir.Key),typeof(FlowDirection)),
                Shift = shift,
                Width = width,
                Height = height,
                TabulatedCrossSectionDefinition = DetermineTabulatedCrossSectionDefinition(definition,
                                                                                           standardCrossSectionDefinition,
                                                                                           tabulatedCrossSectionDefinition,
                                                                                           crossSectionDefinitionId,
                                                                                           shift, width, height),
                YZCrossSectionDefinition = DetermineYZCrossSectionDefinition(definition,
                                                                             standardCrossSectionDefinition,
                                                                             crossSectionDefinitionId,
                                                                             shift, width, height),
                Length = Category.ReadProperty<double>(StructureRegion.Length.Key, true),
                InletLossCoefficient = Category.ReadProperty<double>(StructureRegion.InletLossCoeff.Key, true),
                OutletLossCoefficient = Category.ReadProperty<double>(StructureRegion.OutletLossCoeff.Key, true),
                PillarWidth = Category.ReadProperty<double>(StructureRegion.PillarWidth.Key, true),
                ShapeFactor = Category.ReadProperty<double>(StructureRegion.FormFactor.Key, true),
                FrictionDataType = (Friction)Enum.Parse(typeof(Friction), Category.ReadProperty<string>(StructureRegion.FrictionType.Key), true),
                Friction = Category.ReadProperty<double>(StructureRegion.Friction.Key)
            };
        }

        private CrossSectionDefinitionYZ DetermineYZCrossSectionDefinition(
            ICrossSectionDefinition definition, 
            CrossSectionDefinitionStandard standardCrossSectionDefinition, 
            string crossSectionDefinitionId, 
            double shift, double width, double height)
        {
            if (standardCrossSectionDefinition != null || definition == null)
            {
                return CrossSectionDefinitionYZ.CreateDefault(crossSectionDefinitionId).SetAsRectangle(shift, width, height);
            }

            if (definition.CrossSectionType == CrossSectionType.YZ)
            {
                return definition as CrossSectionDefinitionYZ;
            }

            if (definition.CrossSectionType == CrossSectionType.ZW)
            {
                return CrossSectionDefinitionYZ.CreateDefault(crossSectionDefinitionId)
                                               .ConvertZWDataTableToYZ(((CrossSectionDefinitionZW)definition).ZWDataTable);
            }

            return CrossSectionDefinitionYZ.CreateDefault(crossSectionDefinitionId).SetAsRectangle(shift, width, height);
        }

        private CrossSectionDefinitionZW DetermineTabulatedCrossSectionDefinition(
            ICrossSectionDefinition definition,
            CrossSectionDefinitionStandard standardCrossSectionDefinition, 
            CrossSectionDefinitionZW tabulatedCrossSectionDefinition, 
            string crossSectionDefinitionId, 
            double shift, double width, double height)
        {
            if (standardCrossSectionDefinition == null && definition != null && definition.CrossSectionType == CrossSectionType.ZW)
            {
                return definition as CrossSectionDefinitionZW;
            }

            if (tabulatedCrossSectionDefinition == null)
            {
                return CrossSectionDefinitionZW.CreateDefault(crossSectionDefinitionId).SetAsRectangle(shift, width, height);
            }

            return tabulatedCrossSectionDefinition;
        }

        private BridgeType DetermineBridgeType(ICrossSectionDefinition definition)
        {
            if (definition == null)
            {
                return BridgeType.Rectangle;
            }
            
            if (definition.CrossSectionType == CrossSectionType.ZW)
            {
                return BridgeType.Tabulated;
            }

            if (definition.CrossSectionType == CrossSectionType.YZ)
            {
                return BridgeType.YzProfile;
            }

            return BridgeType.Rectangle;
        }
    }
}