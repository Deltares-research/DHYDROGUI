using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public partial class ExtForceFile
    {
        private string extSubFilesReferenceFilePath;

        public void Read(string extForceFilePath, WaterFlowFMModelDefinition modelDefinition,
                         string extForceSubFilesReferenceFilePath)
        {
            extFilePath = extForceFilePath;
            extSubFilesReferenceFilePath = extForceSubFilesReferenceFilePath;

            Read(modelDefinition);
        }

        protected override void CreateCommonBlock()
        {
            if (CurrentLine.ToUpper().StartsWith(extForcesFileQuantityBlockStarter))
            {
                LineNumber++;
                storedNextInputLine = reader.ReadLine();
                if (storedNextInputLine != null)
                {
                    string contentIdentifier =
                        CreateContentIdentifier(CurrentLine.Trim() + storedNextInputLine.Trim());
                    commentBlocks.Add(contentIdentifier, currentCommentBlock);
                }
            }
            else
            {
                // can not handle internal comments
                currentCommentBlock = null;
            }
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

        private IEnumerable<ExtForceFileItem> GetUnknownExtForceFileItems(IEnumerable<ExtForceFileItem> allExtForceFileItems) =>
            allExtForceFileItems.Except(supportedExtForceFileItems);

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
                    GetOtherFilePathInSameDirectory(extSubFilesReferenceFilePath, unknownForceFileItem.FileName);

                var unsupportedFileBasedExtForceFileItem =
                    new UnsupportedFileBasedExtForceFileItem(referencedFilePath, unknownForceFileItem);

                modelDefinition.UnsupportedFileBasedExtForceFileItems.Add(unsupportedFileBasedExtForceFileItem);
            }
        }

        private IEnumerable<ExtForceFileItem> ParseExtForceFile()
        {
            OpenInputFile(extFilePath);

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
                            $"Invalid Quantity item '{extForceFileItem.Quantity}' starting on line {startLineNumber} in file {extFilePath}; Item is skipped.");
                    }
                }
            }
            finally
            {
                CloseInputFile();
            }
        }

        private static bool IsNewEntry(string line)
        {
            string lineToLower = line.ToLower();
            IEnumerable<string> keysToCheck = new[]
            {
                ExtForceFileConstants.QuantityKey,
                disabledQuantityKey
            }.Concat(unsupportedQuantityKeys).Select(s => s.ToLower());

            return keysToCheck.Any(lineToLower.StartsWith);
        }

        private ExtForceFileItem ReadQuantityBlock(int startLineNumber)
        {
            string propertyName = GetKeyPart(currentLine);
            var extForceFileItem = new ExtForceFileItem(GetValuePart(currentLine));

            if (propertyName.ToUpper() != ExtForceFileConstants.QuantityKey)
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

            switch (propertyName.ToUpper())
            {
                case ExtForceFileConstants.FileNameKey:
                    SetFileName(extForceFileItem);
                    break;
                case ExtForceFileConstants.VarNameKey:
                    SetVarName(extForceFileItem);
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
                case valueKey:
                    SetValue(extForceFileItem);
                    break;
                case factorKey:
                    SetFactor(extForceFileItem);
                    break;
                case offsetKey:
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
                case frictionTypeKey:
                    SetFrictionType(extForceFileItem);
                    break;
                case extrapoltolKey:
                    SetExtraPolTol(extForceFileItem);
                    break;
                default:
                    log.WarnFormat(
                        Resources
                            .ExtForceFile_ReadQuantityProperty_Unexpected_line___0___on_line__1__in_file__2__and_will_be_ignored_,
                        currentLine, LineNumber, extFilePath);
                    break;
            }
        }

        private void ReadPolyLineData(IEnumerable<ExtForceFileItem> extForceFileItems,
                                      WaterFlowFMModelDefinition modelDefinition)
        {
            IEventedList<BoundaryConditionSet> boundaryConditionSets = modelDefinition.BoundaryConditionSets;
            var boundaryConditions = new List<IBoundaryCondition>();
            var sourcesAndSinks = new List<SourceAndSink>();

            var modelReferenceDate = modelDefinition.GetReferenceDateAsDateTime();

            foreach (ExtForceFileItem extForceFileItem in GetSupportedFileItems(extForceFileItems))
            {
                ValidateFileType(extForceFileItem);

                // check what type of polyLine to read
                bool isSourceAndSink = Equals(extForceFileItem.Quantity, ExtForceQuantNames.SourceAndSink);

                if (!ExtForceQuantNames.TryParseBoundaryQuantityType(extForceFileItem.Quantity, out FlowBoundaryQuantityType _) && !isSourceAndSink)
                {
                    continue;
                }

                supportedExtForceFileItems.Add(extForceFileItem);

                // read the pli file
                string pliFilePath = GetOtherFilePathInSameDirectory(extSubFilesReferenceFilePath, extForceFileItem.FileName);
                IList<Feature2D> features2D = ReadFeatureFile(isSourceAndSink, pliFilePath);
                ExistingForceFileItems[extForceFileItem] = features2D;

                // go through all feature2Ds
                foreach (Feature2D feature2D in features2D)
                {
                    if (isSourceAndSink)
                    {
                        SourceAndSink sourceAndSink = ReadSourceAndSink(pliFilePath, feature2D, extForceFileItem, modelReferenceDate);

                        if (sourceAndSink == null)
                        {
                            continue;
                        }

                        PolyLineForceFileItems[sourceAndSink] = extForceFileItem;
                        sourcesAndSinks.Add(sourceAndSink);
                    }
                    else
                    {
                        BoundaryCondition boundaryCondition = ReadBoundaryCondition(boundaryConditionSets, feature2D, pliFilePath, extForceFileItem, modelReferenceDate);

                        if (boundaryCondition == null)
                        {
                            continue;
                        }

                        PolyLineForceFileItems[boundaryCondition] = extForceFileItem;
                        boundaryConditions.Add(boundaryCondition);
                    }
                }
            }

            foreach (BoundaryConditionSet boundaryConditionSet in boundaryConditionSets)
            {
                var feature = boundaryConditionSet.Feature as IFeature;
                boundaryConditionSet.BoundaryConditions = new EventedList<IBoundaryCondition>();
                boundaryConditionSet.BoundaryConditions.AddRange(boundaryConditions.Where(bc => Equals(bc.Feature, feature)));
            }

            NamingHelper.MakeNamesUnique(boundaryConditionSets.Select(bd => bd.Feature).Cast<INameable>().ToList());
            NamingHelper.MakeNamesUnique(sourcesAndSinks.Select(bd => bd.Feature).Cast<INameable>().ToList());

            modelDefinition.Boundaries.AddRange(boundaryConditionSets.Select(bd => bd.Feature));
            modelDefinition.SourcesAndSinks.AddRange(sourcesAndSinks);
            modelDefinition.Pipes.AddRange(sourcesAndSinks.Select(ss => ss.Feature).Distinct());
        }

        private static IEnumerable<ExtForceFileItem> GetSupportedFileItems(IEnumerable<ExtForceFileItem> extForceFileItems)
        {
            return extForceFileItems.Where(
                e =>
                {
                    string extension = Path.GetExtension(e.FileName);
                    return extension == FileConstants.PliFileExtension
                           || extension == FileConstants.PlizFileExtension;
                });
        }

        private BoundaryCondition ReadBoundaryCondition(ICollection<BoundaryConditionSet> boundaryConditionSets, Feature2D feature2D,
                                                        string filePath, ExtForceFileItem extForceFileItem, DateTime modelReferenceDate)
        {
            Feature2D uniqueFeature = boundaryConditionSets.Select(bcs => bcs.Feature)
                                                           .FirstOrDefault(f => f.Geometry.EqualsTopologically(feature2D.Geometry)) ?? feature2D;

            if (uniqueFeature == feature2D)
            {
                boundaryConditionSets.Add(new BoundaryConditionSet {Feature = feature2D});
            }

            BoundaryCondition boundaryCondition;

            try
            {
                boundaryCondition = ReadBoundaryConditionData(filePath,
                                                              uniqueFeature,
                                                              extForceFileItem,
                                                              modelReferenceDate);
            }
            catch (Exception e)
            {
                if (e is ArgumentException || e is PathTooLongException || e is FormatException ||
                    e is FileNotFoundException || e is IOException || e is OutOfMemoryException)
                {
                    throw new InvalidOperationException($"An error (Message: {e.Message}) occured while reading boundary condition data for " +
                                                        $"feature {feature2D.Name} in file {extFilePath}", e);
                }

                throw;
            }

            return boundaryCondition;
        }

        private SourceAndSink ReadSourceAndSink(string filePath, Feature2D feature2D, ExtForceFileItem extForceFileItem, DateTime modelReferenceDate)
        {
            SourceAndSink sourceAndSink;
            try
            {
                sourceAndSink = ReadSourceAndSinkData(filePath, feature2D, extForceFileItem, modelReferenceDate);
            }
            catch (Exception e)
            {
                if (e is ArgumentException || e is PathTooLongException || e is FormatException ||
                    e is FileNotFoundException || e is IOException || e is OutOfMemoryException)
                {
                    throw new InvalidOperationException($"An error (Message: {e.Message}) occured while source/sink data for " +
                                                        $"feature {feature2D.Name} in file {extFilePath}", e);
                }

                throw;
            }

            return sourceAndSink;
        }

        private static void ValidateFileType(ExtForceFileItem extForceFileItem)
        {
            if (extForceFileItem.FileType != ExtForceQuantNames.FileTypes.PolyTim)
            {
                throw new NotSupportedException("The provided pli file is not a PolyTim file. " +
                                                extForceFileItem.FileName);
            }
        }

        private static IList<Feature2D> ReadFeatureFile(bool isSourceAndSink, string pliFilePath)
        {
            string extension = Path.GetExtension(pliFilePath);

            PliFile<Feature2D> fileReader;
            switch (extension)
            {
                case FileConstants.PliFileExtension:
                    fileReader = new PliFile<Feature2D>();
                    break;
                case FileConstants.PlizFileExtension:
                    fileReader = new PlizFile<Feature2D>();
                    break;
                default:
                    throw new NotSupportedException($"Unsupported file extension ({extension}) encountered.");
            }

            return ReadPliFile(isSourceAndSink, pliFilePath, fileReader);
        }

        private static IList<Feature2D> ReadPliFile(bool isSourceAndSink, string pliFilePath, PliFile<Feature2D> fileReader)
        {
            if (isSourceAndSink)
            {
                fileReader.CreateDelegate = CreateFeature2D;
            }

            IList<Feature2D> features2D = fileReader.Read(pliFilePath);
            return features2D;
        }

        private static Feature2D CreateFeature2D(List<Coordinate> points, string name) =>
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

        private void ReadHeatFluxModelData(IEnumerable<ExtForceFileItem> extForceFileItems,
                                           WaterFlowFMModelDefinition modelDefinition)
        {
            var modelReferenceDate = modelDefinition.GetReferenceDateAsDateTime();
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

            ExtForceFileItem forceFileItem = extForceFileItems.LastOrDefault(e => e.Quantity == ExtForceQuantNames.MeteoData ||
                                                                                  e.Quantity == ExtForceQuantNames.MeteoDataWithRadiation);

            // File types supported (E.1.8 - time series for the heat model parameters):
            // - curvilinear time series (6) (see manual C.12.2)
            // - uniform time series (1) (see manual C.4) 
            const int curvilinear_data = 6;  
            const int timeseries = 1;
            try
            {
                if (forceFileItem == null)
                {
                    return;
                }

                supportedExtForceFileItems.Add(forceFileItem);

                heatFluxModel.ContainsSolarRadiation =
                    forceFileItem.Quantity == ExtForceQuantNames.MeteoDataWithRadiation;

                string filePath = GetOtherFilePathInSameDirectory(extSubFilesReferenceFilePath, forceFileItem.FileName);
                if ( forceFileItem.FileType == timeseries)
                {
                    new TimFile().Read(filePath, heatFluxModel.MeteoData, modelReferenceDate);
                    ExistingForceFileItems[forceFileItem] = heatFluxModel.MeteoData;
                }
                else if (forceFileItem.FileType == curvilinear_data)
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
                    forceFileItem.Quantity, extFilePath, ex.Message);
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
                List<ExtForceFileItem> readItems = unreadExtForceFileItems.Where(i => i.Quantity == quantityPair.Key).ToList();
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
                                                          .Where(fi => fi.Quantity.StartsWith(ExtForceQuantNames.InitialSpatialVaryingSedimentPrefix))
                                                          .ToList();
            foreach (ExtForceFileItem sedimentItem in initialSedimentItems)
            {
                /* DELFT3DFM-1112
                 * The only Spatially Varying Sediment that gets read from the ExtForces file is
                 * SedimentConcentration. We could simply remove its prefix, however, due to the 
                 * way it's meant to be written in said file, we need to add the postfix */
                string spatialVaryingSedimentConcentration =
                    sedimentItem.Quantity.Substring(ExtForceQuantNames.InitialSpatialVaryingSedimentPrefix.Length) +
                    ExtForceFileConstants.SedimentConcentrationPostfix;
                ReadSpatialOperationData(initialSedimentItems, modelDefinition, sedimentItem.Quantity,
                                         spatialVaryingSedimentConcentration);
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
                supportedExtForceFileItems.Add(extForceFileItem);
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
                    throw new ArgumentException($"Cannot construct spatial operation for file {extForceFileItem.FileName} with file type {extForceFileItem.FileType}");
            }
        }

        private ISpatialOperation CreatePolygonOperation(ExtForceFileItem extForceFileItem)
        {
            string path = GetOtherFilePathInSameDirectory(extSubFilesReferenceFilePath, extForceFileItem.FileName);

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
                FilePath = GetOtherFilePathInSameDirectory(extSubFilesReferenceFilePath, extForceFileItem.FileName)
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
            var refDate = modelDefinition.GetReferenceDateAsDateTime();
            foreach (
                ExtForceFileItem extForceFileItem in
                extForceFileItems.Where(i => ExtForceQuantNames.WindQuantityNames.Values.Contains(i.Quantity)))
            {
                supportedExtForceFileItems.Add(extForceFileItem);
                try
                {
                    IWindField windField = ExtForceFileHelper.CreateWindField(extForceFileItem, extFilePath);

                    string windFile =
                        GetOtherFilePathInSameDirectory(extSubFilesReferenceFilePath, extForceFileItem.FileName);

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

        private BoundaryCondition ReadBoundaryConditionData(string filePath, Feature2D feature2D,
                                                            ExtForceFileItem extForceFileItem,
                                                            DateTime modelReferenceDate)
        {
            FlowBoundaryQuantityType quantityType;
            if (ExtForceQuantNames.TryParseBoundaryQuantityType(extForceFileItem.Quantity, out quantityType))
            {
                IList<int> dataFileNumbers;
                BoundaryConditionDataType dataType;
                string fileExtension;

                if (TryDetermineForcingType(feature2D, filePath, out dataType,
                                            out dataFileNumbers,
                                            out fileExtension))
                {
                    // create a quantity name from the type and the tracer name if it is set to tracer.
                    var quantityName = quantityType.ToString();
                    if (quantityType == FlowBoundaryQuantityType.Tracer)
                    {
                        quantityName += "_" + extForceFileItem.Quantity.Substring(9); // remove tracerbnd
                    }

                    var boundaryCondition =
                        (FlowBoundaryCondition) new FlowBoundaryConditionFactory().CreateBoundaryCondition(
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
            object area;
            extForceFileItem.ModelData.TryGetValue(ExtForceFileConstants.AreaKey, out area);
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
                        VerticalProfileDefinition verticalProfileDefinition;
                        TimeSeries series = new T3DFile().Read(filePath, out verticalProfileDefinition);
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

        private IEnumerable<ExtForceFileItem> FilterByFrictionType(IEnumerable<ExtForceFileItem> extForceFileItems,
                                                                   WaterFlowFMModelDefinition modelDefinition)
        {
            WaterFlowFMProperty frictionTypeProperty = modelDefinition.Properties.FirstOrDefault(p => p.PropertyDefinition.MduPropertyName == KnownProperties.FrictionType);

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

                if (extForceFileItem.ModelData.ContainsKey(frictionTypeKey))
                {
                    frictionType = GetIntegerPropertyValue(extForceFileItem.ModelData[frictionTypeKey].ToString());
                }

                if (frictionType != modelFrictionType)
                {
                    log.WarnFormat("Ignoring roughness operation with friction {0} type unequal to uniform model friction type {1}",
                                   frictionType, modelFrictionType);
                }
                else
                {
                    yield return extForceFileItem;
                }
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
        
        private void SetVarName(ExtForceFileItem extForceFileItem)
        {
            if (string.IsNullOrEmpty(extForceFileItem.VarName))
            {
                extForceFileItem.VarName = GetValuePart(currentLine);
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
                LogWarningQuantityPropertyAlreadySet(valueKey);
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
                LogWarningQuantityPropertyAlreadySet(factorKey);
            }
        }

        private void SetOffset(ExtForceFileItem extForceFileItem)
        {
            if (extForceFileItem.Operand == null)
            {
                throw new FormatException(GetMessageUnexpectedKeyword(offsetKey));
            }

            if (double.IsNaN(extForceFileItem.Offset))
            {
                extForceFileItem.Offset = GetDouble(GetValuePart(currentLine));
            }
            else
            {
                LogWarningQuantityPropertyAlreadySet(offsetKey);
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
            if (extForceFileItem.ModelData.ContainsKey(frictionTypeKey))
            {
                LogWarningQuantityPropertyAlreadySet(frictionTypeKey);
            }

            extForceFileItem.ModelData[frictionTypeKey] = GetIntegerPropertyValue(currentLine);
        }
        
        private void SetExtraPolTol(ExtForceFileItem extForceFileItem)
        {
            if (double.IsNaN(extForceFileItem.ExtraPolTol))
            {
                extForceFileItem.ExtraPolTol = GetDouble(GetValuePart(currentLine));
            }
            else
            {
                LogWarningQuantityPropertyAlreadySet(extrapoltolKey);
            }
        }

        private void LogWarningQuantityPropertyAlreadySet(string quantityName)
        {
            log.WarnFormat(
                Resources
                    .ExtForceFile_LogWarningQuantityPropertyAlreadySet__0__is_already_set__Line__1__of_file__2__will_be_ignored_,
                quantityName, LineNumber, extFilePath);
        }

        private string GetMessageUnexpectedKeyword(string quantityName) =>
            string.Format(
                Resources.ExtForceFile_GetMessageUnexpectedKeyword_Unexpected_keyword__0__on_line__1__of_file__2_,
                quantityName, LineNumber, extFilePath);

        private static bool IsValidQuantity(ExtForceFileItem extForceFileItem) =>
            !(string.IsNullOrEmpty(extForceFileItem?.FileName)
              || extForceFileItem.FileType == int.MinValue
              || extForceFileItem.Method == int.MinValue
              || extForceFileItem.Operand == null
              || !extForceFileItem.Enabled);

        private static string GetKeyPart(string line) => line.Substring(0, line.IndexOf("=")).Trim();

        private double GetDoublePropertyValue(string line) => GetDouble(GetValuePart(line), "double value");

        private int GetIntegerPropertyValue(string line) => GetInt(GetValuePart(line), "integer value");

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
                throw new FormatException($"Expected '<key>=<value>(!/#)<comment>' formatted line on line {LineNumber} of file {OutputFilePath}");
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
    }
}