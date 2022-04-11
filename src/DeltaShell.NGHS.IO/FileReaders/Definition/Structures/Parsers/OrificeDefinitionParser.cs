using System;
using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers
{
    /// <summary>
    /// Parser for orifices.
    /// </summary>
    public class OrificeDefinitionParser : StructureParserBase
    {
        /// <summary>
        /// Initializes a new <see cref="OrificeDefinitionParser"/>.
        /// </summary>
        /// <param name="structureType">The structure type.</param>
        /// <param name="category">The <see cref="IDelftIniCategory"/> to parse a structure from.</param>
        /// <param name="branch">The branch to import the bridge on.</param>
        /// <param name="structuresFilename">The structures filename.</param>
        /// <exception cref="ArgumentNullException">When any argument is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when an invalid <paramref name="structureType"/> is provided.
        /// </exception>
        public OrificeDefinitionParser(StructureType structureType,
                                       IDelftIniCategory category, 
                                       IBranch branch, 
                                       string structuresFilename) 
            : base(structureType, category, branch, structuresFilename) {}

        protected override IStructure1D Parse()
        {
            var allowedFlowDirValue = Category.ReadProperty<string>(StructureRegion.AllowedFlowDir.Key, true);
            var allowedFlowDir = allowedFlowDirValue != null
                                     ? (FlowDirection)Enum.Parse(typeof(FlowDirection), allowedFlowDirValue, true)
                                     : 0;

            var orifice = new Orifice
            {
                Name = Category.ReadProperty<string>(StructureRegion.Id.Key),
                LongName = Category.ReadProperty<string>(StructureRegion.Name.Key, true),
                CrestLevel = Category.ReadProperty<double>(StructureRegion.CrestLevel.Key),
                CrestWidth = Category.ReadProperty<double>(StructureRegion.CrestWidth.Key, true),
                FlowDirection = allowedFlowDir,
                Branch = Branch,
                Chainage = Branch.GetBranchSnappedChainage(Category.ReadProperty<double>(StructureRegion.Chainage.Key)),
                UseVelocityHeight = Category.ReadProperty<bool>(StructureRegion.UseVelocityHeight.Key, true, true)
            };

            orifice.WeirFormula = WeirFormulaParser.ReadFormulaFromDefinition(Category, orifice);

            return orifice;
        }
    }
}