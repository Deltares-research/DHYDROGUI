using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.IO.ExtForce;
using GeoAPI.Extensions.Feature;
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using SharpMap;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class ExtForceFile : FMSuiteFileBase
    {
        private readonly ILog log = LogManager.GetLogger(typeof (ExtForceFile));
        
        // Known file extensions
        public const string Extension = ".ext";

        // items that existed in the file when the file was read
        private readonly IDictionary<ExtForceData, object> existingForceFileItems;
        private readonly IDictionary<IFeatureData, ExtForceData> polylineForceFileItems;

        private readonly IFileSystem fileSystem;
        
        public ExtForceFile() : base(true)
        {
            existingForceFileItems = new Dictionary<ExtForceData, object>();
            polylineForceFileItems = new Dictionary<IFeatureData, ExtForceData>();
            fileSystem = new FileSystem();
            WriteToDisk = true;
        }

        private string FilePath { get; set; }

        public bool WriteToDisk { get; set; }

        public IEnumerable<IBoundaryCondition> ExistingBoundaryConditions
        {
            get { return polylineForceFileItems.Keys.OfType<IBoundaryCondition>(); }
        }

        #region write logic

        /// <summary>
        /// Writes the model definition external forcings to file.
        /// </summary>
        /// <param name="extForceFilePath">File path</param>
        /// <param name="modelDefinition">External forcings data</param>
        /// <param name="writeBoundaryConditions">Whether we are writing boundary conditions.</param> 
        public void Write(string extForceFilePath, WaterFlowFMModelDefinition modelDefinition, bool writeBoundaryConditions = true)
        {
            FilePath = extForceFilePath;
            Write(modelDefinition, writeBoundaryConditions);
        }

        private void Write(WaterFlowFMModelDefinition modelDefinition, bool writeBoundaryConditions = true)
        {
            var extForceFileItems = WriteExtForceFileSubFiles(modelDefinition, writeBoundaryConditions);
            
            foreach (var notUsedExtForceFileItem in modelDefinition.UnsupportedFileBasedExtForceFileItems)
            {
                extForceFileItems.Add(notUsedExtForceFileItem.UnsupportedExtForceFileItem);
                var newPath = Path.Combine(Path.GetDirectoryName(FilePath),
                    Path.GetFileName(notUsedExtForceFileItem.UnsupportedExtForceFileItem.FileName));
                notUsedExtForceFileItem.CopyTo(newPath);
            }

            if (extForceFileItems.Count > 0)
            {
                WriteExtForceFile(new ExtForceFileData(extForceFileItems));
                modelDefinition.GetModelProperty(KnownProperties.ExtForceFile).SetValueAsString(Path.GetFileName(FilePath));
            }
            else
            {
                FileUtils.DeleteIfExists(FilePath);
                modelDefinition.GetModelProperty(KnownProperties.ExtForceFile).SetValueAsString(string.Empty);
            }
        }

        private void WriteExtForceFile(ExtForceFileData extForceFileData)
        {
            using (FileSystemStream stream = fileSystem.File.Open(FilePath, FileMode.Create))
            {
                var formatter = new ExtForceFileFormatter();
                formatter.Format(extForceFileData, stream);
            }
        }

        /// <summary>
        /// Writes data files references by the external forcings file.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <param name="modelDefinition">Contains data to be written.</param>
        /// <param name="writeBoundaryConditions">Flag denoting whether to write boundary conditions</param>
        /// <returns>Resulting force file items</returns>
        public IEnumerable<ExtForceData> WriteExtForceFileSubFiles(string path, WaterFlowFMModelDefinition modelDefinition, bool writeBoundaryConditions)
        {
            FilePath = path;
            return WriteExtForceFileSubFiles(modelDefinition, writeBoundaryConditions);
        }

        private IList<ExtForceData> WriteExtForceFileSubFiles(WaterFlowFMModelDefinition modelDefinition, bool writeBoundaryConditions)
        {
            var extForceFileItems = new List<ExtForceData>();

            ExtForceFileHelper.StartWritingSubFiles(); // hack: tracks & resolves duplicate filenames

            if (writeBoundaryConditions)
            {
                extForceFileItems.AddRange(
                    WriteBoundaryConditions(modelDefinition).Distinct());
            }

            extForceFileItems.AddRange(WriteSourcesAndSinks(modelDefinition).Distinct());

            extForceFileItems.AddRange(
                WriteSpatialData(ExtForceQuantNames.InitialSalinity,
                    modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.InitialSalinityDataItemName))
                    .Distinct());

            extForceFileItems.AddRange(
                WriteSpatialData(ExtForceQuantNames.InitialSalinity,
                    modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.InitialSalinityDataItemName +
                                                         " (layer 1)"))
                    .Distinct());

            extForceFileItems.AddRange(
                WriteSpatialData(ExtForceQuantNames.InitialSalinityTop,
                    modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.InitialSalinityDataItemName +
                                                         " (layer 2)"))
                    .Distinct());

            extForceFileItems.AddRange(
                WriteSpatialData(ExtForceQuantNames.InitialTemperature,
                    modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.InitialTemperatureDataItemName))
                    .Distinct());

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.HorEddyViscCoef,
                modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.ViscosityDataItemName)).Distinct());

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.HorEddyDiffCoef,
                modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.DiffusivityDataItemName)).Distinct());

            extForceFileItems.AddRange(WriteWindItems(modelDefinition).Distinct());

            extForceFileItems.AddRange(WriteHeatFluxModelData(modelDefinition).Distinct());

            foreach (var tracerName in modelDefinition.InitialTracerNames)
            {
                extForceFileItems.AddRange(
                    WriteSpatialData(ExtForceQuantNames.InitialTracerPrefix + tracerName, modelDefinition.GetSpatialOperations(tracerName))
                    .Distinct());
            }

            /* DELFT3DFM-1112
             * This is only meant for SedimentConcentration */
            var sedConcSpatiallyVarying =
                modelDefinition.InitialSpatiallyVaryingSedimentPropertyNames.Where(sp => sp.EndsWith(ExtForceQuantNames.SedConcPostfix));
            foreach (var spatiallyVaryingSedimentPropertyName in sedConcSpatiallyVarying)
            {
                var spatialOperations = modelDefinition.GetSpatialOperations(spatiallyVaryingSedimentPropertyName);
                if (spatialOperations == null ||
                    !spatialOperations.All(s => s is ImportSamplesOperationImportData ||
                                                s is AddSamplesOperation))
                {
                    var warnMsg = String.Format(
                        Resources.SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_No_spatial_operations_of_type_Import__Add_or_Value_found_for_spatially_varying_property__0___Remember_to_interpolate_them_to_generate_the_xyz_file__Otherwise_the_model_might_not_run_as_expected_,
                        spatiallyVaryingSedimentPropertyName); 
                    if (spatialOperations != null)
                    {
                        warnMsg = String.Format(
                            Resources
                                .SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_Cannot_create_xyz_file_for_spatial_varying_initial_condition__0__because_it_is_a_value_spatial_operation__please_interpolate_the_operation_to_the_grid_or,
                            spatiallyVaryingSedimentPropertyName);
                    }

                    log.Warn(warnMsg);
                    continue;
                }
                var forceFileItems = WriteSpatialData(spatiallyVaryingSedimentPropertyName,
                        spatialOperations, ExtForceQuantNames.InitialSpatialVaryingSedimentPrefix)
                    .Distinct().ToList();
                
                //Remove the postfix from the quantity (it is not accepted by the kernel)
                if(spatiallyVaryingSedimentPropertyName.EndsWith(ExtForceQuantNames.SedConcPostfix))
                    forceFileItems.ForEach( ffi => ffi.Quantity = ffi.Quantity.Substring(0, ffi.Quantity.Length - ExtForceQuantNames.SedConcPostfix.Length));
                extForceFileItems.AddRange(forceFileItems);
            }

            return extForceFileItems;
        }

        private IEnumerable<ExtForceData> WriteSourcesAndSinks(WaterFlowFMModelDefinition modelDefinition)
        {
            var referenceTime = modelDefinition.GetReferenceDateAsDateTime();

            foreach (var sourceAndSink in modelDefinition.SourcesAndSinks.Where(ss => ss.Feature.Name != null))
            {
                ExtForceData matchingItem;
                polylineForceFileItems.TryGetValue(sourceAndSink, out matchingItem);

                yield return ExtForceFileHelper.WriteSourceAndSinkData(FilePath, sourceAndSink, referenceTime, matchingItem, WriteToDisk, modelDefinition);
            }
        }

        private IEnumerable<ExtForceData> WriteBoundaryConditions(WaterFlowFMModelDefinition modelDefinition)
        {
            var referenceTime = modelDefinition.GetReferenceDateAsDateTime();

            foreach (var boundaryConditionSet in modelDefinition.BoundaryConditionSets.Where(bcs => bcs.Feature.Name != null))
            {
                var flowBoundaryConditions =
                    boundaryConditionSet.BoundaryConditions.OfType<FlowBoundaryCondition>().ToList();

                if (WriteToDisk && !flowBoundaryConditions.Any())
                {
                    log.WarnFormat(
                        "Boundary {0} has no boundary conditions defined for flow, and cannot be written to disc.",
                        boundaryConditionSet.Name);
                }
                foreach (var flowBoundaryCondition in flowBoundaryConditions)
                {
                    ExtForceData matchingItem;
                    if (!polylineForceFileItems.TryGetValue(flowBoundaryCondition, out matchingItem))
                    {
                        continue; //new boundary conditions shall be written by BndExtForceFile.
                    }

                    var index =
                        boundaryConditionSet.BoundaryConditions.Where(b => b.VariableName == flowBoundaryCondition.VariableName)
                            .ToList()
                            .IndexOf(flowBoundaryCondition);

                    yield return
                        ExtForceFileHelper.WriteBoundaryData(FilePath, flowBoundaryCondition, referenceTime, index,
                            matchingItem, WriteToDisk);
                }
            }
        }

        private IEnumerable<ExtForceData> WriteSpatialData(string quantity, IEnumerable<ISpatialOperation> spatialOperations, string prefix = null)
        {
            if(spatialOperations != null)
            {
                // if all ops are interpolations/set value within polygons, write them to the file
                foreach (var spatialOperation in spatialOperations)
                {
                    var importSamplesOperation = spatialOperation as ImportSamplesOperationImportData;
                    if (importSamplesOperation != null)
                    {
                        var existingItem = GetExistingForceFileItemOrNull(importSamplesOperation);
                        yield return
                            ExtForceFileHelper.WriteInitialConditionsSamples(FilePath, quantity, importSamplesOperation,
                                existingItem, WriteToDisk, prefix);
                        continue;
                    }
                        
                    var polygonOperation = spatialOperation as SetValueOperation;
                    if (polygonOperation != null)
                    {
                        var existingItem = GetExistingForceFileItemOrNull(spatialOperation);
                        yield return
                            ExtForceFileHelper.WriteInitialConditionsPolygon(FilePath, quantity, polygonOperation,
                                existingItem, WriteToDisk, prefix);
                        continue;
                    }

                    var addSamplesOperation = spatialOperation as AddSamplesOperation;
                    if (addSamplesOperation != null)
                    {
                        yield return
                            ExtForceFileHelper.WriteInitialConditionsUnsupported(FilePath, quantity, addSamplesOperation,
                                WriteToDisk, prefix);
                        continue;
                    }

                    throw new NotImplementedException(
                        string.Format("Cannot serialize operation of type {0} to external forcings file",
                            spatialOperation.GetType()));
                }
            }
        }

        public static string MakeXyzFileName(string quantity)
        {
            return string.Join(".", quantity.Replace(" ", "_").Replace("\t", "_"), ExtForceQuantNames.XyzFileExtension);
        }

        private IEnumerable<ExtForceData> WriteWindItems(WaterFlowFMModelDefinition modelDefinition)
        {
            var referenceTime = modelDefinition.GetReferenceDateAsDateTime();
            var directory = Path.GetDirectoryName(FilePath);
            ExtForceFileHelper.StartWritingSubFiles();

            foreach (var windField in modelDefinition.WindFields)
            {
                var fileBasedWindField = windField as IFileBased;
                if (fileBasedWindField != null)
                {
                    var extForceFileItem = GetExistingForceFileItemOrNull(windField) ??
                                           ExtForceFileHelper.CreateWindFieldExtForceFileItem(windField,
                                               Path.GetFileName(((IFileBased)windField).Path));
                    var newPath = Path.Combine(Path.GetDirectoryName(FilePath), Path.GetFileName(extForceFileItem.FileName));
                    ((IFileBased)windField).CopyTo(newPath);
                    yield return extForceFileItem;
                }

                var uniformWindField = windField as UniformWindField;
                if (uniformWindField != null)
                {
                    var fileName = string.Join(".", ExtForceQuantNames.WindQuantityNames[windField.Quantity],
                        ExtForceQuantNames.TimFileExtension);
                    var extForceFileItem = GetExistingForceFileItemOrNull(windField) ??
                                           ExtForceFileHelper.CreateWindFieldExtForceFileItem(windField, fileName);
                    ExtForceFileHelper.AddSuffixInCaseOfDuplicateFile(extForceFileItem);
                    var timFile = new TimFile();
                    var timFilePath = Path.Combine(directory, extForceFileItem.FileName);
                    timFile.Write(timFilePath, windField.Data, referenceTime);
                    yield return extForceFileItem;
                }
            }
        }
        #endregion

        #region Read logic

        public void Read(string extForceFilePath, WaterFlowFMModelDefinition modelDefinition)
        {
            FilePath = extForceFilePath;
            Read(modelDefinition);
        }

        private void Read(WaterFlowFMModelDefinition modelDefinition)
        {
                var extForceFileData = ParseExtForceFile();
                var forceFileItems = extForceFileData.Forcings.ToList();

                RemoveInvalidForcings(forceFileItems);
                ReadPolyLineData(forceFileItems, modelDefinition);
                ReadWindItems(forceFileItems, modelDefinition);
                ReadHeatFluxModelData(forceFileItems, modelDefinition);
                ReadSpatialData(forceFileItems, modelDefinition);
                ReadInternalTidesFrictionCoefficientIfAvailable(forceFileItems, modelDefinition);
        }

        private ExtForceFileData ParseExtForceFile()
        {
            log.InfoFormat(Resources.Reading_external_forcings_file_from_0_, FilePath);
            
            using (FileSystemStream stream = fileSystem.File.OpenRead(FilePath))
            {
                var parser = new ExtForceFileParser();
                return parser.Parse(stream);
            }
        }
        
        private void RemoveInvalidForcings(ICollection<ExtForceData> forcings)
        {
            var validator = new ExtForceDataValidator(fileSystem);
            var parentDirectory = Path.GetDirectoryName(FilePath);
            
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

        #endregion

        /// <summary>
        /// Get the data files that are references in the extForceFile.
        /// </summary>
        /// <param name="modelDefinition"></param>
        /// <returns>A list of tuples of name and file path.</returns>
        public IEnumerable<string[]> GetFeatureDataFiles(WaterFlowFMModelDefinition modelDefinition)
        {
            ExtForceFileHelper.StartWritingSubFiles();
            foreach (var boundaryConditionSet in modelDefinition.BoundaryConditionSets.Where(bc => bc.Feature.Name != null))
            {
                foreach (var bc in boundaryConditionSet.BoundaryConditions.OfType<FlowBoundaryCondition>())
                {
                    ExtForceData matchingItem;
                    polylineForceFileItems.TryGetValue(bc, out matchingItem);
                    var dataFiles =
                        ExtForceFileHelper.GetBoundaryDataFiles(bc, boundaryConditionSet, matchingItem).ToList();

                    foreach (var dataFile in dataFiles)
                    {
                        yield return dataFile;
                    }
                }
            }
            foreach (var sourceAndSink in modelDefinition.SourcesAndSinks)
            {
                ExtForceData matchingItem;
                polylineForceFileItems.TryGetValue(sourceAndSink, out matchingItem);
                var dataFiles =
                    ExtForceFileHelper.GetSourceAndSinkDataFiles(sourceAndSink, matchingItem).ToList();

                foreach (var dataFile in dataFiles)
                {
                    yield return dataFile;
                }
            }
        }

        private void ReadPolyLineData(IEnumerable<ExtForceData> extForceFileItems,
                                            WaterFlowFMModelDefinition modelDefinition)
        {
            var boundaryConditionSets = modelDefinition.BoundaryConditionSets;
            var boundaryConditions = new List<IBoundaryCondition>();
            var sourcesAndSinks = new List<SourceAndSink>();

            var modelReferenceDate = modelDefinition.GetReferenceDateAsDateTime();

            foreach (var extForceFileItem in extForceFileItems.Where(e => e.FileName.ToLower().EndsWith(".pli")))
            {
                if (extForceFileItem.FileType != ExtForceFileConstants.FileTypes.PolyTim)
                    throw new NotSupportedException("The provided pli file is not a PolyTim file. " +
                                                    extForceFileItem.FileName);

                // check what type of polyline to read
                bool isSourceAndSink = Equals(extForceFileItem.Quantity, ExtForceQuantNames.SourceAndSink);
                if (!ExtForceQuantNames.TryParseBoundaryQuantityType(extForceFileItem.Quantity, out FlowBoundaryQuantityType _) &&
                    !isSourceAndSink)
                {
                    log.ErrorFormat(Resources.ExtForceFile_ReadPolyLineData_Unsupported_quantity_type___0___in_the__ext_file__1__detected__It_will_not_be_imported_, extForceFileItem.Quantity, FilePath);
                    continue;
                }

                // read the pli file
                var pliFilePath = GetOtherFilePathInSameDirectory(FilePath, extForceFileItem.FileName);
                var reader = new PliFile<Feature2D>();
                if (isSourceAndSink)
                {
                    reader.CreateDelegate =
                        (points, name) =>
                            points.Count == 1
                                ? new Feature2DPoint {Name = name, Geometry = new Point(points[0])}
                                : new Feature2D {Name = name, Geometry = PliFile<Feature2D>.CreatePolyLineGeometry(points)};
                }
                var features2D = reader.Read(pliFilePath);
                existingForceFileItems[extForceFileItem] = features2D;

                // go through all feature2Ds
                foreach (var feature2D in features2D)
                {
                    if (isSourceAndSink)
                    {
                        SourceAndSink sourceAndSink;
                        try
                        {
                            sourceAndSink = ExtForceFileHelper.ReadSourceAndSinkData(pliFilePath, feature2D, extForceFileItem, modelReferenceDate);
                        }
                        catch (Exception e)
                        {
                            if (e is ArgumentException || e is PathTooLongException || e is FormatException ||
                                e is FileNotFoundException || e is IOException || e is OutOfMemoryException)
                            {
                                throw new InvalidOperationException(
                                    String.Format(
                                        "An error (Message: {0}) occured while source/sink data for feature {1} in file {2}",
                                        e.Message, feature2D.Name, FilePath), e);
                            }
                            throw;
                        }

                        if (sourceAndSink != null)
                        {
                            polylineForceFileItems[sourceAndSink] = extForceFileItem;
                            sourcesAndSinks.Add(sourceAndSink);
                        }
                    }
                    else // boundary condition
                    {
                        
                        var uniqueFeature =
                            boundaryConditionSets.Select(bcs => bcs.Feature)
                                .FirstOrDefault(f => f.Geometry.Equals(feature2D.Geometry)) ?? feature2D;

                        if (uniqueFeature == feature2D)
                        {
                            boundaryConditionSets.Add(new BoundaryConditionSet {Feature = feature2D});
                        }

                        BoundaryCondition boundaryCondition;

                        try
                        {
                            boundaryCondition = ExtForceFileHelper.ReadBoundaryConditionData(pliFilePath,
                                uniqueFeature,extForceFileItem, modelReferenceDate);
                        }
                        catch (Exception e)
                        {
                            if (e is ArgumentException || e is PathTooLongException || e is FormatException ||
                                e is FileNotFoundException || e is IOException || e is OutOfMemoryException)
                            {
                                throw new InvalidOperationException(
                                    String.Format(
                                        "An error (Message: {0}) occured while reading boundary condition data for feature {1} in file {2}",
                                        e.Message, feature2D.Name, FilePath),
                                    e);
                            }
                            throw;
                        }

                        if (boundaryCondition != null)
                        {
                            polylineForceFileItems[boundaryCondition] = extForceFileItem;
                            boundaryConditions.Add(boundaryCondition);
                        }
                    }
                }
            }

            foreach (var boundaryConditionSet in boundaryConditionSets)
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

        private void ReadHeatFluxModelData(IEnumerable<ExtForceData> extForceFileItems, WaterFlowFMModelDefinition modelDefinition)
        {
            var modelReferenceDate = modelDefinition.GetReferenceDateAsDateTime();
            switch (modelDefinition.HeatFluxModel.Type)
            {
                case HeatFluxModelType.None:
                    return;
                case HeatFluxModelType.ExcessTemperature:
                case HeatFluxModelType.Composite:
                    ReadCompositeTemperatureData(extForceFileItems, modelDefinition.HeatFluxModel, modelReferenceDate);
                    return;
            }
        }

        private void ReadCompositeTemperatureData(IEnumerable<ExtForceData> extForceFileItems,
            HeatFluxModel heatFluxModel, DateTime modelReferenceDate)
        {
            var forceFileItem =
                extForceFileItems.LastOrDefault(e => e.Quantity == ExtForceQuantNames.MeteoData ||
                                                     e.Quantity == ExtForceQuantNames.MeteoDataWithRadiation);

            if (forceFileItem == null) return;

            heatFluxModel.ContainsSolarRadiation = forceFileItem.Quantity == ExtForceQuantNames.MeteoDataWithRadiation;

            var filePath = GetOtherFilePathInSameDirectory(FilePath, forceFileItem.FileName);
            new TimFile().Read(filePath, heatFluxModel.MeteoData, modelReferenceDate);
            existingForceFileItems[forceFileItem] = heatFluxModel.MeteoData;
        }

        private IEnumerable<ExtForceData> WriteHeatFluxModelData(WaterFlowFMModelDefinition modelDefinition)
        {
            if (modelDefinition.HeatFluxModel.MeteoData == null) yield break;

            var extForceFileItem = GetExistingForceFileItemOrNull(modelDefinition.HeatFluxModel)
                                   ??
                                   new ExtForceData
                                   {
                                       Quantity = modelDefinition.HeatFluxModel.ContainsSolarRadiation
                                                      ? ExtForceQuantNames.MeteoDataWithRadiation
                                                      : ExtForceQuantNames.MeteoData,
                                       FileName = modelDefinition.ModelName + "_meteo.tim",
                                       FileType = ExtForceFileConstants.FileTypes.Uniform,
                                       Method = ExtForceFileConstants.Methods.SpaceAndTimeKeepMeteoFields,
                                       Operand = ExtForceQuantNames.OperatorToStringMapping[Operator.Overwrite]
                                   };

            if (WriteToDisk)
            {
                var path = GetOtherFilePathInSameDirectory(FilePath, extForceFileItem.FileName);
                new TimFile().Write(path, modelDefinition.HeatFluxModel.MeteoData, modelDefinition.GetReferenceDateAsDateTime());
            }

            yield return extForceFileItem;
        }

        private void ReadSpatialData(IList<ExtForceData> extForceFileItems, WaterFlowFMModelDefinition modelDefinition)
        {
            var unReadExtForceFileItems = extForceFileItems.ToList();

            var knownQuantities = new Dictionary<string, string>
            {
                {ExtForceQuantNames.InitialSalinity, WaterFlowFMModelDefinition.InitialSalinityDataItemName},
                {ExtForceQuantNames.InitialSalinityTop, WaterFlowFMModelDefinition.InitialSalinityDataItemName},
                {ExtForceQuantNames.InitialTemperature, WaterFlowFMModelDefinition.InitialTemperatureDataItemName},
                {ExtForceQuantNames.FrictCoef, WaterFlowFMModelDefinition.RoughnessDataItemName},
                {ExtForceQuantNames.HorEddyViscCoef, WaterFlowFMModelDefinition.ViscosityDataItemName},
                {ExtForceQuantNames.HorEddyDiffCoef, WaterFlowFMModelDefinition.DiffusivityDataItemName}
            };

            foreach (var quantityPair in knownQuantities)
            {
                var readItems = unReadExtForceFileItems.Where(i => i.Quantity == quantityPair.Key).ToList();
                if (quantityPair.Key.Equals(ExtForceQuantNames.FrictCoef))
                    readItems = FilterByFrictionType(unReadExtForceFileItems, modelDefinition).ToList();

                ReadSpatialOperationData(readItems, modelDefinition, quantityPair.Key, quantityPair.Value);
                
                //Remove read items.
                unReadExtForceFileItems = unReadExtForceFileItems.Except(readItems).ToList();
                if (!unReadExtForceFileItems.Any()) return;
            }

            //Read tracer items.
            var initialTracerItems = unReadExtForceFileItems.Where(fi => fi.Quantity.StartsWith(ExtForceQuantNames.InitialTracerPrefix)).ToList();
            foreach( var tracerItem in initialTracerItems)
            {
                string tracerName = tracerItem.Quantity.Substring(ExtForceQuantNames.InitialTracerPrefix.Length);
                ReadSpatialOperationData(initialTracerItems, modelDefinition, tracerItem.Quantity, tracerName);
            }
            unReadExtForceFileItems = unReadExtForceFileItems.Except(initialTracerItems).ToList();
            if (!unReadExtForceFileItems.Any()) return;

            //Read sediment items.
            var initialSedimentItems = unReadExtForceFileItems.Where(fi => fi.Quantity.StartsWith(ExtForceQuantNames.InitialSpatialVaryingSedimentPrefix)).ToList();
            foreach (var sedimentItem in initialSedimentItems)
            {
                /* DELFT3DFM-1112
                 * The only Spatially Varying Sediment that gets read from the ExtForces file is
                 * SedimentConcentration. We could simply remove its prefix, however, due to the 
                 * way it's meant to be written in said file, we need to add the postfix */
                string spatialvaryingSedConc = sedimentItem.Quantity.Substring(ExtForceQuantNames.InitialSpatialVaryingSedimentPrefix.Length) + ExtForceQuantNames.SedConcPostfix;
                ReadSpatialOperationData(initialSedimentItems, modelDefinition, sedimentItem.Quantity,spatialvaryingSedConc);
            }
            unReadExtForceFileItems = unReadExtForceFileItems.Except(initialSedimentItems).ToList();
            if (!unReadExtForceFileItems.Any()) return;

            //If there are any XYZ items left means they are not a known quantity.
            var xyzUnreadItems = unReadExtForceFileItems.Where(fi => fi.FileName.EndsWith(XyzFile.Extension));
            foreach (var xyzItem in xyzUnreadItems)
            {
                if (xyzItem.Quantity != ExtForceQuantNames.UnsupportedQuantityInMemory)
                {
                    log.WarnFormat(
                        Resources
                            .ExtForceFile_ReadSpatialData_The_model_may_not_run__Spatial_varying_quantity__0__could_not_be_imported_because_the_prefix_does_not_match__1__for_Tracers_or__2__for_Spatial_Varying_Sediments_,
                        xyzItem.Quantity, ExtForceQuantNames.InitialTracerPrefix, ExtForceQuantNames.InitialSpatialVaryingSedimentPrefix);
                }
            }
        }

        private IEnumerable<ExtForceData> FilterByFrictionType(IEnumerable<ExtForceData> extForceFileItems,
            WaterFlowFMModelDefinition modelDefinition)
        {
            var frictionTypeProperty =
                modelDefinition.Properties.FirstOrDefault(
                    p => p.PropertyDefinition.MduPropertyName == KnownProperties.FrictionType);

            var modelFrictionType = frictionTypeProperty != null
                ? int.Parse(frictionTypeProperty.GetValueAsString())
                : 1;

            foreach (var extForceFileItem in extForceFileItems)
            {
                if (extForceFileItem.Quantity != ExtForceQuantNames.FrictCoef) continue;

                var frictionType = modelFrictionType;

                if (extForceFileItem.TryGetModelData(ExtForceFileConstants.Keys.FrictionType, out int forcingFrictionType))
                {
                    frictionType = forcingFrictionType;
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

        private void ReadSpatialOperationData(IEnumerable<ExtForceData> spatialForcingsItems, WaterFlowFMModelDefinition waterFlowFMModelDefinition, string quantity, string dataItemName)
        {
            var forcingsItems = spatialForcingsItems.Where(i => i.Quantity == quantity).ToList();

            if (!forcingsItems.Any()) return;

            var spatialOperations = waterFlowFMModelDefinition.GetSpatialOperations(dataItemName);

            bool createOperationSet = spatialOperations == null;

            if (createOperationSet)
            {
                spatialOperations = new List<ISpatialOperation>();
                waterFlowFMModelDefinition.SpatialOperations[dataItemName] = spatialOperations;
            }

            spatialOperations.Clear();
            foreach (var extForceFileItem in forcingsItems)
            {
                var spatialOperation = CreateSpatialOperation(extForceFileItem);
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
                case 7:
                case 8:
                    return CreateSamplesOperation(extForceFileItem);
                case 10:
                    return CreatePolygonOperation(extForceFileItem);
                default:
                    throw new ArgumentException(
                        string.Format("Cannot construct spatial operation for file {0} with file type {1}",
                            extForceFileItem.FileName, extForceFileItem.FileType));
            }
        }

        private ISpatialOperation CreatePolygonOperation(ExtForceData extForceFileItem)
        {
            var path = GetOtherFilePathInSameDirectory(FilePath, extForceFileItem.FileName);

            var features = new PolFile<Feature2DPolygon>().Read(path).Select(f => new Feature{Geometry = f.Geometry, Attributes = f.Attributes});
            
            var operationName = Path.GetFileNameWithoutExtension(extForceFileItem.FileName);

            var operation = new SetValueOperation
            {
                Value = extForceFileItem.Value ?? double.NaN,
                OperationType = ExtForceQuantNames.ParseOperationType(extForceFileItem.Operand),
                Name = operationName,
                Mask = { Provider = new FeatureCollection(features.ToList(), typeof(Feature)) }
            };

            existingForceFileItems[extForceFileItem] = operation;

            return operation;
        }

        private ISpatialOperation CreateSamplesOperation(ExtForceData extForceFileItem)
        {
            var operationName = Path.GetFileNameWithoutExtension(extForceFileItem.FileName);

            var operation = new ImportSamplesOperationImportData
            {
                Name = operationName,
                FilePath = GetOtherFilePathInSameDirectory(FilePath, extForceFileItem.FileName)
            };
            
            if (extForceFileItem.TryGetModelData(ExtForceFileConstants.Keys.AveragingType, out int averagingMethod))
            {
                operation.AveragingMethod = (GridCellAveragingMethod) averagingMethod;
            }
            if (extForceFileItem.TryGetModelData(ExtForceFileConstants.Keys.RelativeSearchCellSize, out double relativeSearchCellSize))
            {
                operation.RelativeSearchCellSize = relativeSearchCellSize;
            }
            switch (extForceFileItem.Method)
            {
                case ExtForceFileConstants.Methods.Triangulation:
                    operation.InterpolationMethod = SpatialInterpolationMethod.Triangulation;
                    break;
                case ExtForceFileConstants.Methods.Averaging:
                    operation.InterpolationMethod = SpatialInterpolationMethod.Averaging;
                    break;
                default:
                    throw new Exception(string.Format("Invalid interpolation method {0} for file {1}",
                        extForceFileItem.Method, extForceFileItem.FileName));
            }
            return operation;
        }

        private void ReadWindItems(IEnumerable<ExtForceData> extForceFileItems, WaterFlowFMModelDefinition modelDefinition)
        {
            var refDate = modelDefinition.GetReferenceDateAsDateTime();
            foreach (
                var extForceFileItem in
                    extForceFileItems.Where(i => ExtForceQuantNames.WindQuantityNames.Values.Contains(i.Quantity)))
            {
                try
                {
                    var windField = ExtForceFileHelper.CreateWindField(extForceFileItem, FilePath);
                    var windFile = Path.Combine(Path.GetDirectoryName(FilePath), extForceFileItem.FileName);
                    if (!File.Exists(windFile))
                    {
                        throw new FileNotFoundException(string.Format("Wind file {0} could not be found", windFile));
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

        private void ReadInternalTidesFrictionCoefficientIfAvailable(IEnumerable<ExtForceData> extForceFileItems,
            WaterFlowFMModelDefinition modelDefinition)
        {
            try
            {
                var unsupportedFileBasedExtForceFileItems = extForceFileItems.ToList()
                    .Where(fi => fi.Quantity.StartsWith(ExtForceQuantNames.UnsupportedQuantityInMemory)).ToList();

                if (unsupportedFileBasedExtForceFileItems.Count > 0)
                {
                    log.WarnFormat(
                        "Spatial varying quantity {0} detected in the external force file and will be passed to the computational core. This may affect your simulation.",
                        ExtForceQuantNames.UnsupportedQuantityInMemory);

                    foreach (var forceFileItem in unsupportedFileBasedExtForceFileItems)
                    {
                        var unsupportedFileBaseExtForceFile =
                            Path.Combine(Path.GetDirectoryName(FilePath), forceFileItem.FileName);
                        if (!File.Exists(unsupportedFileBaseExtForceFile))
                        {
                            throw new FileNotFoundException(string.Format("File {0} could not be found for an internaltidesfrictioncoefficient quantity in the external force file",
                                unsupportedFileBaseExtForceFile));
                        }

                        var unsupportedFileBasedExtForceFileItem = new UnsupportedFileBasedExtForceFileItem(
                            Path.Combine(Path.GetDirectoryName(FilePath), forceFileItem.FileName), forceFileItem);

                        modelDefinition.UnsupportedFileBasedExtForceFileItems.Add(unsupportedFileBasedExtForceFileItem);
                    }
                }
            }
            catch (Exception e)
            {
                log.Warn(e.Message);
            }
        }

        private ExtForceData GetExistingForceFileItemOrNull(object value)
        {
            return existingForceFileItems.Where(kvp => Equals(kvp.Value, value)).Select(kvp => kvp.Key).FirstOrDefault();
        }
    }
}
