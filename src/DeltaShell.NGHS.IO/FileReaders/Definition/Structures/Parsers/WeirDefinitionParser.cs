using System;
using System.ComponentModel;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers
{
    /// <summary>
    /// Parser for weirs.
    /// </summary>
    public class WeirDefinitionParser : StructureParserBase 
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WeirDefinitionParser));

        private readonly ITimFileReader timFileReader;
        private readonly string structuresFilePath;
        private readonly DateTime referenceDateTime;

        protected virtual Weir CreateStructure() => new Weir(true);

        /// <summary>
        /// Initializes a new <see cref="WeirDefinitionParser"/>.
        /// </summary>
        /// <param name="timFileReader">The tim file reader</param>
        /// <param name="structureType">The structure type.</param>
        /// <param name="category">The <see cref="IDelftIniCategory"/> to parse a structure from.</param>
        /// <param name="branch">The branch to import the bridge on.</param>
        /// <param name="structuresFilePath">The structures filename.</param>
        /// <param name="referenceDateTime">The reference time date.</param>
        /// <exception cref="ArgumentNullException">When any argument is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when an invalid <paramref name="structureType"/> is provided.
        /// </exception>
        public WeirDefinitionParser(ITimFileReader timFileReader,
                                    StructureType structureType,
                                    IDelftIniCategory category,
                                    IBranch branch,
                                    string structuresFilePath,
                                    DateTime referenceDateTime)
            : base(structureType, category, branch, Path.GetFileName(structuresFilePath))
        {
            Ensure.NotNull(timFileReader, nameof(timFileReader));

            this.timFileReader = timFileReader;
            this.structuresFilePath = structuresFilePath;
            this.referenceDateTime = referenceDateTime;
        }

        protected override IStructure1D Parse()
        {
            var allowedFlowDirValue = Category.ReadProperty<string>(StructureRegion.AllowedFlowDir.Key, true);
            var allowedFlowDir = allowedFlowDirValue != null
                                     ? (FlowDirection)Enum.Parse(typeof(FlowDirection), allowedFlowDirValue, true)
                                     : 0;

            Weir weir = CreateStructure();

            weir.Name = Category.ReadProperty<string>(StructureRegion.Id.Key);
            weir.LongName = Category.ReadProperty<string>(StructureRegion.Name.Key, true);
            weir.CrestWidth = Category.ReadProperty<double>(StructureRegion.CrestWidth.Key, true);
            weir.FlowDirection = allowedFlowDir;
            weir.Branch = Branch;
            weir.Chainage = Branch.GetBranchSnappedChainage(Category.ReadProperty<double>(StructureRegion.Chainage.Key));
            weir.UseVelocityHeight = Category.ReadProperty<bool>(StructureRegion.UseVelocityHeight.Key, true, true);
            SetCrestLevel(weir);

            weir.WeirFormula = WeirFormulaParser.ReadFormulaFromDefinition(Category, 
                                                                           weir, 
                                                                           structuresFilePath, 
                                                                           referenceDateTime,
                                                                           timFileReader);
            return weir;
        }

        private void SetCrestLevel(Weir weir)
        {
            var crestLevelValue = Category.ReadProperty<string>(StructureRegion.CrestLevel.Key);

            if (crestLevelValue.EndsWith(FileSuffices.TimFile))
                ReadCrestLevelTimeSeries(weir, crestLevelValue);
            else
                weir.CrestLevel = Category.ReadProperty<double>(StructureRegion.CrestLevel.Key);
        }

        private void ReadCrestLevelTimeSeries(Weir weir, string relativeCrestLevelPath)
        {
            string filePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, relativeCrestLevelPath);
            weir.UseCrestLevelTimeSeries = true;

            try
            {
                timFileReader.Read(filePath, weir.CrestLevelTimeSeries, referenceDateTime);
            }
            catch (FileReadingException e)
            {
                log.WarnFormat("Could not read the time series at {0} using default crest level instead: {1}", filePath, e.Message);
                weir.UseCrestLevelTimeSeries = false;
            }
        }
    }
}