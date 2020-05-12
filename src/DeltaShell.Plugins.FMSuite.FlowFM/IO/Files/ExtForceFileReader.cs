using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Feature;
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public class ExtForceFileReader
    {
        public Dictionary<string, List<string>> CommentBlocks { get; }
        private readonly ILog log = LogManager.GetLogger(typeof(ExtForceFileReader));

        public IDictionary<ExtForceFileItem, object> ExistingForceFileItems { get; }
        public HashSet<ExtForceFileItem> SupportedExtForceFileItems { get; }

        public List<List<string>> HeadingCommentBlocks { get; }

        public IDictionary<IFeatureData, ExtForceFileItem> PolylineForceFileItems { get; }

        private StreamReader reader;

        private string storedNextInputLine;
        private List<string> currentCommentBlock;
        private bool fileContentHasStarted;

        private string currentLine;

        public ExtForceFileReader()
        {
            ExistingForceFileItems = new Dictionary<ExtForceFileItem, object>();
            SupportedExtForceFileItems = new HashSet<ExtForceFileItem>();
            PolylineForceFileItems = new Dictionary<IFeatureData, ExtForceFileItem>();

            HeadingCommentBlocks = new List<List<string>>();
            CommentBlocks = new Dictionary<string, List<string>>();
        }

        public void Read(string extForceFilePath, WaterFlowFMModelDefinition modelDefinition,
                         string extSubFilesReferenceFilePath)
        {
            ExtSubFilesReferenceFilePath = extSubFilesReferenceFilePath;
            ExtFilePath = extForceFilePath;

            Read(modelDefinition);
        }

        private void Read(WaterFlowFMModelDefinition modelDefinition)
        {
            IEnumerable<ExtForceFileItem> extForceFileItems = ParseExtForceFile();
            IList<ExtForceFileItem> forceFileItems =
                extForceFileItems as IList<ExtForceFileItem> ?? extForceFileItems.ToList();

            ReadPolyLineData(forceFileItems, modelDefinition);
            ReadWindItems(forceFileItems, modelDefinition);
            ReadHeatFluxModelData(forceFileItems, modelDefinition);
            ReadSpatialData(forceFileItems, modelDefinition);
            StoreUnknownQuantities(forceFileItems, modelDefinition);
        }

        private SourceAndSink ReadSourceAndSinkData(string filePath, Feature2D feature2D,
                                                          ExtForceFileItem extForceFileItem,
                                                          DateTime modelReferenceDate)
        {
            if (!Equals(extForceFileItem.Quantity, ExtForceQuantNames.SourceAndSink))
            {
                return null;
            }

            var sourceAndSink = new SourceAndSink
            {
                Feature = feature2D,
            };
            extForceFileItem.ModelData.TryGetValue(ExtForceFileConstants.AreaKey, out object area);
            if (area != null)
            {
                sourceAndSink.Area = Convert.ToDouble(area);
            }

            string dataFilePath = Path.ChangeExtension(filePath, ExtForceQuantNames.TimFileExtension);

            if (!File.Exists(dataFilePath))
            {
                return sourceAndSink;
            }

            ReadSourceAndSinkValues(sourceAndSink, dataFilePath, modelReferenceDate);

            return sourceAndSink;
        }

        private void ReadSourceAndSinkValues(SourceAndSink sourceAndSink, string filePath,
                                                    DateTime modelReferenceDate)
        {
            IFunction data = sourceAndSink.Data;
            if (data == null)
            {
                log.ErrorFormat(Resources.Read_SourceAndSink_values_failed__no_function_detected_for_SourceAndSink__0_,
                                sourceAndSink.Name);
                return;
            }

            TimeSeries readFunction = new TimFile().Read(filePath, modelReferenceDate);
            sourceAndSink.CopyValuesFromFileToSourceAndSinkAttributes(readFunction);
        }

        private string GetNextLine()
        {
            if (reader == null)
            {
                throw new InvalidOperationException("Input file not opened for reading: " + (InputFilePath ?? "(no file)"));
            }

            if (storedNextInputLine != null)
            {
                var nextLine = storedNextInputLine;
                storedNextInputLine = null;
                return nextLine;
            }

            LineNumber++;
            CurrentLine = reader.ReadLine();

            while (CurrentLine != null)
            {
                var trimmedLine = CurrentLine.Trim();

                if (CheckAndProcessCommentInputLine(CurrentLine, trimmedLine))
                {
                    CurrentLine = GetNextLine();
                }
                else if (CheckAndProcessEmptyInputLine(CurrentLine, trimmedLine))
                {
                    CurrentLine = GetNextLine();
                }
                else
                {
                    string nextLine = trimmedLine.Replace('\t', ' '); // avoid having to parse or split on tabs
                    if (currentCommentBlock != null)
                    {
                        if (!fileContentHasStarted)
                        {
                            HeadingCommentBlocks.Add(currentCommentBlock);
                            currentCommentBlock = null;
                        }
                        else
                        {
                            CreateCommonBlock();
                        }

                        currentCommentBlock = null;
                    }

                    fileContentHasStarted = true;
                    return nextLine;
                }
            }

            return null;
        }

        public void CreateCommonBlock()
        {
            if (CurrentLine.ToUpper().StartsWith(ExtForceFileConstants.ExtForcesFileQuantBlockStarter))
            {
                LineNumber++;
                storedNextInputLine = reader.ReadLine();
                if (storedNextInputLine != null)
                {
                    string contentIdentifier =
                        CreateContentIdentifier(CurrentLine.Trim() + storedNextInputLine.Trim());
                    CommentBlocks.Add(contentIdentifier, currentCommentBlock);
                }
            }
            else
            {
                // can not handle internal comments
                currentCommentBlock = null;
            }
        }

        private static string CreateContentIdentifier(string line)
        {
            if (line == null)
            {
                return string.Empty;
            }

            var i = 0;
            var contentIdentifier = new char[line.Length];
            foreach (char c in line)
            {
                if (c == ' ' || c == '\t')
                {
                    continue;
                }

                if (c == '#' || c == '!' || c == '*')
                {
                    break;
                }

                contentIdentifier[i++] = c;
            }

            return new string(contentIdentifier, 0, i);
        }

        private int LineNumber { get; set; }
        private string InputFilePath { get; set; }
        private string OutputFilePath { get; set; }
        private string CurrentLine { get; set; }

        private string ExtFilePath { get; set; }

        private string ExtSubFilesReferenceFilePath { get; set; }

        private IEnumerable<ExtForceFileItem> GetUnknownExtForceFileItems(
            IEnumerable<ExtForceFileItem> allExtForceFileItems)
        {
            return allExtForceFileItems.Except(SupportedExtForceFileItems);
        }

        private void StoreUnknownQuantities(IEnumerable<ExtForceFileItem> allExtForceFileItems,
                                            WaterFlowFMModelDefinition modelDefinition)
        {
            List<ExtForceFileItem> unknownForceFileItems = GetUnknownExtForceFileItems(allExtForceFileItems).ToList();
            foreach (ExtForceFileItem unknownForceFileItem in unknownForceFileItems)
            {
                log.WarnFormat(
                    Resources
                        .ExtForceFile_StoreUnknownQuantities_Quantity___0___detected_in_the_external_force_file_and_will_be_passed_to_the_computational_core__This_may_affect_your_simulation_,
                    unknownForceFileItem.Quantity);

                string referencedFilePath =
                    NGHSFileBase.GetOtherFilePathInSameDirectory(ExtSubFilesReferenceFilePath, unknownForceFileItem.FileName);

                var unsupportedFileBasedExtForceFileItem =
                    new UnsupportedFileBasedExtForceFileItem(referencedFilePath, unknownForceFileItem);

                modelDefinition.UnsupportedFileBasedExtForceFileItems.Add(unsupportedFileBasedExtForceFileItem);
            }
        }

        private bool CheckAndProcessCommentInputLine(string line, string trimmedLine)
        {
            if (trimmedLine.Length == 0)
                return false;

            var firstChar = trimmedLine[0];
            if (firstChar == '#' ||
                firstChar == '!' ||
                firstChar == '*' ||
                firstChar == ':') // part of header in external forcings file
            {
                if (currentCommentBlock == null)
                {
                    currentCommentBlock = new List<string>();
                }

                currentCommentBlock.Add(line);
                return true;
            }

            return false;
        }

        private bool CheckAndProcessEmptyInputLine(string line, string trimmedLine)
        {
            if (trimmedLine.Length == 0)
            {
                if (currentCommentBlock != null)
                {
                    if (!fileContentHasStarted)
                    {
                        HeadingCommentBlocks.Add(currentCommentBlock);
                        currentCommentBlock = null;
                    }
                    else
                    {
                        currentCommentBlock.Add(line);
                    }
                }

                return true;
            }

            return false;
        }

        private IEnumerable<ExtForceFileItem> ParseExtForceFile()
        {
            OpenInputFile(ExtFilePath);

            try
            {
                currentLine = GetNextLine();

                while (currentLine != null && IsNewEntry(currentLine))
                {
                    int startLineNumber = LineNumber;

                    ExtForceFileItem extForceFileItem = ReadQuantityBlock(startLineNumber);

                    if (IsValidQuantity(extForceFileItem))
                    {
                        yield return extForceFileItem;
                    }
                    else
                    {
                        log.WarnFormat(
                            $"Invalid Quantity item '{extForceFileItem.Quantity}' starting on line {startLineNumber} in file {ExtFilePath}; Item is skipped.");
                    }
                }
            }
            finally
            {
                reader.Close();
            }
        }

        private void OpenInputFile(string filePath)
        {
            reader = new StreamReader(filePath);
            fileContentHasStarted = false;
            LineNumber = 0;
        }

        private ExtForceFileItem ReadQuantityBlock(int startLineNumber)
        {
            string propertyName = GetKeyPart(currentLine);
            var extForceFileItem = new ExtForceFileItem(GetValuePart(currentLine));

            if (propertyName != ExtForceFileConstants.QuantityKey)
            {
                //something other than QUANTITY must be disabled
                extForceFileItem.Enabled = false;
            }

            currentLine = GetNextLine();

            try
            {
                while (currentLine != null && !IsNewEntry(currentLine))
                {
                    ReadQuantityProperty(extForceFileItem);
                    currentLine = GetNextLine();
                }
            }
            catch (FormatException e)
            {
                log.ErrorFormat("An error occured while reading Quantity item starting at line {0}: {1}.",
                                startLineNumber, e.Message);
            }

            return extForceFileItem;
        }

        private void ReadQuantityProperty(ExtForceFileItem extForceFileItem)
        {
            string propertyName = GetKeyPart(currentLine);

            switch (propertyName)
            {
                case ExtForceFileConstants.FileNameKey:
                    SetFileName(extForceFileItem);
                    break;
                case ExtForceFileConstants.FileTypeKey:
                    SetFileType(extForceFileItem);
                    break;
                case ExtForceFileConstants.MethodKey:
                    SetMethod(extForceFileItem);
                    break;
                case ExtForceFileConstants.OperandKey:
                    SetOperand(extForceFileItem);
                    break;
                case ExtForceFileConstants.ValueKey:
                    SetValue(extForceFileItem);
                    break;
                case ExtForceFileConstants.FactorKey:
                    SetFactor(extForceFileItem);
                    break;
                case ExtForceFileConstants.OffsetKey:
                    SetOffset(extForceFileItem);
                    break;
                case ExtForceFileConstants.AreaKey:
                    SetArea(extForceFileItem);
                    break;
                case ExtForceFileConstants.AveragingTypeKey:
                    SetAveragingType(extForceFileItem);
                    break;
                case ExtForceFileConstants.RelSearchCellSizeKey:
                    SetRelativeSearchCellSize(extForceFileItem);
                    break;
                case ExtForceFileConstants.FricTypeKey:
                    SetFrictionType(extForceFileItem);
                    break;
                default:
                    log.WarnFormat(
                        Resources
                            .ExtForceFile_ReadQuantityProperty_Unexpected_line___0___on_line__1__in_file__2__and_will_be_ignored_,
                        currentLine, LineNumber, ExtFilePath);
                    break;
            }
        }

        private void SetFileName(ExtForceFileItem extForceFileItem)
        {
            if (string.IsNullOrEmpty(extForceFileItem.FileName))
            {
                extForceFileItem.FileName = GetValuePart(currentLine);
            }
            else
            {
                LogWarningQuantityPropertyAlreadySet(ExtForceFileConstants.FileNameKey);
            }
        }

        private void SetFileType(ExtForceFileItem extForceFileItem)
        {
            if (extForceFileItem.FileName == null)
            {
                throw new FormatException(GetMessageUnexpectedKeyword(ExtForceFileConstants.FileTypeKey));
            }

            if (extForceFileItem.FileType == int.MinValue)
            {
                extForceFileItem.FileType = GetIntegerPropertyValue(currentLine);
            }
            else
            {
                LogWarningQuantityPropertyAlreadySet(ExtForceFileConstants.FileTypeKey);
            }
        }

        private void SetMethod(ExtForceFileItem extForceFileItem)
        {
            if (extForceFileItem.FileType == int.MinValue)
            {
                throw new FormatException(GetMessageUnexpectedKeyword(ExtForceFileConstants.MethodKey));
            }

            if (extForceFileItem.Method == int.MinValue)
            {
                extForceFileItem.Method = GetIntegerPropertyValue(currentLine);

                // backward compatibility: samples triangulation changed from 4 to 5 in #30984
                if (extForceFileItem.FileType == ExtForceQuantNames.FileTypes.Triangulation &&
                    extForceFileItem.Method == 4)
                {
                    extForceFileItem.Method = 5;
                }
            }
            else
            {
                LogWarningQuantityPropertyAlreadySet(ExtForceFileConstants.MethodKey);
            }
        }

        private void SetOperand(ExtForceFileItem extForceFileItem)
        {
            if (extForceFileItem.Method == int.MinValue)
            {
                throw new FormatException(GetMessageUnexpectedKeyword(ExtForceFileConstants.OperandKey));
            }

            if (extForceFileItem.Operand == null)
            {
                extForceFileItem.Operand = GetValuePart(currentLine);
            }
            else
            {
                LogWarningQuantityPropertyAlreadySet(ExtForceFileConstants.OperandKey);
            }
        }

        private void SetValue(ExtForceFileItem extForceFileItem)
        {
            if (double.IsNaN(extForceFileItem.Value))
            {
                extForceFileItem.Value = GetDouble(GetValuePart(currentLine));
            }
            else
            {
                LogWarningQuantityPropertyAlreadySet(ExtForceFileConstants.ValueKey);
            }
        }

        private void SetFactor(ExtForceFileItem extForceFileItem)
        {
            if (double.IsNaN(extForceFileItem.Factor))
            {
                extForceFileItem.Factor = GetDouble(GetValuePart(currentLine));
            }
            else
            {
                LogWarningQuantityPropertyAlreadySet(ExtForceFileConstants.FactorKey);
            }
        }

        private void SetOffset(ExtForceFileItem extForceFileItem)
        {
            if (extForceFileItem.Operand == null)
            {
                throw new FormatException(GetMessageUnexpectedKeyword(ExtForceFileConstants.OffsetKey));
            }

            if (double.IsNaN(extForceFileItem.Offset))
            {
                extForceFileItem.Offset = GetDouble(GetValuePart(currentLine));
            }
            else
            {
                LogWarningQuantityPropertyAlreadySet(ExtForceFileConstants.OffsetKey);
            }
        }

        private void SetArea(ExtForceFileItem extForceFileItem)
        {
            if (extForceFileItem.Quantity != ExtForceQuantNames.SourceAndSink && extForceFileItem.Operand == null)
            {
                throw new FormatException(GetMessageUnexpectedKeyword(ExtForceFileConstants.AreaKey));
            }

            if (extForceFileItem.ModelData.ContainsKey(ExtForceFileConstants.AreaKey))
            {
                LogWarningQuantityPropertyAlreadySet(ExtForceFileConstants.AreaKey);
            }

            extForceFileItem.ModelData[ExtForceFileConstants.AreaKey] = GetDoublePropertyValue(currentLine);
        }

        private void SetAveragingType(ExtForceFileItem extForceFileItem)
        {
            if (extForceFileItem.FileType != ExtForceQuantNames.FileTypes.Triangulation &&
                extForceFileItem.Operand == null)
            {
                throw new FormatException(GetMessageUnexpectedKeyword(ExtForceFileConstants.AveragingTypeKey));
            }

            if (extForceFileItem.ModelData.ContainsKey(ExtForceFileConstants.AveragingTypeKey))
            {
                LogWarningQuantityPropertyAlreadySet(ExtForceFileConstants.AveragingTypeKey);
            }

            extForceFileItem.ModelData[ExtForceFileConstants.AveragingTypeKey] = GetIntegerPropertyValue(currentLine);
        }

        private void SetRelativeSearchCellSize(ExtForceFileItem extForceFileItem)
        {
            if (extForceFileItem.FileType != ExtForceQuantNames.FileTypes.Triangulation &&
                extForceFileItem.Operand == null)
            {
                throw new FormatException(GetMessageUnexpectedKeyword(ExtForceFileConstants.RelSearchCellSizeKey));
            }

            if (extForceFileItem.ModelData.ContainsKey(ExtForceFileConstants.RelSearchCellSizeKey))
            {
                LogWarningQuantityPropertyAlreadySet(ExtForceFileConstants.RelSearchCellSizeKey);
            }

            extForceFileItem.ModelData[ExtForceFileConstants.RelSearchCellSizeKey] = GetDoublePropertyValue(currentLine);
        }

        private void SetFrictionType(ExtForceFileItem extForceFileItem)
        {
            if (extForceFileItem.ModelData.ContainsKey(ExtForceFileConstants.FricTypeKey))
            {
                LogWarningQuantityPropertyAlreadySet(ExtForceFileConstants.FricTypeKey);
            }

            extForceFileItem.ModelData[ExtForceFileConstants.FricTypeKey] = GetIntegerPropertyValue(currentLine);
        }

        private void LogWarningQuantityPropertyAlreadySet(string quantityName)
        {
            log.WarnFormat(
                Resources
                    .ExtForceFile_LogWarningQuantityPropertyAlreadySet__0__is_already_set__Line__1__of_file__2__will_be_ignored_,
                quantityName, LineNumber, ExtFilePath);
        }

        private string GetMessageUnexpectedKeyword(string quantityName)
        {
            return string.Format(
                Resources.ExtForceFile_GetMessageUnexpectedKeyword_Unexpected_keyword__0__on_line__1__of_file__2_,
                quantityName, LineNumber, ExtFilePath);
        }

        private static bool IsValidQuantity(ExtForceFileItem extForceFileItem)
        {
            return !(string.IsNullOrEmpty(extForceFileItem?.FileName)
                     || extForceFileItem.FileType == int.MinValue
                     || extForceFileItem.Method == int.MinValue
                     || extForceFileItem.Operand == null
                     || !extForceFileItem.Enabled);
        }

        private static bool IsNewEntry(string line)
        {
            string lineToLower = line.ToLower();
            IEnumerable<string> keysToCheck = new[]
            {
                ExtForceFileConstants.QuantityKey,
                ExtForceFileConstants.DisabledQuantityKey
            }.Concat(ExtForceFileConstants.UnsupportedQuantityKeys).Select(s => s.ToLower());

            return keysToCheck.Any(lineToLower.StartsWith);
        }

        private void ReadPolyLineData(IEnumerable<ExtForceFileItem> extForceFileItems,
                                      WaterFlowFMModelDefinition modelDefinition)
        {
            IEventedList<BoundaryConditionSet> boundaryConditionSets = modelDefinition.BoundaryConditionSets;
            var boundaryConditions = new List<IBoundaryCondition>();
            var sourcesAndSinks = new List<SourceAndSink>();

            var modelReferenceDate = (DateTime) modelDefinition.GetModelProperty(KnownProperties.RefDate).Value;

            foreach (ExtForceFileItem extForceFileItem in extForceFileItems.Where(
                e => e.FileName.ToLower().EndsWith(FileConstants.PliFileExtension)))
            {
                if (extForceFileItem.FileType != ExtForceQuantNames.FileTypes.PolyTim)
                {
                    throw new NotSupportedException("The provided pli file is not a PolyTim file. " +
                                                    extForceFileItem.FileName);
                }

                // check what type of polyline to read
                bool isSourceAndSink = Equals(extForceFileItem.Quantity, ExtForceQuantNames.SourceAndSink);

                if (!ExtForceQuantNames.TryParseBoundaryQuantityType(extForceFileItem.Quantity,
                                                                     out FlowBoundaryQuantityType quantityType) &&
                    !isSourceAndSink)
                {
                    continue;
                }

                SupportedExtForceFileItems.Add(extForceFileItem);

                // read the pli file
                string pliFilePath =
                    NGHSFileBase.GetOtherFilePathInSameDirectory(ExtSubFilesReferenceFilePath, extForceFileItem.FileName);

                var pliFileReader = new PliFile<Feature2D>();
                if (isSourceAndSink)
                {
                    pliFileReader.CreateDelegate =
                        (points, name) =>
                            points.Count == 1
                                ? new Feature2DPoint
                                {
                                    Name = name,
                                    Geometry = new Point(points[0])
                                }
                                : new Feature2D
                                {
                                    Name = name,
                                    Geometry = LineStringCreator.CreateLineString(points)
                                };
                }

                IList<Feature2D> features2D = pliFileReader.Read(pliFilePath);
                ExistingForceFileItems[extForceFileItem] = features2D;

                // go through all feature2Ds
                foreach (Feature2D feature2D in features2D)
                {
                    if (isSourceAndSink)
                    {
                        SourceAndSink sourceAndSink;
                        try
                        {
                            sourceAndSink =
                                ReadSourceAndSinkData(
                                    pliFilePath, feature2D, extForceFileItem, modelReferenceDate);
                        }
                        catch (Exception e)
                        {
                            if (e is ArgumentException || e is PathTooLongException || e is FormatException ||
                                e is FileNotFoundException || e is IOException || e is OutOfMemoryException)
                            {
                                throw new InvalidOperationException(
                                    $"An error (Message: {e.Message}) occured while source/sink data for feature {feature2D.Name} in file {ExtFilePath}",
                                    e);
                            }

                            throw;
                        }

                        if (sourceAndSink == null)
                        {
                            continue;
                        }

                        PolylineForceFileItems[sourceAndSink] = extForceFileItem;
                        sourcesAndSinks.Add(sourceAndSink);
                    }
                    else // boundary condition
                    {
                        Feature2D uniqueFeature =
                            boundaryConditionSets.Select(bcs => bcs.Feature)
                                                 .FirstOrDefault(f => f.Geometry.EqualsTopologically(feature2D.Geometry)) ??
                            feature2D;

                        if (uniqueFeature == feature2D)
                        {
                            boundaryConditionSets.Add(new BoundaryConditionSet
                            {
                                Feature = feature2D
                            });
                        }

                        BoundaryCondition boundaryCondition;

                        try
                        {
                            boundaryCondition = ReadBoundaryConditionData(pliFilePath,
                                                                                             uniqueFeature,
                                                                                             extForceFileItem,
                                                                                             modelReferenceDate);
                        }
                        catch (Exception e)
                        {
                            if (e is ArgumentException || e is PathTooLongException || e is FormatException ||
                                e is FileNotFoundException || e is IOException || e is OutOfMemoryException)
                            {
                                throw new InvalidOperationException(
                                    $"An error (Message: {e.Message}) occured while reading boundary condition data for feature {feature2D.Name} in file {ExtFilePath}",
                                    e);
                            }

                            throw;
                        }

                        if (boundaryCondition == null)
                        {
                            continue;
                        }

                        PolylineForceFileItems[boundaryCondition] = extForceFileItem;
                        boundaryConditions.Add(boundaryCondition);
                    }
                }
            }

            foreach (BoundaryConditionSet boundaryConditionSet in boundaryConditionSets)
            {
                var feature = boundaryConditionSet.Feature as IFeature;
                boundaryConditionSet.BoundaryConditions = new EventedList<IBoundaryCondition>();
                boundaryConditionSet.BoundaryConditions.AddRange(
                    boundaryConditions.Where(bc => Equals(bc.Feature, feature)));
            }

            NamingHelper.MakeNamesUnique(boundaryConditionSets.Select(bd => bd.Feature).Cast<INameable>().ToList());
            NamingHelper.MakeNamesUnique(sourcesAndSinks.Select(bd => bd.Feature).Cast<INameable>().ToList());

            modelDefinition.Boundaries.AddRange(boundaryConditionSets.Select(bd => bd.Feature));
            modelDefinition.SourcesAndSinks.AddRange(sourcesAndSinks);
            modelDefinition.Pipes.AddRange(sourcesAndSinks.Select(ss => ss.Feature).Distinct());
        }

        private static IWindField CreateWindField(ExtForceFileItem extForceFileItem, string extForceFilePath)
        {
            if (!ExtForceQuantNames.WindQuantityNames.Values.Contains(extForceFileItem.Quantity))
            {
                throw new NotSupportedException($"Wind quantity {extForceFileItem.Quantity} is not supported");
            }

            WindQuantity quantity = ExtForceQuantNames
                                    .WindQuantityNames.First(kvp => kvp.Value == extForceFileItem.Quantity).Key;

            string fileName = extForceFileItem.FileName == null
                                  ? null
                                  : Path.Combine(Path.GetDirectoryName(extForceFilePath), extForceFileItem.FileName);

            switch (extForceFileItem.FileType)
            {
                case ExtForceQuantNames.FileTypes.Uniform:
                    if (quantity == WindQuantity.VelocityX)
                    {
                        return UniformWindField.CreateWindXSeries();
                    }

                    if (quantity == WindQuantity.VelocityY)
                    {
                        return UniformWindField.CreateWindYSeries();
                    }

                    if (quantity == WindQuantity.VelocityVector)
                    {
                        return UniformWindField.CreateWindXYSeries();
                    }

                    if (quantity == WindQuantity.AirPressure)
                    {
                        return UniformWindField.CreatePressureSeries();
                    }

                    break;
                case ExtForceQuantNames.FileTypes.UniMagDir:
                    if (quantity == WindQuantity.VelocityVector)
                    {
                        return UniformWindField.CreateWindPolarSeries();
                    }

                    break;
                case ExtForceQuantNames.FileTypes.ArcInfo:
                    if (quantity == WindQuantity.VelocityX)
                    {
                        return GriddedWindField.CreateXField(fileName);
                    }

                    if (quantity == WindQuantity.VelocityY)
                    {
                        return GriddedWindField.CreateYField(fileName);
                    }

                    if (quantity == WindQuantity.AirPressure)
                    {
                        return GriddedWindField.CreatePressureField(fileName);
                    }

                    break;
                case ExtForceQuantNames.FileTypes.SpiderWeb:
                    if (quantity == WindQuantity.VelocityVectorAirPressure)
                    {
                        return SpiderWebWindField.Create(fileName);
                    }

                    break;
                case ExtForceQuantNames.FileTypes.Curvi:
                    if (quantity == WindQuantity.VelocityVectorAirPressure)
                    {
                        return GriddedWindField.CreateCurviField(fileName,
                                                                 GriddedWindField
                                                                     .GetCorrespondingGridFilePath(fileName));
                    }

                    break;
                case ExtForceQuantNames.FileTypes.NCgrid:
                    if (quantity == WindQuantity.VelocityX)
                    {
                        return GriddedWindField.CreateXField(fileName);
                    }

                    if (quantity == WindQuantity.VelocityY)
                    {
                        return GriddedWindField.CreateYField(fileName);
                    }

                    if (quantity == WindQuantity.AirPressure)
                    {
                        return GriddedWindField.CreatePressureField(fileName);
                    }

                    break;
            }

            throw new NotSupportedException(
                $"External forcing for wind quantity {extForceFileItem.Quantity}, method {extForceFileItem.Method} and file type {extForceFileItem.FileType} is not supported");
        }

        private BoundaryCondition ReadBoundaryConditionData(string filePath, Feature2D feature2D,
                                                                 ExtForceFileItem extForceFileItem,
                                                                 DateTime modelReferenceDate)
        {
            if (ExtForceQuantNames.TryParseBoundaryQuantityType(extForceFileItem.Quantity, out FlowBoundaryQuantityType quantityType))
            {
                if (TryDetermineForcingType(feature2D, filePath, out BoundaryConditionDataType dataType,
                                            out IList<int> dataFileNumbers,
                                            out string fileExtension))
                {
                    // create a quantity name from the type and the tracer name if it is set to tracer.
                    string quantityName = quantityType.ToString();
                    if (quantityType == FlowBoundaryQuantityType.Tracer)
                    {
                        quantityName += "_" + extForceFileItem.Quantity.Substring(9); // remove tracerbnd
                    }

                    var boundaryCondition =
                        (FlowBoundaryCondition)new FlowBoundaryConditionFactory().CreateBoundaryCondition(
                            feature2D, quantityName, dataType);
                    if (boundaryCondition == null)
                    {
                        log.ErrorFormat("Could not create boundary condition of quantity type {0}", quantityName);
                        return null;
                    }

                    if (!double.IsNaN(extForceFileItem.Offset))
                    {
                        boundaryCondition.Offset = extForceFileItem.Offset;
                    }

                    if (!double.IsNaN(extForceFileItem.Factor))
                    {
                        boundaryCondition.Factor = extForceFileItem.Factor;
                    }

                    string[] splitExtension = fileExtension.Split('|');

                    foreach (string extension in splitExtension)
                    {
                        foreach (int dataFileNumber in dataFileNumbers)
                        {
                            int pointIndex = dataFileNumber == 0 ? 0 : dataFileNumber - 1;

                            boundaryCondition.AddPoint(pointIndex);

                            string dataFilePath = ExtForceFileHelper.GetNumberedFilePath(filePath, extension, dataFileNumber);

                            if (!File.Exists(dataFilePath))
                            {
                                continue;
                            }

                            ReadBoundaryConditionValues(boundaryCondition, dataFilePath, pointIndex,
                                                        modelReferenceDate);

                            if (boundaryCondition.IsHorizontallyUniform)
                            {
                                break;
                            }
                        }
                    }

                    return boundaryCondition;
                }
            }

            return null;
        }

        private static bool TryDetermineForcingType(IFeature feature2D, string filePath,
                                                   out BoundaryConditionDataType conditionDataType,
                                                   out IList<int> dataFileNumbers,
                                                   out string fileExtension)
        {
            IList<int> timFileNumbers = GetDataFileNumbers(filePath, feature2D.Geometry.NumPoints,
                                                           ExtForceQuantNames.TimFileExtension);

            IList<int> t3DFileNumbers = GetDataFileNumbers(filePath, feature2D.Geometry.NumPoints,
                                                           ExtForceQuantNames.T3DFileExtension);

            if (timFileNumbers.Any() && !t3DFileNumbers.Any())
            {
                dataFileNumbers = timFileNumbers;
                fileExtension = ExtForceQuantNames.TimFileExtension;
                conditionDataType = BoundaryConditionDataType.TimeSeries;
                return true;
            }

            if (t3DFileNumbers.Any() && !timFileNumbers.Any())
            {
                dataFileNumbers = t3DFileNumbers;
                fileExtension = ExtForceQuantNames.T3DFileExtension;
                conditionDataType = BoundaryConditionDataType.TimeSeries;
                return true;
            }

            if (timFileNumbers.Any() && t3DFileNumbers.Any())
            {
                dataFileNumbers = timFileNumbers.Concat(t3DFileNumbers).ToList();
                fileExtension = string.Join("|", ExtForceQuantNames.TimFileExtension,
                                            ExtForceQuantNames.T3DFileExtension);
                conditionDataType = BoundaryConditionDataType.TimeSeries;
                return true;
            }

            IList<int> cmpFileNumbers = GetDataFileNumbers(filePath, feature2D.Geometry.NumPoints,
                                                           ExtForceQuantNames.CmpFileExtension);
            if (cmpFileNumbers.Any())
            {
                dataFileNumbers = cmpFileNumbers;
                fileExtension = ExtForceQuantNames.CmpFileExtension;
                conditionDataType =
                    new CmpFile().GetForcingType(ExtForceFileHelper.GetNumberedFilePath(filePath, fileExtension, dataFileNumbers[0]));
                return true;
            }

            IList<int> qhFileNumbers = GetDataFileNumbers(filePath, 0, ExtForceQuantNames.QhFileExtension);

            if (qhFileNumbers.Any())
            {
                dataFileNumbers = qhFileNumbers;
                fileExtension = ExtForceQuantNames.QhFileExtension;
                conditionDataType = BoundaryConditionDataType.Qh;
                return true;
            }

            dataFileNumbers = new List<int>();
            conditionDataType = BoundaryConditionDataType.TimeSeries;
            fileExtension = ExtForceQuantNames.TimFileExtension;
            return true;
        }

        private static IList<int> GetDataFileNumbers(string fileNameOrPath, int numPointsOnPolyLine,
                                                     string fileExtension)
        {
            IList<int> dataFileNumbers = new List<int>();
            if (numPointsOnPolyLine == 0 && File.Exists(ExtForceFileHelper.GetNumberedFilePath(fileNameOrPath, fileExtension, 0)))
            {
                dataFileNumbers.Add(0);
            }
            else
            {
                for (var i = 1; i <= numPointsOnPolyLine; i++)
                {
                    string expectedFileName = ExtForceFileHelper.GetNumberedFilePath(fileNameOrPath, fileExtension, i);
                    if (File.Exists(expectedFileName))
                    {
                        dataFileNumbers.Add(i);
                    }
                }
            }

            return dataFileNumbers;
        }

        private static void ReadBoundaryConditionValues(IBoundaryCondition boundaryCondition, string filePath,
                                                        int pointIndex, DateTime modelReferenceDate)
        {
            IFunction data = boundaryCondition.GetDataAtPoint(pointIndex);
            if (data == null)
            {
                return;
            }

            IList<HarmonicComponent> harmonicComponents;
            switch (boundaryCondition.DataType)
            {
                case BoundaryConditionDataType.TimeSeries:
                    if (filePath.EndsWith(ExtForceQuantNames.T3DFileExtension))
                    {
                        TimeSeries series = new T3DFile().Read(filePath, out VerticalProfileDefinition verticalProfileDefinition);
                        int index = boundaryCondition.DataPointIndices.IndexOf(pointIndex);
                        boundaryCondition.PointDepthLayerDefinitions[index] = verticalProfileDefinition;
                        data = boundaryCondition.GetDataAtPoint(pointIndex);
                        FunctionHelper.SetValuesRaw<DateTime>(data.Arguments[0], series.Arguments[0].Values);
                        for (var i = 0; i < data.Components.Count; ++i)
                        {
                            FunctionHelper.SetValuesRaw<double>(data.Components[i], series.Components[i].Values);
                        }
                    }
                    else
                    {
                        new TimFile().Read(filePath, data, modelReferenceDate);
                    }

                    break;
                case BoundaryConditionDataType.Qh:
                    IFunction profile = new QhFile().Read(filePath);
                    FunctionHelper.SetValuesRaw<double>(data.Arguments[0], profile.Arguments[0].Values);
                    FunctionHelper.SetValuesRaw<double>(data.Components[0], profile.Components[0].Values);
                    break;
                case BoundaryConditionDataType.AstroComponents:
                    harmonicComponents = new CmpFile().Read(filePath, BoundaryConditionDataType.AstroComponents);
                    FunctionHelper.SetValuesRaw(data.Arguments[0], harmonicComponents.Select(h => h.Name));
                    FunctionHelper.SetValuesRaw(data.Components[0], harmonicComponents.Select(h => h.Amplitude));
                    FunctionHelper.SetValuesRaw(data.Components[1], harmonicComponents.Select(h => h.Phase));
                    break;
                case BoundaryConditionDataType.AstroCorrection:
                    harmonicComponents = new CmpFile().Read(filePath, BoundaryConditionDataType.AstroComponents);
                    FunctionHelper.SetValuesRaw(data.Arguments[0], harmonicComponents.Select(h => h.Name));
                    FunctionHelper.SetValuesRaw(data.Components[0], harmonicComponents.Select(h => h.Amplitude));
                    FunctionHelper.SetValuesRaw(data.Components[2], harmonicComponents.Select(h => h.Phase));
                    break;
                case BoundaryConditionDataType.Harmonics:
                    harmonicComponents = new CmpFile()
                                         .Read(filePath, BoundaryConditionDataType.Harmonics).OrderBy(c => c.Frequency)
                                         .ToList();
                    FunctionHelper.SetValuesRaw(data.Arguments[0], harmonicComponents.Select(h => h.Frequency));
                    FunctionHelper.SetValuesRaw(data.Components[0], harmonicComponents.Select(h => h.Amplitude));
                    FunctionHelper.SetValuesRaw(data.Components[1], harmonicComponents.Select(h => h.Phase));
                    break;
                case BoundaryConditionDataType.HarmonicCorrection:
                    harmonicComponents = new CmpFile()
                                         .Read(filePath, BoundaryConditionDataType.Harmonics).OrderBy(c => c.Frequency)
                                         .ToList();
                    FunctionHelper.SetValuesRaw(data.Arguments[0], harmonicComponents.Select(h => h.Frequency));
                    FunctionHelper.SetValuesRaw(data.Components[0], harmonicComponents.Select(h => h.Amplitude));
                    FunctionHelper.SetValuesRaw(data.Components[2], harmonicComponents.Select(h => h.Phase));
                    break;
            }
        }

        private void ReadHeatFluxModelData(IEnumerable<ExtForceFileItem> extForceFileItems,
                                           WaterFlowFMModelDefinition modelDefinition)
        {
            var modelReferenceDate = (DateTime) modelDefinition.GetModelProperty(KnownProperties.RefDate).Value;
            switch (modelDefinition.HeatFluxModel.Type)
            {
                case HeatFluxModelType.None:
                    return;
                case HeatFluxModelType.ExcessTemperature:
                case HeatFluxModelType.Composite:
                    ReadCompositeTemperatureData(extForceFileItems, modelDefinition, modelReferenceDate);
                    return;
            }
        }

        private void ReadCompositeTemperatureData(IEnumerable<ExtForceFileItem> extForceFileItems,
                                                  WaterFlowFMModelDefinition modelDefinition, DateTime modelReferenceDate)
        {
            HeatFluxModel heatFluxModel = modelDefinition.HeatFluxModel;

            ExtForceFileItem forceFileItem =
                extForceFileItems.LastOrDefault(e => e.Quantity == ExtForceQuantNames.MeteoData ||
                                                     e.Quantity == ExtForceQuantNames.MeteoDataWithRadiation);
            try
            {
                if (forceFileItem == null)
                {
                    return;
                }

                SupportedExtForceFileItems.Add(forceFileItem);

                heatFluxModel.ContainsSolarRadiation =
                    forceFileItem.Quantity == ExtForceQuantNames.MeteoDataWithRadiation;
                string extension = Path.GetExtension(forceFileItem.FileName);

                string filePath =
                    NGHSFileBase.GetOtherFilePathInSameDirectory(ExtSubFilesReferenceFilePath, forceFileItem.FileName);
                if (extension == FileConstants.TimFileExtension)
                {
                    new TimFile().Read(filePath, heatFluxModel.MeteoData, modelReferenceDate);
                    ExistingForceFileItems[forceFileItem] = heatFluxModel.MeteoData;
                }
                else if (extension == FileConstants.GriddedHeatFluxModelFileExtension)
                {
                    string gridFilePath = HeatFluxModel.GetCorrespondingGridFilePath(filePath);

                    if (File.Exists(gridFilePath))
                    {
                        heatFluxModel.GriddedHeatFluxFilePath = filePath;
                        heatFluxModel.GridFilePath = gridFilePath;

                        ExistingForceFileItems[forceFileItem] = heatFluxModel.Type;
                    }
                    else
                    {
                        throw new FileNotFoundException($"Could not find heat flux grid file {gridFilePath}");
                    }
                }
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is ArgumentNullException ||
                                       ex is InvalidOperationException || ex is ArgumentException ||
                                       ex is IOException || ex is FormatException)
            {
                heatFluxModel.Type = HeatFluxModelType.None;
                modelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueAsString("0");
                log.ErrorFormat(
                    "An error occured while reading Quantity {0} of file {1}: {2} Process temperature is reset to None ",
                    forceFileItem.Quantity, ExtFilePath, ex.Message);
            }
        }

        private void ReadSpatialData(IList<ExtForceFileItem> extForceFileItems,
                                     WaterFlowFMModelDefinition modelDefinition)
        {
            IList<ExtForceFileItem> unreadExtForceFileItems = extForceFileItems;

            var knownQuantities = new Dictionary<string, string>
            {
                {ExtForceQuantNames.InitialWaterLevel, WaterFlowFMModelDefinition.InitialWaterLevelDataItemName},
                {ExtForceQuantNames.InitialSalinity, WaterFlowFMModelDefinition.InitialSalinityDataItemName},
                {ExtForceQuantNames.InitialSalinityTop, WaterFlowFMModelDefinition.InitialSalinityDataItemName},
                {ExtForceQuantNames.InitialTemperature, WaterFlowFMModelDefinition.InitialTemperatureDataItemName},
                {ExtForceQuantNames.FrictCoef, WaterFlowFMModelDefinition.RoughnessDataItemName},
                {ExtForceQuantNames.HorEddyViscCoef, WaterFlowFMModelDefinition.ViscosityDataItemName},
                {ExtForceQuantNames.HorEddyDiffCoef, WaterFlowFMModelDefinition.DiffusivityDataItemName}
            };

            foreach (KeyValuePair<string, string> quantityPair in knownQuantities)
            {
                List<ExtForceFileItem> readItems =
                    unreadExtForceFileItems.Where(i => i.Quantity == quantityPair.Key).ToList();
                if (quantityPair.Key.Equals(ExtForceQuantNames.FrictCoef))
                {
                    readItems = FilterByFrictionType(unreadExtForceFileItems, modelDefinition).ToList();
                }

                ReadSpatialOperationData(readItems, modelDefinition, quantityPair.Key, quantityPair.Value);

                //Remove read items.
                unreadExtForceFileItems = unreadExtForceFileItems.Except(readItems).ToList();
                if (!unreadExtForceFileItems.Any())
                {
                    return;
                }
            }

            //Read tracer items.
            List<ExtForceFileItem> initialTracerItems = unreadExtForceFileItems
                                                        .Where(fi => fi.Quantity.StartsWith(
                                                                   ExtForceQuantNames.InitialTracerPrefix))
                                                        .ToList();
            foreach (ExtForceFileItem tracerItem in initialTracerItems)
            {
                string tracerName = tracerItem.Quantity.Substring(ExtForceQuantNames.InitialTracerPrefix.Length);
                ReadSpatialOperationData(initialTracerItems, modelDefinition, tracerItem.Quantity, tracerName);
            }

            unreadExtForceFileItems = unreadExtForceFileItems.Except(initialTracerItems).ToList();
            if (!unreadExtForceFileItems.Any())
            {
                return;
            }

            //Read sediment items.
            List<ExtForceFileItem> initialSedimentItems = unreadExtForceFileItems
                                                          .Where(fi => fi.Quantity.StartsWith(
                                                                     ExtForceQuantNames
                                                                         .InitialSpatialVaryingSedimentPrefix))
                                                          .ToList();
            foreach (ExtForceFileItem sedimentItem in initialSedimentItems)
            {
                /* DELFT3DFM-1112
                 * The only Spatially Varying Sediment that gets read from the ExtForces file is
                 * SedimentConcentration. We could simply remove its prefix, however, due to the 
                 * way it's meant to be written in said file, we need to add the postfix */
                string spatialvaryingSedConc =
                    sedimentItem.Quantity.Substring(ExtForceQuantNames.InitialSpatialVaryingSedimentPrefix.Length) + ExtForceFileConstants.SedConcPostfix;
                ReadSpatialOperationData(initialSedimentItems, modelDefinition, sedimentItem.Quantity,
                                         spatialvaryingSedConc);
            }
        }

        private IEnumerable<ExtForceFileItem> FilterByFrictionType(IEnumerable<ExtForceFileItem> extForceFileItems,
                                                                   WaterFlowFMModelDefinition modelDefinition)
        {
            WaterFlowFMProperty frictionTypeProperty =
                modelDefinition.Properties.FirstOrDefault(
                    p => p.PropertyDefinition.MduPropertyName == KnownProperties.FrictionType);

            int modelFrictionType = frictionTypeProperty != null
                                        ? GetIntegerPropertyValue(frictionTypeProperty.GetValueAsString())
                                        : 1;

            foreach (ExtForceFileItem extForceFileItem in extForceFileItems)
            {
                if (extForceFileItem.Quantity != ExtForceQuantNames.FrictCoef)
                {
                    continue;
                }

                int frictionType = modelFrictionType;

                if (extForceFileItem.ModelData.ContainsKey(ExtForceFileConstants.FricTypeKey))
                {
                    frictionType = GetIntegerPropertyValue(extForceFileItem.ModelData[ExtForceFileConstants.FricTypeKey].ToString());
                }

                if (frictionType != modelFrictionType)
                {
                    log.WarnFormat(
                        "Ignoring roughness operation with friction {0} type unequal to uniform model friction type {1}",
                        frictionType, modelFrictionType);
                }
                else
                {
                    yield return extForceFileItem;
                }
            }
        }

        private void ReadSpatialOperationData(IEnumerable<ExtForceFileItem> spatialForcingsItems,
                                              WaterFlowFMModelDefinition waterFlowFMModelDefinition, string quantity,
                                              string dataItemName)
        {
            List<ExtForceFileItem> forcingsItems = spatialForcingsItems.Where(i => i.Quantity == quantity).ToList();

            if (!forcingsItems.Any())
            {
                return;
            }

            IList<ISpatialOperation> spatialOperations = waterFlowFMModelDefinition.GetSpatialOperations(dataItemName);

            bool createOperationSet = spatialOperations == null;

            if (createOperationSet)
            {
                spatialOperations = new List<ISpatialOperation>();
                waterFlowFMModelDefinition.SpatialOperations[dataItemName] = spatialOperations;
            }

            spatialOperations.Clear();
            foreach (ExtForceFileItem extForceFileItem in forcingsItems)
            {
                SupportedExtForceFileItems.Add(extForceFileItem);
                ISpatialOperation spatialOperation = CreateSpatialOperation(extForceFileItem);
                if (spatialOperation != null)
                {
                    spatialOperations.Add(spatialOperation);
                }
            }
        }

        private ISpatialOperation CreateSpatialOperation(ExtForceFileItem extForceFileItem)
        {
            switch (extForceFileItem.FileType)
            {
                case 7:
                case 8:
                    return CreateSamplesOperation(extForceFileItem);
                case 10:
                    return CreatePolygonOperation(extForceFileItem);
                default:
                    throw new ArgumentException(
                        $"Cannot construct spatial operation for file {extForceFileItem.FileName} with file type {extForceFileItem.FileType}");
            }
        }

        private ISpatialOperation CreatePolygonOperation(ExtForceFileItem extForceFileItem)
        {
            string path = NGHSFileBase.GetOtherFilePathInSameDirectory(ExtSubFilesReferenceFilePath, extForceFileItem.FileName);

            IEnumerable<Feature> features = new PolFile<Feature2DPolygon>()
                                            .Read(path).Select(f => new Feature
                                            {
                                                Geometry = f.Geometry,
                                                Attributes = f.Attributes
                                            });

            string operationName = Path.GetFileNameWithoutExtension(extForceFileItem.FileName);

            var operation = new SetValueOperation
            {
                Value = extForceFileItem.Value,
                OperationType = ExtForceQuantNames.ParseOperationType(extForceFileItem.Operand),
                Name = operationName
            };
            operation.Mask.Provider = new FeatureCollection(features.ToList(), typeof(Feature));

            ExistingForceFileItems[extForceFileItem] = operation;

            return operation;
        }

        private ISpatialOperation CreateSamplesOperation(ExtForceFileItem extForceFileItem)
        {
            string operationName = Path.GetFileNameWithoutExtension(extForceFileItem.FileName);

            var operation = new ImportSamplesSpatialOperation
            {
                Name = operationName,
                FilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(ExtSubFilesReferenceFilePath, extForceFileItem.FileName)
            };

            if (extForceFileItem.ModelData.TryGetValue(ExtForceFileConstants.AveragingTypeKey, out object value))
            {
                operation.AveragingMethod = (GridCellAveragingMethod) value;
            }

            if (extForceFileItem.ModelData.TryGetValue(ExtForceFileConstants.RelSearchCellSizeKey, out value))
            {
                operation.RelativeSearchCellSize = (double) value;
            }

            switch (extForceFileItem.Method)
            {
                case 5:
                    operation.InterpolationMethod = SpatialInterpolationMethod.Triangulation;
                    break;
                case 6:
                    operation.InterpolationMethod = SpatialInterpolationMethod.Averaging;
                    break;
                default:
                    throw new Exception(
                        $"Invalid interpolation method {extForceFileItem.Method} for file {extForceFileItem.FileName}");
            }

            ExistingForceFileItems[extForceFileItem] = operation;

            return operation;
        }

        private void ReadWindItems(IEnumerable<ExtForceFileItem> extForceFileItems,
                                   WaterFlowFMModelDefinition modelDefinition)
        {
            var refDate = (DateTime) modelDefinition.GetModelProperty(KnownProperties.RefDate).Value;
            foreach (
                ExtForceFileItem extForceFileItem in
                extForceFileItems.Where(i => ExtForceQuantNames.WindQuantityNames.Values.Contains(i.Quantity)))
            {
                SupportedExtForceFileItems.Add(extForceFileItem);
                try
                {
                    IWindField windField = CreateWindField(extForceFileItem, ExtFilePath);

                    string windFile =
                        NGHSFileBase.GetOtherFilePathInSameDirectory(ExtSubFilesReferenceFilePath, extForceFileItem.FileName);

                    if (!File.Exists(windFile))
                    {
                        throw new FileNotFoundException($"Wind file {windFile} could not be found");
                    }

                    if (windField is UniformWindField)
                    {
                        var fileReader = new TimFile();
                        fileReader.Read(windFile, windField.Data, refDate);
                    }

                    modelDefinition.WindFields.Add(windField);
                    ExistingForceFileItems[extForceFileItem] = windField;
                }
                catch (Exception e)
                {
                    log.Warn(e.Message);
                }
            }
        }

        private static string GetKeyPart(string line)
        {
            return line.Substring(0, line.IndexOf("=")).Trim();
        }

        private int GetIntegerPropertyValue(string line)
        {
            return GetInt(GetValuePart(line), "integer value");
        }

        private double GetDoublePropertyValue(string line)
        {
            return GetDouble(GetValuePart(line), "double value");
        }

        private string GetValuePart(string line)
        {
            string valuePart;
            try
            {
                // Strip "key=" part away, if present:
                valuePart = line.Substring(line.IndexOf("=", StringComparison.Ordinal) + 1).Trim();
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new FormatException(
                    $"Expected '<key>=<value>(!/#)<comment>' formatted line on line {LineNumber} of file {OutputFilePath}");
            }

            // Determine comment starting index:
            int commentIndex1 = valuePart.IndexOf('!');
            int commentIndex2 = valuePart.IndexOf('#');
            int commentIndex = Math.Min(commentIndex1, commentIndex2);
            if (commentIndex < 0)
            {
                // not both characters present, may one of them
                commentIndex = Math.Max(commentIndex1, commentIndex2);
            }

            // Strip comment, if present:
            if (commentIndex1 > 0)
            {
                valuePart = valuePart.Substring(0, commentIndex).Trim();
            }

            return valuePart;
        }

        /// <summary>
        /// Parses a string for a double in <see cref="CultureInfo.InvariantCulture"/>.
        /// </summary>
        /// <param name="lineField">String representation of the number.</param>
        /// <param name="errorMessageKey">Optional: Additional description on value.</param>
        /// <returns>The value.</returns>
        /// <exception cref="FormatException">When <paramref name="lineField"/> does not represent a double.</exception>
        private double GetDouble(string lineField, string errorMessageKey = null)
        {
            if (!double.TryParse(lineField, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out double x))
            {
                throw new FormatException(
                    $"Invalid {(errorMessageKey != null ? errorMessageKey + " on " : "")} line {LineNumber} in file {InputFilePath}");
            }

            return x;
        }

        /// <summary>
        /// Parses a string for an int in <see cref="CultureInfo.InvariantCulture"/>.
        /// </summary>
        /// <param name="lineField">String representation of the number.</param>
        /// <param name="errorMessageKey">Optional: Additional description on value.</param>
        /// <returns>The value.</returns>
        /// <exception cref="FormatException">When <paramref name="lineField"/> does not represent an integer.</exception>
        private int GetInt(string lineField, string errorMessageKey = null)
        {
            if (int.TryParse(lineField, out int xAsInt))
            {
                return xAsInt;
            }

            if (double.TryParse(lineField, NumberStyles.Any, CultureInfo.InvariantCulture, out double xAsDouble))
            {
                double xAsDoubleFloored = Math.Floor(xAsDouble);
                if (xAsDouble - xAsDoubleFloored < 1e-12)
                {
                    // valid int, (accidentally) written as double
                    return (int) xAsDoubleFloored;
                }
            }

            throw new FormatException(
                $"Invalid {(errorMessageKey != null ? errorMessageKey + " on " : "")} line {LineNumber} in file {InputFilePath}");
        }
    }
}