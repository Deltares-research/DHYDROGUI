using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public class BcFile : NGHSFileBase
    {
        public enum WriteMode
        {
            [Description("Single file")]
            SingleFile,

            [Description("File per boundary")]
            FilePerFeature,

            [Description("File per process")]
            FilePerProcess,

            [Description("File per quantity")]
            FilePerQuantity
        }

        public const string Extension = ".bc";

        public const string BlockKey = "[forcing]";
        public const string QuantityKey = "Quantity";
        private const string generalHeader = "[general]";
        private const string SupportPointKey = "Name";
        private const string ForcingTypeKey = "Function";
        private const string SeriesIndexKey = "FunctionIndex";
        private const string UnitKey = "Unit";
        private const string TimeInterpolationKey = "Time-interpolation";
        private const string VerticalIntepolationKey = "Vertical interpolation";
        private const string VerticalPositionTypeKey = "Vertical position type";
        private const string VerticalPositionSpecKey = "Vertical position specification";
        private const string VerticalPositionKey = "Vertical position";
        private const string OffsetKey = "Offset";
        private const string FactorKey = "Factor";
        private const StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase;
        protected readonly ILog log = LogManager.GetLogger(typeof(BcFile));

        private readonly List<FlowBoundaryQuantityType> supportedProcesses = new List<FlowBoundaryQuantityType>()
        {
            FlowBoundaryQuantityType.WaterLevel,
            FlowBoundaryQuantityType.Velocity,
            FlowBoundaryQuantityType.Discharge,
            FlowBoundaryQuantityType.Riemann,
            FlowBoundaryQuantityType.RiemannVelocity,
            FlowBoundaryQuantityType.Neumann,
            FlowBoundaryQuantityType.Outflow,
            FlowBoundaryQuantityType.NormalVelocity,
            FlowBoundaryQuantityType.TangentVelocity,
            FlowBoundaryQuantityType.VelocityVector,
            FlowBoundaryQuantityType.Salinity,
            FlowBoundaryQuantityType.Temperature,
            FlowBoundaryQuantityType.Tracer,
            FlowBoundaryQuantityType.SedimentConcentration
        };

        private readonly int columnWidth = VerticalPositionSpecKey.Length;

        public BcFile()
        {
            MultiFileMode = WriteMode.SingleFile;
        }

        public WriteMode MultiFileMode { get; set; }

        public bool CorrectionFile { private get; set; }

        public IEnumerable<IGrouping<string, Tuple<IBoundaryCondition, BoundaryConditionSet>>> GroupBoundaryConditions(
            IEnumerable<BoundaryConditionSet> boundaryConditionSets)
        {
            Func<IBoundaryCondition, string> discriminator = BcDiscriminator(MultiFileMode);
            return boundaryConditionSets
                   .SelectMany(bcs => bcs.BoundaryConditions
                                         .Where(bc => SupportedProcesses.Contains(bc.ProcessName) &&
                                                      bc.DataType !=
                                                      BoundaryConditionDataType.Empty) // don't write empty bc!
                                         .Select(bc => new Tuple<IBoundaryCondition, BoundaryConditionSet>(bc, bcs)))
                   .GroupBy(t => discriminator(t.Item1));
        }

        public void Write(IEnumerable<BoundaryConditionSet> boundaryConditionSets, string filePath,
                          BcFileFlowBoundaryDataBuilder boundaryDataBuilder, DateTime? refDate = null)
        {
            IEnumerable<IGrouping<string, Tuple<IBoundaryCondition, BoundaryConditionSet>>> grouping =
                GroupBoundaryConditions(boundaryConditionSets.ToList());

            foreach (IGrouping<string, Tuple<IBoundaryCondition, BoundaryConditionSet>> group in grouping)
            {
                string subFile = string.IsNullOrEmpty(group.Key) ? filePath : AppendToFile(filePath, "_" + group.Key);
                Write(group.ToDictionary(t => t.Item1, t => t.Item2), subFile, boundaryDataBuilder, refDate);
            }
        }

        public static bool IsCorrectionType(BoundaryConditionDataType dataType)
        {
            return dataType == BoundaryConditionDataType.AstroCorrection ||
                   dataType == BoundaryConditionDataType.HarmonicCorrection;
        }

        public virtual void Write(
            IEnumerable<KeyValuePair<IBoundaryCondition, BoundaryConditionSet>> boundaryConditions,
            string filePath, BcFileFlowBoundaryDataBuilder boundaryDataBuilder, DateTime? refDate = null)
        {
            OpenOutputFile(filePath);
            try
            {
                foreach (KeyValuePair<IBoundaryCondition, BoundaryConditionSet> boundaryConditionKeyValuePair in
                    boundaryConditions)
                {
                    if (CorrectionFile && !IsCorrectionType(boundaryConditionKeyValuePair.Key.DataType))
                    {
                        continue;
                    }

                    IBoundaryCondition boundaryCondition = boundaryConditionKeyValuePair.Key;
                    IList<string> supportPointNames = boundaryConditionKeyValuePair.Value.SupportPointNames;

                    int seriesIndex =
                        boundaryConditionKeyValuePair.Value.BoundaryConditions.Where(
                                                         bc => bc.VariableName == boundaryCondition.VariableName &&
                                                               SimilarDataType(
                                                                   bc.DataType, boundaryCondition.DataType))
                                                     .ToList()
                                                     .IndexOf(boundaryCondition);

                    IEnumerable<BcBlockData> blockData = boundaryDataBuilder.CreateBlockData(
                        boundaryCondition as FlowBoundaryCondition,
                        supportPointNames, refDate, seriesIndex, CorrectionFile);

                    WriteBlocks(blockData);
                }
            }
            finally
            {
                CloseOutputFile();
            }
        }

        private void WriteBlocks(IEnumerable<BcBlockData> blockData)
        {
            foreach (BcBlockData block in blockData)
            {
                WriteBlock(block);
                WriteLine("");
            }
        }

        /// <summary>
        /// Write the lateral time series data to the specified file.
        /// </summary>
        /// <param name="laterals"> The laterals. </param>
        /// <param name="filePath"> The target bc file path. </param>
        /// <param name="boundaryDataBuilder"> The bc file flow boundary data builder. </param>
        /// <param name="refDate"> The reference date. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="laterals"/> or <paramref name="boundaryDataBuilder"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="filePath"/> is <c>null</c> or empty or white space.
        /// </exception>
        public void WriteLateralData(
            IEnumerable<Lateral> laterals,
            string filePath, BcFileFlowBoundaryDataBuilder boundaryDataBuilder, DateTime? refDate = null)
        {
            Ensure.NotNull(laterals, nameof(laterals));
            Ensure.NotNullOrWhiteSpace(filePath, nameof(filePath));
            Ensure.NotNull(boundaryDataBuilder, nameof(boundaryDataBuilder));
            
            var blocks = new List<BcBlockData>();
            foreach (Lateral lateral in laterals)
            {
                BcBlockData block = boundaryDataBuilder.CreateBcBlockForLateral(lateral, refDate);
                blocks.Add(block);
            }
            
            OpenOutputFile(filePath);
            try
            {
                WriteBlocks(blocks);
            }
            finally
            {
                CloseOutputFile();
            }
        }

        public virtual IEnumerable<BcBlockData> Read(string inputFile)
        {
            OpenInputFile(inputFile);
            try
            {
                string line = GetNextLine();
                while (line != null)
                {
                    line = line.Trim();

                    if (line.Equals(generalHeader, stringComparison))
                    {
                        line = GetNextLine();
                        continue;
                    }
                    
                    if (line.Equals(BlockKey, stringComparison))
                    {
                        BcBlockData block = ReadDataBlock(out line);
                        if (block != null)
                        {
                            yield return block;
                        }
                    }
                    else
                    {
                        if (IsNewSection(line))
                        {
                            log.WarnFormat("Section {0} not supported on line {1}. File: {2}", line, LineNumber, inputFile);
                        }

                        line = GetNextLine();
                    }
                }
            }
            finally
            {
                CloseInputFile();
            }
        }

        private static bool IsNewSection(string line)
        {
            return line.StartsWith("[") && line.EndsWith("]");
        }

        protected virtual List<string> SupportedProcesses
        {
            get
            {
                return supportedProcesses.Select(sp => FlowBoundaryCondition.GetProcessNameForQuantity(sp)).Distinct()
                                         .ToList();
            }
        }

        protected virtual void WriteBlock(BcBlockData block)
        {
            WriteLine(BlockKey);

            WriteKeyValuePairLine(SupportPointKey, block.SupportPoint);

            WriteKeyValuePairLine(ForcingTypeKey, block.FunctionType);

            if (block.SeriesIndex != null)
            {
                WriteKeyValuePairLine(SeriesIndexKey, block.SeriesIndex);
            }

            if (block.TimeInterpolationType != null)
            {
                WriteKeyValuePairLine(TimeInterpolationKey, block.TimeInterpolationType);
            }

            if (block.VerticalPositionType != null)
            {
                WriteKeyValuePairLine(VerticalPositionTypeKey, block.VerticalPositionType);
            }

            if (block.VerticalPositionDefinition != null)
            {
                WriteKeyValuePairLine(VerticalPositionSpecKey, block.VerticalPositionDefinition);
            }

            if (block.VerticalInterpolationType != null)
            {
                WriteKeyValuePairLine(VerticalIntepolationKey, block.VerticalInterpolationType);
            }

            if (block.Offset != null)
            {
                WriteKeyValuePairLine(OffsetKey, block.Offset);
            }

            if (block.Factor != null)
            {
                WriteKeyValuePairLine(FactorKey, block.Factor);
            }

            foreach (BcQuantityData quantity in block.Quantities)
            {
                WriteKeyValuePairLine(QuantityKey, quantity.QuantityName);
                if (quantity.Unit != null)
                {
                    WriteKeyValuePairLine(UnitKey, quantity.Unit);
                }

                if (quantity.VerticalPosition != null)
                {
                    WriteKeyValuePairLine(VerticalPositionKey, quantity.VerticalPosition);
                }
            }

            int rowCount = block.Quantities.Select(q => q.Values.Count).Min();

            if (rowCount == 0)
            {
                return;
            }

            List<int> columnWidths =
                block.Quantities.Select(q => q.Values.Take(rowCount).Select(s => s.Length).Max() + 1).ToList();

            for (var i = 0; i < rowCount; ++i)
            {
                var j = 0;
                WriteLine(string.Join(" ", block.Quantities.Select(q => q.Values[i].PadRight(columnWidths[j++])))
                                .TrimEnd());
            }
        }

        private static Func<IBoundaryCondition, string> BcDiscriminator(WriteMode writeMode)
        {
            switch (writeMode)
            {
                case WriteMode.SingleFile:
                    return bc => string.Empty;
                case WriteMode.FilePerFeature:
                    return bc => bc.Feature.ToString();
                case WriteMode.FilePerProcess:
                    return bc => bc.ProcessName;
                case WriteMode.FilePerQuantity:
                    return bc => bc.VariableName;
                default:
                    throw new ArgumentException("File split mode " + writeMode + " is not supported by BC file writer");
            }
        }

        private static string AppendToFile(string filePath, string tag)
        {
            string extension = Path.GetExtension(filePath);
            if (extension == null)
            {
                return filePath + tag;
            }

            return filePath.Substring(0, filePath.Length - extension.Length) + tag + extension;
        }

        private static bool SimilarDataType(BoundaryConditionDataType dt1, BoundaryConditionDataType dt2)
        {
            if (dt1 == BoundaryConditionDataType.AstroCorrection && dt2 == BoundaryConditionDataType.AstroComponents)
            {
                return true;
            }

            if (dt1 == BoundaryConditionDataType.AstroComponents && dt2 == BoundaryConditionDataType.AstroCorrection)
            {
                return true;
            }

            if (dt1 == BoundaryConditionDataType.HarmonicCorrection && dt2 == BoundaryConditionDataType.Harmonics)
            {
                return true;
            }

            if (dt1 == BoundaryConditionDataType.Harmonics && dt2 == BoundaryConditionDataType.HarmonicCorrection)
            {
                return true;
            }

            return dt1 == dt2;
        }

        private void WriteKeyValuePairLine(string keyString, string valueString)
        {
            WriteLine(keyString.PadRight(columnWidth) + " = " + valueString);
        }

        private static string[] SplitString(string str)
        {
            return str.Split(new[]
            {
                '='
            }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
        }

        private BcBlockData ReadDataBlock(out string line)
        {
            string supportPointName = null;
            string forcingType = null;
            string timeInterpolationType = null;
            string verticalPositionType = null;
            string verticalPositionSpecification = null;
            string verticalInterpolationType = null;
            string seriesIndex = null;
            string offset = null;
            string factor = null;
            var quantityDataList = new List<BcQuantityData>();

            BcQuantityData quantityData = null;

            int lineNumber = LineNumber;

            line = GetNextLine();

            while (line != null)
            {
                string[] split = SplitString(line);
                if (split.Length < 2)
                {
                    break;
                }

                string key = split[0];
                if (key.Equals(SupportPointKey, stringComparison))
                {
                    supportPointName = split[1];
                }

                if (key.Equals(ForcingTypeKey, stringComparison))
                {
                    forcingType = split[1];
                }

                if (key.Equals(TimeInterpolationKey, stringComparison))
                {
                    timeInterpolationType = split[1];
                }

                if (key.Equals(VerticalPositionTypeKey, stringComparison))
                {
                    verticalPositionType = split[1];
                }

                if (key.Equals(VerticalPositionSpecKey, stringComparison))
                {
                    verticalPositionSpecification = split[1];
                }

                if (key.Equals(VerticalIntepolationKey, stringComparison))
                {
                    verticalInterpolationType = split[1];
                }

                if (key.Equals(SeriesIndexKey, stringComparison))
                {
                    seriesIndex = split[1];
                }

                if (key.Equals(OffsetKey, stringComparison))
                {
                    offset = split[1];
                }

                if (key.Equals(FactorKey, stringComparison))
                {
                    factor = split[1];
                }

                if (key.Equals(QuantityKey, stringComparison))
                {
                    if (quantityData != null)
                    {
                        quantityDataList.Add(quantityData);
                    }

                    if (split.Length == 2)
                    {
                        quantityData = new BcQuantityData { QuantityName = split[1] };
                    }
                }

                if (key.Equals(UnitKey, stringComparison))
                {
                    if (quantityData == null)
                    {
                        continue;
                    }

                    quantityData.Unit = split[1];
                }

                if (key.Equals(VerticalPositionKey, stringComparison))
                {
                    if (quantityData == null)
                    {
                        continue;
                    }

                    quantityData.VerticalPosition = split[1];
                }

                line = GetNextLine();
            }

            if (quantityData != null)
            {
                quantityDataList.Add(quantityData);
            }

            while (line != null)
            {
                if (line.StartsWith(BlockKey, stringComparison))
                {
                    break;
                }

                string[] columns = line.Split(new[]
                {
                    ' ',
                    '\t'
                }, StringSplitOptions.RemoveEmptyEntries);
                if (columns.Length < quantityDataList.Count)
                {
                    log.WarnFormat("Omitting line {0} with less than {1} columns", LineNumber, quantityDataList.Count);
                }

                for (var i = 0; i < quantityDataList.Count; ++i)
                {
                    quantityDataList[i].Values.Add(columns[i]);
                }

                line = GetNextLine();
            }

            if (supportPointName == null || forcingType == null || !quantityDataList.Any())
            {
                return null;
            }

            return
                new BcBlockData
                {
                    FilePath = InputFilePath,
                    LineNumber = lineNumber,
                    SupportPoint = supportPointName,
                    FunctionType = forcingType,
                    SeriesIndex = seriesIndex,
                    TimeInterpolationType = timeInterpolationType,
                    VerticalPositionDefinition = verticalPositionSpecification,
                    VerticalPositionType = verticalPositionType,
                    VerticalInterpolationType = verticalInterpolationType,
                    Offset = offset,
                    Factor = factor,
                    Quantities = quantityDataList
                };
        }
    }
}