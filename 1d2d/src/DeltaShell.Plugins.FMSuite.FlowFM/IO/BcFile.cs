using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class BcFile: FMSuiteFileBase
    {
        protected readonly ILog log = LogManager.GetLogger(typeof (BcFile));

        public const string Extension = ".bc";

        public string BlockKey = "[forcing]";
        private const string SupportPointKey = "Name";
        private const string FunctionTypeKey = "Function";
        private const string SeriesIndexKey = "FunctionIndex";
        public const string QuantityKey = "Quantity";
        private const string UnitKey = "Unit";
        
        private const string TimeInterpolationKey = "timeInterpolation";
        private const string VerticalIntepolationKey = "Vertical interpolation";
        private const string VerticalPositionTypeKey = "Vertical position type";
        private const string VerticalPositionSpecKey = "Vertical position specification";
        private const string VerticalPositionKey = "Vertical position";
        private const string OffsetKey = "Offset";
        private const string FactorKey = "Factor";

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

        public BcFile()
        {
            MultiFileMode = WriteMode.SingleFile;
        }

        public WriteMode MultiFileMode { get; set; }

        public bool CorrectionFile { private get; set; }

        private static string AppendToFile(string filePath, string tag)
        {
            var extension = Path.GetExtension(filePath);
            if (extension == null) return filePath + tag;
            return filePath.Substring(0, filePath.Length - extension.Length) + tag + extension;
        }

        protected virtual List<string> SupportedProcesses
        {
            get
            {
                return supportedProcesses.Select(sp => FlowBoundaryCondition.GetProcessNameForQuantity(sp)).Distinct().ToList();
            }
        }

        public IEnumerable<IGrouping<string, Tuple<IBoundaryCondition, BoundaryConditionSet>>> GroupBoundaryConditions(
            IEnumerable<BoundaryConditionSet> boundaryConditionSets)
        {
            var discriminator = BcDiscriminator(MultiFileMode);
            return boundaryConditionSets.SelectMany(bcs =>
                bcs.BoundaryConditions.Where(bc => SupportedProcesses.Contains(bc.ProcessName) && bc.DataType != BoundaryConditionDataType.Empty) // don't write empty bc!
                .Select(bc => new Tuple<IBoundaryCondition, BoundaryConditionSet>(bc, bcs)))
                .GroupBy(t => discriminator(t.Item1));
        }

        public void Write(IEnumerable<BoundaryConditionSet> boundaryConditionSets, string filePath,
            BcFileFlowBoundaryDataBuilder boundaryDataBuilder, DateTime? refDate = null)
        {
            var grouping = GroupBoundaryConditions(boundaryConditionSets.ToList());

            foreach (var group in grouping)
            {
                var subFile = string.IsNullOrEmpty(group.Key) ? filePath : AppendToFile(filePath, "_" + group.Key);
                Write(group.ToDictionary(t => t.Item1, t => t.Item2), subFile, boundaryDataBuilder, refDate);
            }
        }

        public static bool IsCorrectionType(BoundaryConditionDataType dataType)
        {
            return dataType == BoundaryConditionDataType.AstroCorrection ||
                   dataType == BoundaryConditionDataType.HarmonicCorrection;
        }

        private static bool SimilarDataType(BoundaryConditionDataType dt1, BoundaryConditionDataType dt2)
        {
            if (dt1 == BoundaryConditionDataType.AstroCorrection && dt2 == BoundaryConditionDataType.AstroComponents)
                return true;
            if (dt1 == BoundaryConditionDataType.AstroComponents && dt2 == BoundaryConditionDataType.AstroCorrection)
                return true;
            if (dt1 == BoundaryConditionDataType.HarmonicCorrection && dt2 == BoundaryConditionDataType.Harmonics)
                return true;
            if (dt1 == BoundaryConditionDataType.Harmonics && dt2 == BoundaryConditionDataType.HarmonicCorrection)
                return true;
            return dt1 == dt2;
        }

        public virtual void Write(IEnumerable<KeyValuePair<IBoundaryCondition, BoundaryConditionSet>> boundaryConditions,
            string filePath, BcFileFlowBoundaryDataBuilder boundaryDataBuilder, DateTime? refDate = null, bool appendToFile = false)
        {
            if (!appendToFile)
            {
                var generalRegion = GeneralRegionGenerator.GenerateGeneralRegion(
                    GeneralRegion.BoundaryConditionsMajorVersion, GeneralRegion.BoundaryConditionsMinorVersion,
                    GeneralRegion.FileTypeName.BoundaryConditions);
                new IniFileWriter().WriteIniFile(new[] { generalRegion }, filePath);
            }

            OpenOutputFile(filePath, true);
            try
            {
                foreach (var boundaryConditionKeyValuePair in boundaryConditions)
                {
                    if (CorrectionFile && !IsCorrectionType(boundaryConditionKeyValuePair.Key.DataType))
                    {
                        continue;
                    }

                    var boundaryCondition = boundaryConditionKeyValuePair.Key;
                    var supportPointNames = boundaryConditionKeyValuePair.Value.SupportPointNames;

                    var seriesIndex =
                        boundaryConditionKeyValuePair.Value.BoundaryConditions.Where(
                            bc => bc.VariableName == boundaryCondition.VariableName &&
                                  SimilarDataType(bc.DataType, boundaryCondition.DataType))
                            .ToList()
                            .IndexOf(boundaryCondition);

                    var blockData = boundaryDataBuilder.CreateBlockData(boundaryCondition as FlowBoundaryCondition,
                            supportPointNames, refDate, seriesIndex, CorrectionFile);

                    foreach (var block in blockData)
                    {
                        WriteBlock(block);
                        WriteLine("");
                    }

                }
            }
            finally
            {
                CloseOutputFile();
            }
        }

        public virtual void Write(IFmMeteoField fmMeteoField, string filePath, BcMeteoFileDataBuilder bcMeteoFileDataBuilder, DateTime refDate, bool appendToFile)
        {
            if (!appendToFile)
            {
                var generalRegion = GeneralRegionGenerator.GenerateGeneralRegion(
                    GeneralRegion.BoundaryConditionsMajorVersion, GeneralRegion.BoundaryConditionsMinorVersion,
                    GeneralRegion.FileTypeName.BoundaryConditions);
                new IniFileWriter().WriteIniFile(new[] { generalRegion }, filePath);
            }

            OpenOutputFile(filePath, true);
            try
            {
                var blockData = bcMeteoFileDataBuilder.CreateBlockData(fmMeteoField, refDate);

                WriteBlock(blockData);
                WriteLine("");
            }
            finally
            {
                CloseOutputFile();
            }
        }

        private void WriteKeyValuePairLine(string keyString, string valueString)
        {
            WriteLine(keyString.PadRight(columnWidth) + " = " + valueString);
        }

        protected virtual void WriteBlock(BcBlockData block)
        {
            WriteLine(BlockKey);
            
            WriteKeyValuePairLine(SupportPointKey, block.SupportPoint);
            
            WriteKeyValuePairLine(FunctionTypeKey, block.FunctionType);
            
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
            
            foreach (var quantity in block.Quantities)
            {
                WriteKeyValuePairLine(QuantityKey, quantity.Quantity);
                if (quantity.Unit != null)
                {
                    WriteKeyValuePairLine(UnitKey, quantity.Unit);
                }
                if (quantity.VerticalPosition != null)
                {
                    WriteKeyValuePairLine(VerticalPositionKey, quantity.VerticalPosition);
                }
            }

            var rowCount = block.Quantities.Select(q => q.Values.Count).Min();
            
            if (rowCount == 0) return;
            
            var columnWidths =
                block.Quantities.Select(q => q.Values.Take(rowCount).Select(s => s.Length).Max() + 1).ToList();
            
            for (var i = 0; i < rowCount; ++i)
            {
                var j = 0;
                WriteLine(string.Join(" ", block.Quantities.Select(q => q.Values[i].PadRight(columnWidths[j++]))).TrimEnd());
            }
        }

        public virtual IEnumerable<BcBlockData> Read(string inputFile)
        {
            OpenInputFile(inputFile);
            try
            {
                var line = GetNextLine();
                while (line != null)
                {
                    if (LineIsStartOfNewBlock(line))
                    {
                        var block = ReadDataBlock(out line);
                        if (block != null)
                        {
                            yield return block;
                        }
                    }
                    else
                    {
                        if (line.StartsWith(GeneralRegion.Header))
                        {
                            line = GetNextLine(); //fileVersion
                            line = GetNextLine(); //fileType
                        }
                        else
                        {
                            log.WarnFormat("Omitting line {0} not starting with {1}", LineNumber, BlockKey);
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

        static string[] SplitString(string str)
        {
            return str.Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
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

            var lineNumber = LineNumber;

            line = GetNextLine();

            while (line != null)
            {
                var split = SplitString(line);
                if (split.Length < 2)
                {
                    break;
                }
                if (String.Equals(split[0], SupportPointKey, StringComparison.OrdinalIgnoreCase))
                {
                    supportPointName = split[1];
                }
                if (String.Equals(split[0], FunctionTypeKey, StringComparison.OrdinalIgnoreCase))
                {
                    forcingType = split[1];
                }
                if (String.Equals(split[0], TimeInterpolationKey, StringComparison.OrdinalIgnoreCase))
                {
                    timeInterpolationType = split[1];
                }
                if (String.Equals(split[0], VerticalPositionTypeKey, StringComparison.OrdinalIgnoreCase))
                {
                    verticalPositionType = split[1];
                }
                if (String.Equals(split[0], VerticalPositionSpecKey, StringComparison.OrdinalIgnoreCase))
                {
                    verticalPositionSpecification = split[1];
                }
                if (String.Equals(split[0], VerticalIntepolationKey, StringComparison.OrdinalIgnoreCase))
                {
                    verticalInterpolationType = split[1];
                }
                if (String.Equals(split[0], SeriesIndexKey, StringComparison.OrdinalIgnoreCase))
                {
                    seriesIndex = split[1];
                }
                if (String.Equals(split[0], OffsetKey, StringComparison.OrdinalIgnoreCase))
                {
                    offset = split[1];
                }
                if (String.Equals(split[0], FactorKey, StringComparison.OrdinalIgnoreCase))
                {
                    factor = split[1];
                }
                if (String.Equals(split[0], QuantityKey, StringComparison.OrdinalIgnoreCase))
                {
                    if (quantityData != null)
                    {
                        quantityDataList.Add(quantityData);
                    }
                    if (split.Length == 2)
                    {
                        quantityData = new BcQuantityData { Quantity = split[1] };
                    }
                }
                if (String.Equals(split[0], UnitKey, StringComparison.OrdinalIgnoreCase))
                {
                    if (quantityData == null) continue;
                    quantityData.Unit = split[1];
                }
                if (String.Equals(split[0], VerticalPositionKey, StringComparison.OrdinalIgnoreCase))
                {
                    if (quantityData == null) continue;
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
                if (LineIsStartOfNewBlock(line)) break;
                var columns = line.SplitOnEmptySpace();
                if(columns.Length<quantityDataList.Count)
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

        private bool LineIsStartOfNewBlock(string line)
        {
            return line[0] == '[' && line.Length >= BlockKey.Length && line.Substring(0, BlockKey.Length).Equals(BlockKey, StringComparison.InvariantCultureIgnoreCase);
        }

        public void Write(BcIniSection boundaryBcSection, string file, string path, bool appendToFile)
        {
            var bcWriter = new BcWriter(new FileSystem());
            switch (MultiFileMode)
            {
                case WriteMode.SingleFile:
                {
                    var filename = Path.Combine(path, file);
                    WriteBc1DFile(boundaryBcSection, filename, bcWriter, appendToFile);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            
        }

        private static void WriteBc1DFile(BcIniSection bcSection, string filename, BcWriter bcWriter, bool appendToFile)
        {
            BcIniSection generalRegionBcSection = CreateGeneralBcIniSection();

            var model1DNodeBoundaryBcSections = new List<BcIniSection>();
            if (!appendToFile)
            {
                model1DNodeBoundaryBcSections.Add(generalRegionBcSection);
            }
            model1DNodeBoundaryBcSections.Add(bcSection);

            bcWriter.WriteBcFile(model1DNodeBoundaryBcSections, filename, appendToFile);
        }

        private static BcIniSection CreateGeneralBcIniSection()
        {
            IniSection generalRegion = GeneralRegionGenerator.GenerateGeneralRegion(
                GeneralRegion.BoundaryConditionsMajorVersion, GeneralRegion.BoundaryConditionsMinorVersion,
                GeneralRegion.FileTypeName.BoundaryConditions);
            
            return new BcIniSection(generalRegion);
        }
    }
}
