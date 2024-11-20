using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using Deltares.Infrastructure.Extensions;
using Deltares.Infrastructure.API.Validation;
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
using DHYDRO.Common.IO.ExtForce;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using SharpMap;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public partial class ExtForceFile
    {
        public void Read(string filePath, string referenceFilePath, WaterFlowFMModelDefinition definition)
        {
            extFilePath = filePath;
            extSubFilesReferenceFilePath = referenceFilePath;
            modelDefinition = definition;
            
            Read();
        }

        private void Read()
        {
            ExtForceFileData extForceFileData = ParseExtForceFile();
            IList<ExtForceData> forceFileItems = extForceFileData.Forcings.ToList();

            RemoveInvalidForcings(forceFileItems);
            ReadPolyLineData(forceFileItems);
            ReadWindItems(forceFileItems);
            ReadHeatFluxModelData(forceFileItems);
            ReadSpatialData(forceFileItems);
            ReadInitialVelocityData(forceFileItems);
            StoreUnknownQuantities(forceFileItems);
        }

        private void RemoveInvalidForcings(ICollection<ExtForceData> forcings)
        {
            var validator = new ExtForceDataValidator(fileSystem);
            var parentDirectory = Path.GetDirectoryName(extSubFilesReferenceFilePath);
            
            foreach (ExtForceData extForceData in forcings.ToArray())
            {
                extForceData.ParentDirectory = parentDirectory;
                
                ValidationResult result = validator.Validate(extForceData);
                if (!result.Valid)
                {
                    log.Error(result.Message);
                    forcings.Remove(extForceData);
                }
                else if (!extForceData.IsEnabled)
                {
                    forcings.Remove(extForceData);
                }
            }
        }

        private void ReadInitialVelocityData(IEnumerable<ExtForceData> forceFileItems)
        {
            foreach (ExtForceData forceFileItem in forceFileItems)
            {
                if (forceFileItem.Quantity.EqualsCaseInsensitive(ExtForceQuantNames.initialVelocityXQuantity))
                {
                    ReadSamples(forceFileItem, modelDefinition.InitialVelocityX);
                }

                if (forceFileItem.Quantity.EqualsCaseInsensitive(ExtForceQuantNames.initialVelocityYQuantity))
                {
                    ReadSamples(forceFileItem, modelDefinition.InitialVelocityY);
                }
            }
        }

        private void ReadSamples(ExtForceData forceFileItem, Samples samples)
        {
            string path = GetOtherFilePathInSameDirectory(extSubFilesReferenceFilePath, forceFileItem.FileName);
            IList<IPointValue> pointValues = ReadPointValues(path);
            
            samples.SetPointValues(pointValues);
            samples.Operand = ExtForceQuantNames.ParseOperationType(forceFileItem.Operand);
            samples.InterpolationMethod = GetInterpolationMethod(forceFileItem);
            samples.SourceFileName = forceFileItem.FileName;
            
            if (forceFileItem.TryGetModelData(ExtForceFileConstants.Keys.AveragingType, out int averagingMethod))
            {
                samples.AveragingMethod = (GridCellAveragingMethod) averagingMethod;
            }
            if (forceFileItem.TryGetModelData(ExtForceFileConstants.Keys.RelativeSearchCellSize, out double relativeSearchCellSize))
            {
                samples.RelativeSearchCellSize = relativeSearchCellSize;
            }
            if (forceFileItem.TryGetModelData(ExtForceFileConstants.Keys.ExtrapolationTolerance, out double extrapolationTolerance))
            {
                samples.ExtrapolationTolerance = extrapolationTolerance;
            }

            AddSamplesToForcingFileItems(forceFileItem, samples);
        }

        private static IList<IPointValue> ReadPointValues(string path) => XyzFile.Read(path, checkForUnsupportedSize: true);

        private void AddSamplesToForcingFileItems(ExtForceData forceFileItem, Samples samples)
        {
            supportedExtForceFileItems.Add(forceFileItem);
            ExistingForceFileItems[forceFileItem] = samples;
        }

        private IEnumerable<ExtForceData> GetUnknownExtForceFileItems(IEnumerable<ExtForceData> allExtForceFileItems) =>
            allExtForceFileItems.Except(supportedExtForceFileItems);

        private void StoreUnknownQuantities(IEnumerable<ExtForceData> allExtForceFileItems)
        {
            List<ExtForceData> unknownForceFileItems = GetUnknownExtForceFileItems(allExtForceFileItems).ToList();
            foreach (ExtForceData unknownForceFileItem in unknownForceFileItems)
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

        private ExtForceFileData ParseExtForceFile()
        {
            log.InfoFormat(Resources.Reading_external_forcings_file_from_0_, extFilePath);
            
            using (FileSystemStream stream = fileSystem.File.OpenRead(extFilePath))
            {
                var parser = new ExtForceFileParser();
                return parser.Parse(stream);
            }
        }

        private void ReadPolyLineData(IEnumerable<ExtForceData> extForceFileItems)
        {
            IEventedList<BoundaryConditionSet> boundaryConditionSets = modelDefinition.BoundaryConditionSets;
            var boundaryConditions = new List<IBoundaryCondition>();
            var sourcesAndSinks = new List<SourceAndSink>();

            var modelReferenceDate = modelDefinition.GetReferenceDateAsDateTime();

            foreach (ExtForceData extForceFileItem in GetSupportedFileItems(extForceFileItems))
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

        private static IEnumerable<ExtForceData> GetSupportedFileItems(IEnumerable<ExtForceData> extForceFileItems)
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
                                                        string filePath, ExtForceData extForceFileItem, DateTime modelReferenceDate)
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

        private SourceAndSink ReadSourceAndSink(string filePath, Feature2D feature2D, ExtForceData extForceFileItem, DateTime modelReferenceDate)
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

        private static void ValidateFileType(ExtForceData extForceFileItem)
        {
            if (extForceFileItem.FileType != ExtForceFileConstants.FileTypes.PolyTim)
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

        private void ReadHeatFluxModelData(IEnumerable<ExtForceData> extForceFileItems)
        {
            var modelReferenceDate = modelDefinition.GetReferenceDateAsDateTime();
            switch (modelDefinition.HeatFluxModel.Type)
            {
                case HeatFluxModelType.None:
                    return;
                case HeatFluxModelType.ExcessTemperature:
                case HeatFluxModelType.Composite:
                    ReadCompositeTemperatureData(extForceFileItems, modelReferenceDate);
                    return;
            }
        }

        private void ReadCompositeTemperatureData(IEnumerable<ExtForceData> extForceFileItems, DateTime modelReferenceDate)
        {
            HeatFluxModel heatFluxModel = modelDefinition.HeatFluxModel;

            ExtForceData forceFileItem = extForceFileItems.LastOrDefault(e => e.Quantity == ExtForceQuantNames.MeteoData ||
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
                modelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueFromString("0");
                log.ErrorFormat(
                    "An error occured while reading Quantity {0} of file {1}: {2} Process temperature is reset to None ",
                    forceFileItem.Quantity, extFilePath, ex.Message);
            }
        }

        private void ReadSpatialData(IList<ExtForceData> extForceFileItems)
        {
            IList<ExtForceData> unreadExtForceFileItems = extForceFileItems;

            var knownQuantities = new Dictionary<string, string>
            {
                {ExtForceQuantNames.InitialWaterLevel, WaterFlowFMModelDefinition.InitialWaterLevelDataItemName},
                {ExtForceQuantNames.InitialSalinity, WaterFlowFMModelDefinition.InitialSalinityDataItemName},
                {ExtForceQuantNames.InitialTemperature, WaterFlowFMModelDefinition.InitialTemperatureDataItemName},
                {ExtForceQuantNames.FrictCoef, WaterFlowFMModelDefinition.RoughnessDataItemName},
                {ExtForceQuantNames.HorEddyViscCoef, WaterFlowFMModelDefinition.ViscosityDataItemName},
                {ExtForceQuantNames.HorEddyDiffCoef, WaterFlowFMModelDefinition.DiffusivityDataItemName}
            };

            foreach (KeyValuePair<string, string> quantityPair in knownQuantities)
            {
                List<ExtForceData> readItems = unreadExtForceFileItems.Where(i => i.Quantity == quantityPair.Key).ToList();
                foreach (var readItem in readItems)
                {
                    supportedExtForceFileItems.Add(readItem);
                }
                if (quantityPair.Key.Equals(ExtForceQuantNames.FrictCoef))
                {
                    readItems = FilterByFrictionType(unreadExtForceFileItems).ToList();
                }

                ReadSpatialOperationData(readItems, quantityPair.Key, quantityPair.Value);

                //Remove read items.
                unreadExtForceFileItems = unreadExtForceFileItems.Except(readItems).ToList();
                if (!unreadExtForceFileItems.Any())
                {
                    return;
                }
            }

            //Read tracer items.
            List<ExtForceData> initialTracerItems = unreadExtForceFileItems
                                                        .Where(fi => fi.Quantity.StartsWith(
                                                                   ExtForceQuantNames.InitialTracerPrefix))
                                                        .ToList();
            foreach (ExtForceData tracerItem in initialTracerItems)
            {
                string tracerName = tracerItem.Quantity.Substring(ExtForceQuantNames.InitialTracerPrefix.Length);
                ReadSpatialOperationData(initialTracerItems, tracerItem.Quantity, tracerName);
            }

            unreadExtForceFileItems = unreadExtForceFileItems.Except(initialTracerItems).ToList();
            if (!unreadExtForceFileItems.Any())
            {
                return;
            }

            //Read sediment items.
            List<ExtForceData> initialSedimentItems = unreadExtForceFileItems
                                                          .Where(fi => fi.Quantity.StartsWith(ExtForceQuantNames.InitialSpatialVaryingSedimentPrefix))
                                                          .ToList();
            foreach (ExtForceData sedimentItem in initialSedimentItems)
            {
                /* DELFT3DFM-1112
                 * The only Spatially Varying Sediment that gets read from the ExtForces file is
                 * SedimentConcentration. We could simply remove its prefix, however, due to the 
                 * way it's meant to be written in said file, we need to add the postfix */
                string spatialVaryingSedimentConcentration =
                    sedimentItem.Quantity.Substring(ExtForceQuantNames.InitialSpatialVaryingSedimentPrefix.Length) +
                    ExtForceQuantNames.SedimentConcentrationPostfix;
                ReadSpatialOperationData(initialSedimentItems, sedimentItem.Quantity, spatialVaryingSedimentConcentration);
            }
        }

        private void ReadSpatialOperationData(IEnumerable<ExtForceData> spatialForcingsItems, string quantity, string dataItemName)
        {
            List<ExtForceData> forcingsItems = spatialForcingsItems.Where(i => i.Quantity == quantity).ToList();

            if (!forcingsItems.Any())
            {
                return;
            }

            IList<ISpatialOperation> spatialOperations = modelDefinition.GetSpatialOperations(dataItemName);

            bool createOperationSet = spatialOperations == null;

            if (createOperationSet)
            {
                spatialOperations = new List<ISpatialOperation>();
                modelDefinition.SpatialOperations[dataItemName] = spatialOperations;
            }

            spatialOperations.Clear();
            foreach (ExtForceData extForceFileItem in forcingsItems)
            {
                supportedExtForceFileItems.Add(extForceFileItem);
                ISpatialOperation spatialOperation = CreateSpatialOperation(extForceFileItem);
                if (spatialOperation != null)
                {
                    spatialOperations.Add(spatialOperation);
                }
            }
        }

        private ISpatialOperation CreateSpatialOperation(ExtForceData extForceFileItem)
        {
            switch (extForceFileItem.FileType)
            {
                case ExtForceFileConstants.FileTypes.Triangulation:
                case ExtForceFileConstants.FileTypes.TriangulationMagDir:
                    return CreateSamplesOperation(extForceFileItem);
                case ExtForceFileConstants.FileTypes.InsidePolygon:
                    return CreatePolygonOperation(extForceFileItem);
                default:
                    throw new ArgumentException($"Cannot construct spatial operation for file {extForceFileItem.FileName} with file type {extForceFileItem.FileType}");
            }
        }

        private ISpatialOperation CreatePolygonOperation(ExtForceData extForceFileItem)
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
                Value = extForceFileItem.Value ?? double.NaN,
                OperationType = ExtForceQuantNames.ParseOperationType(extForceFileItem.Operand),
                Name = operationName,
                Mask = { Provider = new FeatureCollection(features.ToList(), typeof(Feature)) }
            };

            ExistingForceFileItems[extForceFileItem] = operation;

            return operation;
        }

        private ISpatialOperation CreateSamplesOperation(ExtForceData extForceFileItem)
        {
            string operationName = Path.GetFileNameWithoutExtension(extForceFileItem.FileName);

            var operation = new ImportSamplesSpatialOperation
            {
                Name = operationName,
                FilePath = GetOtherFilePathInSameDirectory(extSubFilesReferenceFilePath, extForceFileItem.FileName)
            };

            if (extForceFileItem.TryGetModelData(ExtForceFileConstants.Keys.AveragingType, out int averagingType))
            {
                operation.AveragingMethod = (GridCellAveragingMethod) averagingType;
            }

            if (extForceFileItem.TryGetModelData(ExtForceFileConstants.Keys.RelativeSearchCellSize, out double relativeSearchCellSize))
            {
                operation.RelativeSearchCellSize = relativeSearchCellSize;
            }
            operation.InterpolationMethod = GetInterpolationMethod(extForceFileItem);

            ExistingForceFileItems[extForceFileItem] = operation;

            return operation;
        }

        private static SpatialInterpolationMethod GetInterpolationMethod(ExtForceData extForceFileItem)
        {
            switch (extForceFileItem.Method)
            {
                case ExtForceFileConstants.Methods.Triangulation:
                    return SpatialInterpolationMethod.Triangulation;
                case ExtForceFileConstants.Methods.Averaging:
                    return SpatialInterpolationMethod.Averaging;
                default:
                    throw new Exception(
                        $"Invalid interpolation method {extForceFileItem.Method} for file {extForceFileItem.FileName}");
            }
        }

        private void ReadWindItems(IEnumerable<ExtForceData> extForceFileItems)
        {
            var refDate = modelDefinition.GetReferenceDateAsDateTime();
            foreach (
                ExtForceData extForceFileItem in
                extForceFileItems.Where(i => ExtForceQuantNames.WindQuantityNames.Values.Contains(i.Quantity)))
            {
                supportedExtForceFileItems.Add(extForceFileItem);
                try
                {
                    string windFile = GetOtherFilePathInSameDirectory(extSubFilesReferenceFilePath, extForceFileItem.FileName);
                    
                    IWindField windField = ExtForceFileHelper.CreateWindField(extForceFileItem, windFile);

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
                                                            ExtForceData extForceFileItem,
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

                    if (extForceFileItem.Offset != null)
                    {
                        boundaryCondition.Offset = extForceFileItem.Offset.Value;
                    }

                    if (extForceFileItem.Factor != null)
                    {
                        boundaryCondition.Factor = extForceFileItem.Factor.Value;
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
                                                    ExtForceData extForceFileItem,
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
            
            if (extForceFileItem.TryGetModelData(ExtForceFileConstants.Keys.Area, out double area))
            {
                sourceAndSink.Area = area;
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

        private IEnumerable<ExtForceData> FilterByFrictionType(IEnumerable<ExtForceData> extForceFileItems)
        {
            WaterFlowFMProperty frictionTypeProperty = modelDefinition.Properties.FirstOrDefault(p => p.PropertyDefinition.MduPropertyName == KnownProperties.FrictionType);

            int modelFrictionType = frictionTypeProperty != null
                                        ? int.Parse(frictionTypeProperty.GetValueAsString())
                                        : 1;

            foreach (ExtForceData extForceFileItem in extForceFileItems)
            {
                if (extForceFileItem.Quantity != ExtForceQuantNames.FrictCoef)
                {
                    continue;
                }

                int frictionType = modelFrictionType;

                if (extForceFileItem.TryGetModelData(ExtForceFileConstants.Keys.FrictionType, out int forcingFrictionType))
                {
                    frictionType = forcingFrictionType;
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