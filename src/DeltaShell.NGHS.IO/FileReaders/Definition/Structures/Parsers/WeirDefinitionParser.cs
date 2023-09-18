using System;
using System.ComponentModel;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;
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

        private readonly ITimeSeriesFileReader fileReader;
        private readonly string structuresFilePath;
        private readonly DateTime referenceDateTime;

        protected virtual Weir CreateStructure() => new Weir(true);

        /// <summary>
        /// Initializes a new <see cref="WeirDefinitionParser"/>.
        /// </summary>
        /// <param name="fileReader">The file reader</param>
        /// <param name="structureType">The structure type.</param>
        /// <param name="iniSection">The <see cref="IniSection"/> to parse a structure from.</param>
        /// <param name="branch">The branch to import the bridge on.</param>
        /// <param name="structuresFilePath">The structures filename.</param>
        /// <param name="referenceDateTime">The reference time date.</param>
        /// <exception cref="ArgumentNullException">When any argument is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when an invalid <paramref name="structureType"/> is provided.
        /// </exception>
        public WeirDefinitionParser(ITimeSeriesFileReader fileReader,
                                    StructureType structureType,
                                    IniSection iniSection,
                                    IBranch branch,
                                    string structuresFilePath,
                                    DateTime referenceDateTime)
            : base(structureType, iniSection, branch, Path.GetFileName(structuresFilePath))
        {
            Ensure.NotNull(fileReader, nameof(fileReader));

            this.fileReader = fileReader;
            this.structuresFilePath = structuresFilePath;
            this.referenceDateTime = referenceDateTime;
        }

        protected override IStructure1D Parse()
        {
            var allowedFlowDirValue = IniSection.ReadProperty<string>(StructureRegion.AllowedFlowDir.Key, true);
            var allowedFlowDir = allowedFlowDirValue != null
                                     ? (FlowDirection)Enum.Parse(typeof(FlowDirection), allowedFlowDirValue, true)
                                     : 0;

            Weir weir = CreateStructure();

            weir.Name = IniSection.ReadProperty<string>(StructureRegion.Id.Key);
            weir.LongName = IniSection.ReadProperty<string>(StructureRegion.Name.Key, true);
            weir.CrestWidth = IniSection.ReadProperty<double>(StructureRegion.CrestWidth.Key, true);
            weir.FlowDirection = allowedFlowDir;
            weir.Branch = Branch;
            weir.Chainage = Branch.GetBranchSnappedChainage(IniSection.ReadProperty<double>(StructureRegion.Chainage.Key));
            weir.UseVelocityHeight = IniSection.ReadProperty<bool>(StructureRegion.UseVelocityHeight.Key, true, true);
            SetCrestLevel(weir);

            weir.WeirFormula = WeirFormulaParser.ReadFormulaFromDefinition(IniSection, 
                                                                           weir, 
                                                                           structuresFilePath, 
                                                                           referenceDateTime,
                                                                           fileReader);
            return weir;
        }

        private void SetCrestLevel(Weir weir)
        {
            var crestLevelValue = IniSection.ReadProperty<string>(StructureRegion.CrestLevel.Key);

            if (fileReader.IsTimeSeriesProperty(crestLevelValue))
                ReadCrestLevelTimeSeries(weir, crestLevelValue);
            else
                weir.CrestLevel = IniSection.ReadProperty<double>(StructureRegion.CrestLevel.Key);
        }

        private void ReadCrestLevelTimeSeries(Weir weir, string relativeCrestLevelPath)
        {
            string filePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, relativeCrestLevelPath);
            weir.UseCrestLevelTimeSeries = true;

            try
            {
                fileReader.Read(relativeCrestLevelPath, filePath, new StructureTimeSeries(weir, weir.CrestLevelTimeSeries), referenceDateTime);
            }
            catch (FileReadingException e)
            {
                log.WarnFormat("Could not read the time series at {0} using default crest level instead: {1}", filePath, e.Message);
                weir.UseCrestLevelTimeSeries = false;
            }
        }
    }
}