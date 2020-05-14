using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
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
    //TODO: this has become a complete mess (The helper class too). Refactor.
    public class ExtForceFile : FMSuiteFileBase
    {
        // Known file extensions

        // keywords in file used for modelDefinition specific data
        public const string FricTypeKey = "IFRCTYP";
        public const string AreaKey = "AREA";
        public const string AveragingTypeKey = "AVERAGINGTYPE";
        public const string RelSearchCellSizeKey = "RELATIVESEARCHCELLSIZE";

        // general keywords in file
        private const string DisabledQuantityKey = "DISABLED_QUANTITY";
        private const string QuantityKey = "QUANTITY";
        private const string FileNameKey = "FILENAME";
        private const string FileTypeKey = "FILETYPE";
        private const string MethodKey = "METHOD";
        private const string OperandKey = "OPERAND";

        // keywords in file used for polygons (*.pol files)
        private const string ValueKey = "VALUE";
        private const string FactorKey = "FACTOR";
        private const string OffsetKey = "OFFSET";
        private const string SedConcPostfix = "_SedConc";

        private static readonly string[] UnsupportedQuantityKeys =
        {
            "WUANTITY",
            "_UANTITY"
        };

        private readonly ILog log = LogManager.GetLogger(typeof(ExtForceFile));

        // items that existed in the file when the file was read
        private readonly IDictionary<ExtForceFileItem, object> existingForceFileItems;
        private readonly HashSet<ExtForceFileItem> supportedExtForceFileItems;
        private readonly IDictionary<IFeatureData, ExtForceFileItem> polylineForceFileItems;

        private string currentLine;

        public ExtForceFile() : base(true)
        {
            existingForceFileItems = new Dictionary<ExtForceFileItem, object>();
            supportedExtForceFileItems = new HashSet<ExtForceFileItem>();
            polylineForceFileItems = new Dictionary<IFeatureData, ExtForceFileItem>();
            WriteToDisk = true;
        }

        public bool WriteToDisk { get; set; }

        public IEnumerable<IBoundaryCondition> ExistingBoundaryConditions =>
            polylineForceFileItems.Keys.OfType<IBoundaryCondition>();

        /// <summary>
        /// Get the data files that are references in the extForceFile.
        /// </summary>
        /// <param name="modelDefinition"> </param>
        /// <returns> A list of tuples of name and file path. </returns>
        public IEnumerable<string[]> GetFeatureDataFiles(WaterFlowFMModelDefinition modelDefinition)
        {
            ExtForceFileHelper.StartWritingSubFiles();

            foreach (BoundaryConditionSet boundaryConditionSet in
                modelDefinition.BoundaryConditionSets.Where(bc => bc.Feature.Name != null))
            {
                foreach (FlowBoundaryCondition bc in boundaryConditionSet
                                                     .BoundaryConditions.OfType<FlowBoundaryCondition>())
                {
                    polylineForceFileItems.TryGetValue(bc, out ExtForceFileItem matchingItem);
                    List<string[]> dataFiles =
                        ExtForceFileHelper.GetBoundaryDataFiles(bc, boundaryConditionSet, matchingItem).ToList();

                    foreach (string[] dataFile in dataFiles)
                    {
                        yield return dataFile;
                    }
                }
            }

            foreach (SourceAndSink sourceAndSink in modelDefinition.SourcesAndSinks)
            {
                polylineForceFileItems.TryGetValue(sourceAndSink, out ExtForceFileItem matchingItem);
                List<string[]> dataFiles =
                    ExtForceFileHelper.GetSourceAndSinkDataFiles(sourceAndSink, matchingItem).ToList();

                foreach (string[] dataFile in dataFiles)
                {
                    yield return dataFile;
                }
            }
        }

        private string ExtFilePath { get; set; }

        private string ExtSubFilesReferenceFilePath { get; set; }

        private static bool IsNewEntry(string line)
        {
            string lineToLower = line.ToLower();
            IEnumerable<string> keysToCheck = new[]
            {
                QuantityKey,
                DisabledQuantityKey
            }.Concat(UnsupportedQuantityKeys).Select(s => s.ToLower());

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

                supportedExtForceFileItems.Add(extForceFileItem);

                // read the pli file
                string pliFilePath =
                    GetOtherFilePathInSameDirectory(ExtSubFilesReferenceFilePath, extForceFileItem.FileName);

                var reader = new PliFile<Feature2D>();
                if (isSourceAndSink)
                {
                    reader.CreateDelegate =
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
                                    Geometry = PliFile<Feature2D>.CreatePolyLineGeometry(points)
                                };
                }

                IList<Feature2D> features2D = reader.Read(pliFilePath);
                existingForceFileItems[extForceFileItem] = features2D;

                // go through all feature2Ds
                foreach (Feature2D feature2D in features2D)
                {
                    if (isSourceAndSink)
                    {
                        SourceAndSink sourceAndSink;
                        try
                        {
                            sourceAndSink =
                                ExtForceFileHelper.ReadSourceAndSinkData(
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

                        polylineForceFileItems[sourceAndSink] = extForceFileItem;
                        sourcesAndSinks.Add(sourceAndSink);
                    }
                    else // boundary condition
                    {
                        Feature2D uniqueFeature =
                            boundaryConditionSets.Select(bcs => bcs.Feature)
                                                 .FirstOrDefault(f => f.Geometry.Equals(feature2D.Geometry)) ??
                            feature2D;

                        if (uniqueFeature == feature2D)
                        {
                            boundaryConditionSets.Add(new BoundaryConditionSet {Feature = feature2D});
                        }

                        BoundaryCondition boundaryCondition;

                        try
                        {
                            boundaryCondition = ExtForceFileHelper.ReadBoundaryConditionData(pliFilePath,
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

                        polylineForceFileItems[boundaryCondition] = extForceFileItem;
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

                supportedExtForceFileItems.Add(forceFileItem);

                heatFluxModel.ContainsSolarRadiation =
                    forceFileItem.Quantity == ExtForceQuantNames.MeteoDataWithRadiation;
                string extension = Path.GetExtension(forceFileItem.FileName);

                string filePath = GetOtherFilePathInSameDirectory(ExtSubFilesReferenceFilePath, forceFileItem.FileName);
                if (extension == FileConstants.TimFileExtension)
                {
                    new TimFile().Read(filePath, heatFluxModel.MeteoData, modelReferenceDate);
                    existingForceFileItems[forceFileItem] = heatFluxModel.MeteoData;
                }
                else if (extension == FileConstants.GriddedHeatFluxModelFileExtension)
                {
                    string gridFilePath = HeatFluxModel.GetCorrespondingGridFilePath(filePath);

                    if (File.Exists(gridFilePath))
                    {
                        heatFluxModel.GriddedHeatFluxFilePath = filePath;
                        heatFluxModel.GridFilePath = gridFilePath;

                        existingForceFileItems[forceFileItem] = heatFluxModel.Type;
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

        private IEnumerable<ExtForceFileItem> WriteHeatFluxModelData(WaterFlowFMModelDefinition modelDefinition,
                                                                     bool switchTo = true)
        {
            var extForceFileItems = new List<ExtForceFileItem>();
            try
            {
                int temperatureProcessNumber = (int) modelDefinition.HeatFluxModel.Type;

                // Process Temperature is Uniform Composite Model (Temperature 5 in MDU, but *.tim file)
                if (temperatureProcessNumber == 5 && modelDefinition.HeatFluxModel.GriddedHeatFluxFilePath == null)
                {
                    ExtForceFileItem extForceFileItem =
                        GetExistingForceFileItemOrNull(modelDefinition.HeatFluxModel.MeteoData)
                        ??
                        new ExtForceFileItem(
                            modelDefinition.HeatFluxModel.ContainsSolarRadiation
                                ? ExtForceQuantNames.MeteoDataWithRadiation
                                : ExtForceQuantNames.MeteoData)
                        {
                            FileName = modelDefinition.ModelName + FileConstants.MeteoFileExtension,
                            FileType = ExtForceQuantNames.FileTypes.Uniform,
                            Method = 1,
                            Operand = ExtForceQuantNames.OperatorToStringMapping[
                                Operator.Overwrite]
                        };

                    if (WriteToDisk)
                    {
                        string path = GetOtherFilePathInSameDirectory(ExtFilePath, extForceFileItem.FileName);
                        new TimFile().Write(path, modelDefinition.HeatFluxModel.MeteoData,
                                            (DateTime) modelDefinition.GetModelProperty(KnownProperties.RefDate).Value);
                    }

                    extForceFileItems.Add(extForceFileItem);
                }
                // Process Temperature is Gridded Composite Model (Temperature 5 in MDU, but *.htc and *.grd file)
                else if (temperatureProcessNumber == 5 && modelDefinition.HeatFluxModel.GriddedHeatFluxFilePath != null)
                {
                    ExtForceFileItem extForceFileItem =
                        GetExistingForceFileItemOrNull(modelDefinition.HeatFluxModel.Type);

                    // extForceFileItem should be existing, since it should be created during the import and a Gridded Composite
                    // Model heat flux cannot be created in the GUI
                    if (extForceFileItem == null)
                    {
                        throw new InvalidOperationException("heat flux model was not correctly imported");
                    }

                    if (WriteToDisk)
                    {
                        string newPath = Path.Combine(Path.GetDirectoryName(ExtFilePath), extForceFileItem.FileName);

                        modelDefinition.HeatFluxModel.CopyTo(newPath, switchTo);
                    }

                    extForceFileItems.Add(extForceFileItem);
                }

                return extForceFileItems;
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is InvalidOperationException ||
                                       ex is ArgumentException || ex is PathTooLongException ||
                                       ex is UnauthorizedAccessException || ex is DirectoryNotFoundException ||
                                       ex is IOException || ex is SecurityException)
            {
                log.ErrorFormat("Error during writing the heat flux model: {0}", ex.Message);
                return extForceFileItems;
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
                    sedimentItem.Quantity.Substring(ExtForceQuantNames.InitialSpatialVaryingSedimentPrefix.Length) +
                    SedConcPostfix;
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

                if (extForceFileItem.ModelData.ContainsKey(FricTypeKey))
                {
                    frictionType = GetIntegerPropertyValue(extForceFileItem.ModelData[FricTypeKey].ToString());
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
                    throw new ArgumentException(
                        $"Cannot construct spatial operation for file {extForceFileItem.FileName} with file type {extForceFileItem.FileType}");
            }
        }

        private ISpatialOperation CreatePolygonOperation(ExtForceFileItem extForceFileItem)
        {
            string path = GetOtherFilePathInSameDirectory(ExtSubFilesReferenceFilePath, extForceFileItem.FileName);

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

            existingForceFileItems[extForceFileItem] = operation;

            return operation;
        }

        private ISpatialOperation CreateSamplesOperation(ExtForceFileItem extForceFileItem)
        {
            string operationName = Path.GetFileNameWithoutExtension(extForceFileItem.FileName);

            var operation = new ImportSamplesSpatialOperationExtension
            {
                Name = operationName,
                FilePath = GetOtherFilePathInSameDirectory(ExtSubFilesReferenceFilePath, extForceFileItem.FileName)
            };

            if (extForceFileItem.ModelData.TryGetValue(AveragingTypeKey, out object value))
            {
                operation.AveragingMethod = (GridCellAveragingMethod) value;
            }

            if (extForceFileItem.ModelData.TryGetValue(RelSearchCellSizeKey, out value))
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

            existingForceFileItems[extForceFileItem] = operation;

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
                supportedExtForceFileItems.Add(extForceFileItem);
                try
                {
                    IWindField windField = ExtForceFileHelper.CreateWindField(extForceFileItem, ExtFilePath);

                    string windFile =
                        GetOtherFilePathInSameDirectory(ExtSubFilesReferenceFilePath, extForceFileItem.FileName);

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
                    existingForceFileItems[extForceFileItem] = windField;
                }
                catch (Exception e)
                {
                    log.Warn(e.Message);
                }
            }
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

        private string GetKeyPart(string line)
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

        private ExtForceFileItem GetExistingForceFileItemOrNull(object value)
        {
            return existingForceFileItems.Where(kvp => Equals(kvp.Value, value)).Select(kvp => kvp.Key)
                                         .FirstOrDefault();
        }

        #region write logic

        /// <summary>
        /// Writes the model definition external forcings to file.
        /// </summary>
        /// <param name="extForceFilePath"> File path </param>
        /// <param name="modelDefinition"> External forcings data </param>
        /// <param name="writeBoundaryConditions"> Whether we are writing boundary conditions. </param>
        public void Write(string extForceFilePath, WaterFlowFMModelDefinition modelDefinition,
                          bool writeBoundaryConditions = true, bool switchTo = true)
        {
            ExtFilePath = extForceFilePath;
            Write(modelDefinition, writeBoundaryConditions, switchTo);
        }

        private void Write(WaterFlowFMModelDefinition modelDefinition, bool writeBoundaryConditions, bool switchTo)
        {
            IList<ExtForceFileItem> extForceFileItems =
                WriteExtForceFileSubFiles(modelDefinition, writeBoundaryConditions, switchTo);

            if (extForceFileItems.Any())
            {
                WriteExtForceFile(extForceFileItems);
                modelDefinition.GetModelProperty(KnownProperties.ExtForceFile)
                               .SetValueAsString(Path.GetFileName(ExtFilePath));
            }
            else
            {
                FileUtils.DeleteIfExists(ExtFilePath);
                modelDefinition.GetModelProperty(KnownProperties.ExtForceFile).SetValueAsString(string.Empty);
            }
        }

        private void WriteExtForceFile(IEnumerable<ExtForceFileItem> extForceFileItems)
        {
            OpenOutputFile(ExtFilePath);
            try
            {
                foreach (ExtForceFileItem extForceFileItem in extForceFileItems)
                {
                    WriteLine("");
                    WriteLine((extForceFileItem.Enabled ? QuantityKey : DisabledQuantityKey) +
                              "=" + extForceFileItem.Quantity);
                    WriteLine(FileNameKey + "=" + extForceFileItem.FileName);
                    WriteLine(FileTypeKey + "=" + extForceFileItem.FileType);
                    WriteLine(MethodKey + "=" + extForceFileItem.Method);
                    WriteLine(OperandKey + "=" + extForceFileItem.Operand);
                    if (!double.IsNaN(extForceFileItem.Value))
                    {
                        WriteLine(ValueKey + "=" + extForceFileItem.Value);
                    }

                    if (!double.IsNaN(extForceFileItem.Factor))
                    {
                        WriteLine(FactorKey + "=" + extForceFileItem.Factor);
                    }

                    if (!double.IsNaN(extForceFileItem.Offset))
                    {
                        WriteLine(OffsetKey + "=" + extForceFileItem.Offset);
                    }

                    if (extForceFileItem.ModelData != null)
                    {
                        foreach (KeyValuePair<string, object> modelData in extForceFileItem.ModelData)
                        {
                            WriteLine(modelData.Key + "=" + modelData.Value);
                        }
                    }
                }
            }
            finally
            {
                CloseOutputFile();
            }
        }

        /// <summary>
        /// Writes data files references by the external forcings file.
        /// </summary>
        /// <param name="path"> File path. </param>
        /// <param name="modelDefinition"> Contains data to be written. </param>
        /// <param name="switchTo"> Flag denoting whether to switch to the file path directory (save) </param>
        /// <param name="writeBoundaryConditions"> Flag denoting whether to write boundary conditions </param>
        /// <returns> Resulting force file items </returns>
        public IEnumerable<ExtForceFileItem> WriteExtForceFileSubFiles(string path,
                                                                       WaterFlowFMModelDefinition modelDefinition,
                                                                       bool switchTo, bool writeBoundaryConditions)
        {
            ExtFilePath = path;
            return WriteExtForceFileSubFiles(modelDefinition, writeBoundaryConditions, switchTo);
        }

        private IList<ExtForceFileItem> WriteExtForceFileSubFiles(WaterFlowFMModelDefinition modelDefinition,
                                                                  bool writeBoundaryConditions, bool switchTo = true)
        {
            var extForceFileItems = new List<ExtForceFileItem>();

            ExtForceFileHelper.StartWritingSubFiles(); // hack: tracks & resolves duplicate filenames

            if (writeBoundaryConditions)
            {
                extForceFileItems.AddRange(
                    WriteBoundaryConditions(modelDefinition).Distinct());
            }

            extForceFileItems.AddRange(WriteSourcesAndSinks(modelDefinition).Distinct());

            extForceFileItems.AddRange(
                WriteSpatialData(ExtForceQuantNames.InitialWaterLevel,
                                 modelDefinition.GetSpatialOperations(
                                     WaterFlowFMModelDefinition.InitialWaterLevelDataItemName))
                    .Distinct());

            extForceFileItems.AddRange(
                WriteSpatialData(ExtForceQuantNames.InitialSalinity,
                                 modelDefinition.GetSpatialOperations(
                                     WaterFlowFMModelDefinition.InitialSalinityDataItemName))
                    .Distinct());

            extForceFileItems.AddRange(
                WriteSpatialData(ExtForceQuantNames.InitialSalinity,
                                 modelDefinition.GetSpatialOperations(
                                     WaterFlowFMModelDefinition.InitialSalinityDataItemName +
                                     " (layer 1)"))
                    .Distinct());

            extForceFileItems.AddRange(
                WriteSpatialData(ExtForceQuantNames.InitialSalinityTop,
                                 modelDefinition.GetSpatialOperations(
                                     WaterFlowFMModelDefinition.InitialSalinityDataItemName +
                                     " (layer 2)"))
                    .Distinct());

            extForceFileItems.AddRange(
                WriteSpatialData(ExtForceQuantNames.InitialTemperature,
                                 modelDefinition.GetSpatialOperations(
                                     WaterFlowFMModelDefinition.InitialTemperatureDataItemName))
                    .Distinct());

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.FrictCoef,
                                                        modelDefinition.GetSpatialOperations(
                                                            WaterFlowFMModelDefinition.RoughnessDataItemName))
                                           .Distinct());

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.HorEddyViscCoef,
                                                        modelDefinition.GetSpatialOperations(
                                                            WaterFlowFMModelDefinition.ViscosityDataItemName))
                                           .Distinct());

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.HorEddyDiffCoef,
                                                        modelDefinition.GetSpatialOperations(
                                                            WaterFlowFMModelDefinition.DiffusivityDataItemName))
                                           .Distinct());

            extForceFileItems.AddRange(WriteWindItems(modelDefinition).Distinct());

            extForceFileItems.AddRange(WriteHeatFluxModelData(modelDefinition, switchTo).Distinct());

            extForceFileItems.AddRange(WriteUnknownQuantities(modelDefinition));

            foreach (string tracerName in modelDefinition.InitialTracerNames)
            {
                extForceFileItems.AddRange(
                    WriteSpatialData(ExtForceQuantNames.InitialTracerPrefix + tracerName,
                                     modelDefinition.GetSpatialOperations(tracerName))
                        .Distinct());
            }

            /* DELFT3DFM-1112
             * This is only meant for SedimentConcentration */
            IEnumerable<string> sedConcSpatiallyVarying =
                modelDefinition.InitialSpatiallyVaryingSedimentPropertyNames.Where(sp => sp.EndsWith(SedConcPostfix));

            foreach (string spatiallyVaryingSedimentPropertyName in sedConcSpatiallyVarying)
            {
                IList<ISpatialOperation> spatialOperations =
                    modelDefinition.GetSpatialOperations(spatiallyVaryingSedimentPropertyName);

                List<ExtForceFileItem> forceFileItems = WriteSpatialData(spatiallyVaryingSedimentPropertyName,
                                                                         spatialOperations,
                                                                         ExtForceQuantNames
                                                                             .InitialSpatialVaryingSedimentPrefix)
                                                        .Distinct().ToList();

                //Remove the postfix from the quantity (it is not accepted by the kernel)
                if (spatiallyVaryingSedimentPropertyName.EndsWith(SedConcPostfix))
                {
                    forceFileItems.ForEach(ffi => ffi.Quantity =
                                                      ffi.Quantity.Substring(
                                                          0, ffi.Quantity.Length - SedConcPostfix.Length));
                }

                extForceFileItems.AddRange(forceFileItems);
            }

            return extForceFileItems;
        }

        private IEnumerable<ExtForceFileItem> WriteUnknownQuantities(WaterFlowFMModelDefinition modelDefinition)
        {
            foreach (IUnsupportedFileBasedExtForceFileItem unsupportedExtForceFileItem in modelDefinition
                .UnsupportedFileBasedExtForceFileItems)
            {
                string relativeFilePath = unsupportedExtForceFileItem.UnsupportedExtForceFileItem.FileName;
                string targetPath = Path.Combine(Path.GetDirectoryName(ExtFilePath), relativeFilePath);
                unsupportedExtForceFileItem.CopyTo(targetPath);

                yield return unsupportedExtForceFileItem.UnsupportedExtForceFileItem;
            }
        }

        private IEnumerable<ExtForceFileItem> WriteSourcesAndSinks(WaterFlowFMModelDefinition modelDefinition)
        {
            var referenceTime = (DateTime) modelDefinition.GetModelProperty(KnownProperties.RefDate).Value;

            foreach (SourceAndSink sourceAndSink in modelDefinition.SourcesAndSinks.Where(ss => ss.Feature.Name != null)
            )
            {
                polylineForceFileItems.TryGetValue(sourceAndSink, out ExtForceFileItem matchingItem);

                yield return ExtForceFileHelper.WriteSourceAndSinkData(ExtFilePath, sourceAndSink, referenceTime,
                                                                       matchingItem, WriteToDisk, modelDefinition);
            }
        }

        private IEnumerable<ExtForceFileItem> WriteBoundaryConditions(WaterFlowFMModelDefinition modelDefinition)
        {
            var referenceTime = (DateTime) modelDefinition.GetModelProperty(KnownProperties.RefDate).Value;

            foreach (BoundaryConditionSet boundaryConditionSet in
                modelDefinition.BoundaryConditionSets.Where(bcs => bcs.Feature.Name != null))
            {
                List<FlowBoundaryCondition> flowBoundaryConditions =
                    boundaryConditionSet.BoundaryConditions.OfType<FlowBoundaryCondition>().ToList();

                if (WriteToDisk && !flowBoundaryConditions.Any())
                {
                    log.WarnFormat(
                        "Boundary {0} has no boundary conditions defined for flow, and cannot be written to disc.",
                        boundaryConditionSet.Name);
                }

                foreach (FlowBoundaryCondition flowBoundaryCondition in flowBoundaryConditions)
                {
                    if (!polylineForceFileItems.TryGetValue(flowBoundaryCondition, out ExtForceFileItem matchingItem))
                    {
                        continue; //new boundary conditions shall be written by BndExtForceFile.
                    }

                    int index =
                        boundaryConditionSet.BoundaryConditions
                                            .Where(b => b.VariableName == flowBoundaryCondition.VariableName)
                                            .ToList()
                                            .IndexOf(flowBoundaryCondition);

                    yield return
                        ExtForceFileHelper.WriteBoundaryData(ExtFilePath, flowBoundaryCondition, referenceTime, index,
                                                             matchingItem, WriteToDisk);
                }
            }
        }

        private IEnumerable<ExtForceFileItem> WriteSpatialData(string quantity,
                                                               IEnumerable<ISpatialOperation> spatialOperations,
                                                               string prefix = null)
        {
            if (spatialOperations == null)
            {
                yield break;
            }

            // if all ops are interpolations/set value within polygons, write them to the file
            foreach (ISpatialOperation spatialOperation in spatialOperations)
            {
                var importSamplesOperation = spatialOperation as ImportSamplesSpatialOperationExtension;
                if (importSamplesOperation != null)
                {
                    ExtForceFileItem existingItem = GetExistingForceFileItemOrNull(importSamplesOperation);
                    yield return
                        ExtForceFileHelper.WriteInitialConditionsSamples(ExtFilePath, quantity, importSamplesOperation,
                                                                         existingItem, WriteToDisk, prefix);
                    continue;
                }

                var polygonOperation = spatialOperation as SetValueOperation;
                if (polygonOperation != null)
                {
                    ExtForceFileItem existingItem = GetExistingForceFileItemOrNull(spatialOperation);
                    yield return
                        ExtForceFileHelper.WriteInitialConditionsPolygon(ExtFilePath, quantity, polygonOperation,
                                                                         existingItem, WriteToDisk, prefix);

                    continue;
                }

                var addSamplesOperation = spatialOperation as AddSamplesOperation;
                if (addSamplesOperation != null)
                {
                    yield return
                        ExtForceFileHelper.WriteInitialConditionsUnsupported(ExtFilePath, quantity, addSamplesOperation,
                                                                             WriteToDisk, prefix);
                    continue;
                }

                throw new NotImplementedException(
                    $"Cannot serialize operation of type {spatialOperation.GetType()} to external forcings file");
            }
        }

        public static string MakeXyzFileName(string quantity)
        {
            return string.Join(".", quantity.Replace(" ", "_").Replace("\t", "_"), ExtForceQuantNames.XyzFileExtension);
        }

        private IEnumerable<ExtForceFileItem> WriteWindItems(WaterFlowFMModelDefinition modelDefinition)
        {
            var referenceTime = (DateTime) modelDefinition.GetModelProperty(KnownProperties.RefDate).Value;
            string directory = Path.GetDirectoryName(ExtFilePath);
            ExtForceFileHelper.StartWritingSubFiles();

            foreach (IWindField windField in modelDefinition.WindFields)
            {
                if (windField is IFileBased fileBasedWindField)
                {
                    ExtForceFileItem extForceFileItem = GetExistingForceFileItemOrNull(windField) ??
                                                        ExtForceFileHelper.CreateWindFieldExtForceFileItem(windField,
                                                                                                           Path
                                                                                                               .GetFileName(
                                                                                                                   fileBasedWindField
                                                                                                                       .Path));
                    string newPath = Path.Combine(Path.GetDirectoryName(ExtFilePath), extForceFileItem.FileName);
                    fileBasedWindField.CopyTo(newPath);
                    yield return extForceFileItem;
                }

                if (windField is UniformWindField)
                {
                    string fileName = string.Join(".", ExtForceQuantNames.WindQuantityNames[windField.Quantity],
                                                  ExtForceQuantNames.TimFileExtension);
                    ExtForceFileItem extForceFileItem = GetExistingForceFileItemOrNull(windField) ??
                                                        ExtForceFileHelper.CreateWindFieldExtForceFileItem(
                                                            windField, fileName);
                    ExtForceFileHelper.AddSuffixInCaseOfDuplicateFile(extForceFileItem);
                    var timFile = new TimFile();
                    string timFilePath = Path.Combine(directory, extForceFileItem.FileName);
                    timFile.Write(timFilePath, windField.Data, referenceTime);
                    yield return extForceFileItem;
                }
            }
        }

        #endregion

        #region Read logic

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

        private IEnumerable<ExtForceFileItem> GetUnknownExtForceFileItems(
            IEnumerable<ExtForceFileItem> allExtForceFileItems)
        {
            return allExtForceFileItems.Except(supportedExtForceFileItems);
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
                    GetOtherFilePathInSameDirectory(ExtSubFilesReferenceFilePath, unknownForceFileItem.FileName);

                var unsupportedFileBasedExtForceFileItem =
                    new UnsupportedFileBasedExtForceFileItem(referencedFilePath, unknownForceFileItem);

                modelDefinition.UnsupportedFileBasedExtForceFileItems.Add(unsupportedFileBasedExtForceFileItem);
            }
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
                CloseInputFile();
            }
        }

        private ExtForceFileItem ReadQuantityBlock(int startLineNumber)
        {
            string propertyName = GetKeyPart(currentLine);
            var extForceFileItem = new ExtForceFileItem(GetValuePart(currentLine));

            if (propertyName != QuantityKey)
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
                case FileNameKey:
                    SetFileName(extForceFileItem);
                    break;
                case FileTypeKey:
                    SetFileType(extForceFileItem);
                    break;
                case MethodKey:
                    SetMethod(extForceFileItem);
                    break;
                case OperandKey:
                    SetOperand(extForceFileItem);
                    break;
                case ValueKey:
                    SetValue(extForceFileItem);
                    break;
                case FactorKey:
                    SetFactor(extForceFileItem);
                    break;
                case OffsetKey:
                    SetOffset(extForceFileItem);
                    break;
                case AreaKey:
                    SetArea(extForceFileItem);
                    break;
                case AveragingTypeKey:
                    SetAveragingType(extForceFileItem);
                    break;
                case RelSearchCellSizeKey:
                    SetRelativeSearchCellSize(extForceFileItem);
                    break;
                case FricTypeKey:
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
                LogWarningQuantityPropertyAlreadySet(FileNameKey);
            }
        }

        private void SetFileType(ExtForceFileItem extForceFileItem)
        {
            if (extForceFileItem.FileName == null)
            {
                throw new FormatException(GetMessageUnexpectedKeyword(FileTypeKey));
            }

            if (extForceFileItem.FileType == int.MinValue)
            {
                extForceFileItem.FileType = GetIntegerPropertyValue(currentLine);
            }
            else
            {
                LogWarningQuantityPropertyAlreadySet(FileTypeKey);
            }
        }

        private void SetMethod(ExtForceFileItem extForceFileItem)
        {
            if (extForceFileItem.FileType == int.MinValue)
            {
                throw new FormatException(GetMessageUnexpectedKeyword(MethodKey));
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
                LogWarningQuantityPropertyAlreadySet(MethodKey);
            }
        }

        private void SetOperand(ExtForceFileItem extForceFileItem)
        {
            if (extForceFileItem.Method == int.MinValue)
            {
                throw new FormatException(GetMessageUnexpectedKeyword(OperandKey));
            }

            if (extForceFileItem.Operand == null)
            {
                extForceFileItem.Operand = GetValuePart(currentLine);
            }
            else
            {
                LogWarningQuantityPropertyAlreadySet(OperandKey);
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
                LogWarningQuantityPropertyAlreadySet(ValueKey);
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
                LogWarningQuantityPropertyAlreadySet(FactorKey);
            }
        }

        private void SetOffset(ExtForceFileItem extForceFileItem)
        {
            if (extForceFileItem.Operand == null)
            {
                throw new FormatException(GetMessageUnexpectedKeyword(OffsetKey));
            }

            if (double.IsNaN(extForceFileItem.Offset))
            {
                extForceFileItem.Offset = GetDouble(GetValuePart(currentLine));
            }
            else
            {
                LogWarningQuantityPropertyAlreadySet(OffsetKey);
            }
        }

        private void SetArea(ExtForceFileItem extForceFileItem)
        {
            if (extForceFileItem.Quantity != ExtForceQuantNames.SourceAndSink && extForceFileItem.Operand == null)
            {
                throw new FormatException(GetMessageUnexpectedKeyword(AreaKey));
            }

            if (extForceFileItem.ModelData.ContainsKey(AreaKey))
            {
                LogWarningQuantityPropertyAlreadySet(AreaKey);
            }

            extForceFileItem.ModelData[AreaKey] = GetDoublePropertyValue(currentLine);
        }

        private void SetAveragingType(ExtForceFileItem extForceFileItem)
        {
            if (extForceFileItem.FileType != ExtForceQuantNames.FileTypes.Triangulation &&
                extForceFileItem.Operand == null)
            {
                throw new FormatException(GetMessageUnexpectedKeyword(AveragingTypeKey));
            }

            if (extForceFileItem.ModelData.ContainsKey(AveragingTypeKey))
            {
                LogWarningQuantityPropertyAlreadySet(AveragingTypeKey);
            }

            extForceFileItem.ModelData[AveragingTypeKey] = GetIntegerPropertyValue(currentLine);
        }

        private void SetRelativeSearchCellSize(ExtForceFileItem extForceFileItem)
        {
            if (extForceFileItem.FileType != ExtForceQuantNames.FileTypes.Triangulation &&
                extForceFileItem.Operand == null)
            {
                throw new FormatException(GetMessageUnexpectedKeyword(RelSearchCellSizeKey));
            }

            if (extForceFileItem.ModelData.ContainsKey(RelSearchCellSizeKey))
            {
                LogWarningQuantityPropertyAlreadySet(RelSearchCellSizeKey);
            }

            extForceFileItem.ModelData[RelSearchCellSizeKey] = GetDoublePropertyValue(currentLine);
        }

        private void SetFrictionType(ExtForceFileItem extForceFileItem)
        {
            if (extForceFileItem.ModelData.ContainsKey(FricTypeKey))
            {
                LogWarningQuantityPropertyAlreadySet(FricTypeKey);
            }

            extForceFileItem.ModelData[FricTypeKey] = GetIntegerPropertyValue(currentLine);
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

        #endregion
    }
}