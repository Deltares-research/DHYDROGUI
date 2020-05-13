using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using DelftTools.Functions;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;
using SharpMap;
using SharpMap.Api.SpatialOperations;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public partial class ExtForceFile
    {
        /// <summary>
        /// Writes the model definition external forcings to file.
        /// </summary>
        /// <param name="extForceFilePath"> File path </param>
        /// <param name="modelDefinition"> External forcings data </param>
        /// <param name="writeBoundaryConditions"> Whether we are writing boundary conditions. </param>
        public void Write(string extForceFilePath, WaterFlowFMModelDefinition modelDefinition,
                          bool writeBoundaryConditions = true, bool switchTo = true)
        {
            extFilePath = extForceFilePath;
            Write(modelDefinition, writeBoundaryConditions, switchTo);
        }

        protected override bool WriteCommentBlock(string line, bool doWriteLine)
        {
            if (line.ToUpper().StartsWith(extForcesFileQuantityBlockStarter))
            {
                storedNextOutputLine = line;
                doWriteLine = false;
            }
            else
            {
                if (storedNextOutputLine != null)
                {
                    string contentIdentifier = CreateContentIdentifier(storedNextOutputLine + line.Trim());
                    if (commentBlocks.ContainsKey(contentIdentifier))
                    {
                        foreach (string commentLine in commentBlocks[contentIdentifier])
                        {
                            writer.WriteLine(commentLine);
                        }
                    }

                    writer.WriteLine(storedNextOutputLine);
                    storedNextOutputLine = null;
                }
            }

            return doWriteLine;
        }

        private void Write(WaterFlowFMModelDefinition modelDefinition, bool writeBoundaryConditions, bool switchTo)
        {
            IList<ExtForceFileItem> extForceFileItems =
                WriteExtForceFileSubFiles(modelDefinition, writeBoundaryConditions, switchTo);

            if (extForceFileItems.Any())
            {
                WriteExtForceFile(extForceFileItems);
                modelDefinition.GetModelProperty(KnownProperties.ExtForceFile)
                               .SetValueAsString(Path.GetFileName(extFilePath));
            }
            else
            {
                FileUtils.DeleteIfExists(extFilePath);
                modelDefinition.GetModelProperty(KnownProperties.ExtForceFile).SetValueAsString(string.Empty);
            }
        }

        private void WriteExtForceFile(IEnumerable<ExtForceFileItem> extForceFileItems)
        {
            OpenOutputFile(extFilePath);
            try
            {
                foreach (ExtForceFileItem extForceFileItem in extForceFileItems)
                {
                    WriteLine("");
                    WriteLine((extForceFileItem.Enabled ? quantityKey : disabledQuantityKey) +
                              "=" + extForceFileItem.Quantity);
                    WriteLine(fileNameKey + "=" + extForceFileItem.FileName);
                    WriteLine(fileTypeKey + "=" + extForceFileItem.FileType);
                    WriteLine(methodKey + "=" + extForceFileItem.Method);
                    WriteLine(operandKey + "=" + extForceFileItem.Operand);
                    if (!double.IsNaN(extForceFileItem.Value))
                    {
                        WriteLine(valueKey + "=" + extForceFileItem.Value);
                    }

                    if (!double.IsNaN(extForceFileItem.Factor))
                    {
                        WriteLine(factorKey + "=" + extForceFileItem.Factor);
                    }

                    if (!double.IsNaN(extForceFileItem.Offset))
                    {
                        WriteLine(offsetKey + "=" + extForceFileItem.Offset);
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
            extFilePath = path;
            return WriteExtForceFileSubFiles(modelDefinition, writeBoundaryConditions, switchTo);
        }

        private IList<ExtForceFileItem> WriteExtForceFileSubFiles(WaterFlowFMModelDefinition modelDefinition,
                                                                  bool writeBoundaryConditions, bool switchTo = true)
        {
            var extForceFileItems = new List<ExtForceFileItem>();

            ExtForceFileHelper.StartWritingSubFiles(); // hack: tracks & resolves duplicate file names

            if (writeBoundaryConditions)
            {
                extForceFileItems.AddRange(WriteBoundaryConditions(modelDefinition).Distinct());
            }

            extForceFileItems.AddRange(WriteSourcesAndSinks(modelDefinition).Distinct());

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.InitialWaterLevel,
                                                        modelDefinition.GetSpatialOperations(
                                                            WaterFlowFMModelDefinition.InitialWaterLevelDataItemName)).Distinct());

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.InitialSalinity,
                                                        modelDefinition.GetSpatialOperations(
                                                            WaterFlowFMModelDefinition.InitialSalinityDataItemName)).Distinct());

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.InitialSalinity,
                                                        modelDefinition.GetSpatialOperations(
                                                            WaterFlowFMModelDefinition.InitialSalinityDataItemName +
                                                            " (layer 1)")).Distinct());

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.InitialSalinityTop,
                                                        modelDefinition.GetSpatialOperations(
                                                            WaterFlowFMModelDefinition.InitialSalinityDataItemName +
                                                            " (layer 2)")).Distinct());

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.InitialTemperature,
                                                        modelDefinition.GetSpatialOperations(
                                                            WaterFlowFMModelDefinition.InitialTemperatureDataItemName)).Distinct());

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.FrictCoef,
                                                        modelDefinition.GetSpatialOperations(
                                                            WaterFlowFMModelDefinition.RoughnessDataItemName)).Distinct());

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.HorEddyViscCoef,
                                                        modelDefinition.GetSpatialOperations(
                                                            WaterFlowFMModelDefinition.ViscosityDataItemName)).Distinct());

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.HorEddyDiffCoef,
                                                        modelDefinition.GetSpatialOperations(
                                                            WaterFlowFMModelDefinition.DiffusivityDataItemName)).Distinct());

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
            IEnumerable<string> sedimentConcentrationSpatiallyVarying =
                modelDefinition.InitialSpatiallyVaryingSedimentPropertyNames.Where(sp => sp.EndsWith(sedimentConcentrationPostfix));

            var logHandler = new LogHandler("Ext force warning handler");

            foreach (string spatiallyVaryingSedimentPropertyName in sedimentConcentrationSpatiallyVarying)
            {
                IList<ISpatialOperation> spatialOperations =
                    modelDefinition.GetSpatialOperations(spatiallyVaryingSedimentPropertyName);
                if (spatialOperations?.All(s => s is ImportSamplesSpatialOperation ||
                                                s is AddSamplesOperation) != true)
                {
                    string warnMsg = string.Format(Resources.SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_No_spatial_operations_of_type_Import__Add_or_Value_found_for_spatially_varying_property__0___Remember_to_interpolate_them_to_generate_the_xyz_file__Otherwise_the_model_might_not_run_as_expected_,
                                                   spatiallyVaryingSedimentPropertyName);
                    if (spatialOperations != null)
                    {
                        warnMsg = string.Format(Resources.SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_Cannot_create_xyz_file_for_spatial_varying_initial_condition__0__because_it_is_a_value_spatial_operation__please_interpolate_the_operation_to_the_grid_or,
                                                spatiallyVaryingSedimentPropertyName);
                    }

                    logHandler.ReportWarning(warnMsg);

                    continue;
                }

                List<ExtForceFileItem> forceFileItems = WriteSpatialData(spatiallyVaryingSedimentPropertyName,
                                                                         spatialOperations,
                                                                         ExtForceQuantNames
                                                                             .InitialSpatialVaryingSedimentPrefix)
                                                        .Distinct().ToList();

                //Remove the postfix from the quantity (it is not accepted by the kernel)
                if (spatiallyVaryingSedimentPropertyName.EndsWith(sedimentConcentrationPostfix))
                {
                    forceFileItems.ForEach(ffi => ffi.Quantity =
                                                      ffi.Quantity.Substring(
                                                          0, ffi.Quantity.Length - sedimentConcentrationPostfix.Length));
                }

                extForceFileItems.AddRange(forceFileItems);
            }

            logHandler.LogReport();

            return extForceFileItems;
        }

        private IEnumerable<ExtForceFileItem> WriteUnknownQuantities(WaterFlowFMModelDefinition modelDefinition)
        {
            foreach (IUnsupportedFileBasedExtForceFileItem unsupportedExtForceFileItem in modelDefinition
                .UnsupportedFileBasedExtForceFileItems)
            {
                string relativeFilePath = unsupportedExtForceFileItem.UnsupportedExtForceFileItem.FileName;
                string targetPath = Path.Combine(Path.GetDirectoryName(extFilePath), relativeFilePath);
                unsupportedExtForceFileItem.CopyTo(targetPath);

                yield return unsupportedExtForceFileItem.UnsupportedExtForceFileItem;
            }
        }

        private IEnumerable<ExtForceFileItem> WriteSourcesAndSinks(WaterFlowFMModelDefinition modelDefinition)
        {
            var referenceTime = (DateTime)modelDefinition.GetModelProperty(KnownProperties.RefDate).Value;

            foreach (SourceAndSink sourceAndSink in modelDefinition.SourcesAndSinks.Where(ss => ss.Feature.Name != null))
            {
                polyLineForceFileItems.TryGetValue(sourceAndSink, out ExtForceFileItem matchingItem);

                yield return WriteSourceAndSinkData(sourceAndSink, referenceTime, matchingItem, modelDefinition);
            }
        }

        private IEnumerable<ExtForceFileItem> WriteBoundaryConditions(WaterFlowFMModelDefinition modelDefinition)
        {
            IDictionary<FlowBoundaryCondition, ExtForceFileItem> boundaryConditionsToWrite =
                ExtForceFileItemFactory.GetBoundaryConditionsItems(modelDefinition, polyLineForceFileItems);

            foreach (BoundaryConditionSet boundaryConditionSet in modelDefinition.BoundaryConditionSets.Where(bcs => bcs.Feature.Name != null))
            {
                if (!boundaryConditionSet.BoundaryConditions.OfType<FlowBoundaryCondition>().Any())
                {
                    log.WarnFormat("Boundary {0} has no boundary conditions defined for flow, and cannot be written to disc.",
                                   boundaryConditionSet.Name);
                }
            }

            var referenceTime = (DateTime)modelDefinition.GetModelProperty(KnownProperties.RefDate).Value;

            foreach (KeyValuePair<FlowBoundaryCondition, ExtForceFileItem> boundaryCondition in boundaryConditionsToWrite)
            {
                if (WriteToDisk)
                {
                    WriteBoundaryData(boundaryCondition, referenceTime);
                }

                yield return boundaryCondition.Value;
            }
        }

        private void WriteBoundaryData(
            KeyValuePair<FlowBoundaryCondition, ExtForceFileItem> boundaryConditionWithExtForceFileItem,
            DateTime modelReferenceDate)
        {
            ExtForceFileItem extForceFileItem = boundaryConditionWithExtForceFileItem.Value;
            FlowBoundaryCondition boundaryCondition = boundaryConditionWithExtForceFileItem.Key;

            string directory = Path.GetDirectoryName(extFilePath);

            string pliFilePath = Path.Combine(directory, extForceFileItem.FileName);

            new PliFile<Feature2D>().Write(pliFilePath, new EventedList<Feature2D>
            {
                boundaryCondition.Feature
            });

            int count = boundaryCondition.Feature.Geometry.Coordinates.Length;

            bool qhBoundary = boundaryCondition.DataType == BoundaryConditionDataType.Qh;
            if (qhBoundary)
            {
                count = 1; //yet another inconsistency in the kernel
            }

            for (var i = 0; i < count; ++i)
            {
                string dataFileExtension =
                    ExtForceQuantNames.ForcingToFileExtensionMapping[boundaryCondition.DataType];

                string dataFilePath =
                    ExtForceFileHelper.GetNumberedFilePath(pliFilePath, dataFileExtension, qhBoundary ? 0 : i + 1);

                IFunction data = boundaryCondition.GetDataAtPoint(i);

                if (data == null)
                {
                    if (File.Exists(dataFilePath))
                    {
                        File.Delete(dataFilePath);
                    }
                }
                else
                {
                    switch (boundaryCondition.DataType)
                    {
                        case BoundaryConditionDataType.HarmonicCorrection:
                        case BoundaryConditionDataType.Harmonics:
                        case BoundaryConditionDataType.AstroCorrection:
                        case BoundaryConditionDataType.AstroComponents:
                            new CmpFile().Write(dataFilePath, ExtForceFileHelper.ToHarmonicComponents(data));
                            break;
                        case BoundaryConditionDataType.TimeSeries:
                            VerticalProfileDefinition depthLayerDefinition =
                                boundaryCondition.GetDepthLayerDefinitionAtPoint(i);
                            if (depthLayerDefinition != null &&
                                depthLayerDefinition.Type != VerticalProfileType.Uniform)
                            {
                                new T3DFile().Write(
                                    dataFilePath.Replace(ExtForceQuantNames.TimFileExtension,
                                                         ExtForceQuantNames.T3DFileExtension), data,
                                    depthLayerDefinition,
                                    modelReferenceDate);
                            }
                            else
                            {
                                new TimFile().Write(dataFilePath, data, modelReferenceDate);
                            }

                            break;
                        case BoundaryConditionDataType.Qh:
                            new QhFile().Write(dataFilePath, data);
                            break;
                        default:
                            throw new Exception("Writing boundary condition type " + boundaryCondition.DataType +
                                                " not (yet) implemented");
                    }
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
                var importSamplesOperation = spatialOperation as ImportSamplesSpatialOperation;
                if (importSamplesOperation != null)
                {
                    ExtForceFileItem existingItem = GetExistingForceFileItemOrNull(importSamplesOperation);
                    yield return WriteInitialConditionsSamples(quantity, importSamplesOperation, existingItem, prefix);
                    continue;
                }

                var polygonOperation = spatialOperation as SetValueOperation;
                if (polygonOperation != null)
                {
                    ExtForceFileItem existingItem = GetExistingForceFileItemOrNull(spatialOperation);
                    yield return WriteInitialConditionsPolygon(quantity, polygonOperation, existingItem, prefix);

                    continue;
                }

                var addSamplesOperation = spatialOperation as AddSamplesOperation;
                if (addSamplesOperation != null)
                {
                    yield return WriteInitialConditionsUnsupported(quantity, addSamplesOperation, prefix);
                    continue;
                }

                throw new NotSupportedException($"Cannot serialize operation of type {spatialOperation.GetType()} to external forcings file");
            }
        }

        private IEnumerable<ExtForceFileItem> WriteWindItems(WaterFlowFMModelDefinition modelDefinition)
        {
            var referenceTime = (DateTime)modelDefinition.GetModelProperty(KnownProperties.RefDate).Value;
            string directory = Path.GetDirectoryName(extFilePath);
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
                    string newPath = Path.Combine(Path.GetDirectoryName(extFilePath), extForceFileItem.FileName);
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

        private IEnumerable<ExtForceFileItem> WriteHeatFluxModelData(WaterFlowFMModelDefinition modelDefinition,
                                                                     bool switchTo = true)
        {
            var extForceFileItems = new List<ExtForceFileItem>();
            try
            {
                var temperatureProcessNumber = (int)modelDefinition.HeatFluxModel.Type;

                // Process Temperature is Uniform Composite Model (Temperature 5 in MDU, but *.tim file)
                if (temperatureProcessNumber == 5 && modelDefinition.HeatFluxModel.GriddedHeatFluxFilePath == null)
                {
                    ExtForceFileItem extForceFileItem =
                        GetExistingForceFileItemOrNull(modelDefinition.HeatFluxModel.MeteoData)
                        ??
                        new ExtForceFileItem(modelDefinition.HeatFluxModel.ContainsSolarRadiation
                                                 ? ExtForceQuantNames.MeteoDataWithRadiation
                                                 : ExtForceQuantNames.MeteoData)
                        {
                            FileName = modelDefinition.ModelName + FileConstants.MeteoFileExtension,
                            FileType = ExtForceQuantNames.FileTypes.Uniform,
                            Method = 1,
                            Operand = ExtForceQuantNames.OperatorToStringMapping[Operator.Overwrite]
                        };

                    if (WriteToDisk)
                    {
                        string path = GetOtherFilePathInSameDirectory(extFilePath, extForceFileItem.FileName);
                        new TimFile().Write(path, modelDefinition.HeatFluxModel.MeteoData,
                                            (DateTime)modelDefinition.GetModelProperty(KnownProperties.RefDate).Value);
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
                        string newPath = Path.Combine(Path.GetDirectoryName(extFilePath), extForceFileItem.FileName);

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

        private ExtForceFileItem WriteSourceAndSinkData(SourceAndSink sourceAndSink,
                                                        DateTime referenceTime,
                                                        ExtForceFileItem existingExtForceFileItem,
                                                        WaterFlowFMModelDefinition modelDefinition)
        {
            ExtForceFileItem extForceFileItem = existingExtForceFileItem ??
                                                new ExtForceFileItem(ExtForceQuantNames.SourceAndSink)
                                                {
                                                    FileName = ExtForceFileHelper.GetPliFileName(sourceAndSink),
                                                    FileType = ExtForceQuantNames.FileTypes.PolyTim,
                                                    Method = 1,
                                                    Operand = ExtForceQuantNames.OperatorToStringMapping[
                                                        Operator.Overwrite]
                                                };

            if (sourceAndSink.Area > 0)
            {
                extForceFileItem.ModelData[areaKey] = sourceAndSink.Area;
            }

            ExtForceFileHelper.AddSuffixInCaseOfDuplicateFile(extForceFileItem);

            if (WriteToDisk)
            {
                string directory = Path.GetDirectoryName(extFilePath);
                string pliFilePath = Path.Combine(directory, extForceFileItem.FileName);

                new PliFile<Feature2D>().Write(pliFilePath, new EventedList<Feature2D> { sourceAndSink.Feature });
                string dataFilePath = Path.ChangeExtension(pliFilePath, ExtForceQuantNames.TimFileExtension);

                IFunction originalFunction = sourceAndSink.Function;
                if (originalFunction == null)
                {
                    return extForceFileItem;
                }

                var function = (IFunction)originalFunction.Clone(true);

                RemoveDisabledComponentsFromSourceAndSink(sourceAndSink, modelDefinition, function);

                new TimFile().Write(dataFilePath, function, referenceTime);
            }

            return extForceFileItem;
        }

        private ExtForceFileItem WriteInitialConditionsSamples(string extForceFileQuantityName,
                                                               ImportSamplesSpatialOperation importSamplesOperation,
                                                               ExtForceFileItem existingExtForceFileItem,
                                                               string prefix = null)
        {
            string targetDirectory = Path.GetDirectoryName(Path.GetFullPath(extFilePath));
            if (WriteToDisk)
            {
                if (Path.GetDirectoryName(importSamplesOperation.FilePath) != targetDirectory)
                {
                    try
                    {
                        importSamplesOperation.SwitchToDirectory(targetDirectory);
                        if (existingExtForceFileItem != null)
                        {
                            existingExtForceFileItem.FileName =
                                targetDirectory != null
                                    ? importSamplesOperation.FilePath.Replace(targetDirectory + "\\", "")
                                    : importSamplesOperation.FilePath;
                        }
                    }
                    catch (Exception e)
                    {
                        log.Warn("Unable to import samples " + e.Message);
                    }
                }
            }

            string quantityName = prefix != null ? prefix + extForceFileQuantityName : extForceFileQuantityName;
            ExtForceFileItem extForceFileItem = existingExtForceFileItem ?? new ExtForceFileItem(quantityName)
            {
                FileName =
                    targetDirectory != null
                        ? importSamplesOperation.FilePath.Replace(
                            targetDirectory + "\\", "")
                        : importSamplesOperation.FilePath,
                FileType = GetSpatialOperationFileType(importSamplesOperation),
                Method = GetImportSamplesSpatialOperationMethod(importSamplesOperation)
            };
            if (importSamplesOperation.InterpolationMethod == SpatialInterpolationMethod.Averaging)
            {
                extForceFileItem.ModelData[averagingTypeKey] =
                    (int)importSamplesOperation.AveragingMethod;
                extForceFileItem.ModelData[relSearchCellSizeKey] =
                    importSamplesOperation.RelativeSearchCellSize;
            }

            extForceFileItem.Enabled = importSamplesOperation.Enabled;
            extForceFileItem.Operand = ExtForceQuantNames.OperatorToStringMapping[Operator.Overwrite];

            return extForceFileItem;
        }

        private ExtForceFileItem WriteInitialConditionsUnsupported(string quantity, SampleSpatialOperation operation,
                                                                   string prefix = null)
        {
            string quantityName = prefix != null ? prefix + quantity : quantity;
            var forceFileItem = new ExtForceFileItem(quantityName)
            {
                FileName = MakeXyzFileName(quantity),
                FileType = ExtForceQuantNames.FileTypes.Triangulation,
                Method = GetAddSamplesMethod(),
                Enabled = operation.Enabled,
                Operand = ExtForceQuantNames.OperatorToStringMapping[Operator.Overwrite],
            };
            forceFileItem.ModelData[averagingTypeKey] = (int)GridCellAveragingMethod.ClosestPoint;
            forceFileItem.ModelData[relSearchCellSizeKey] = 1.0;

            if (WriteToDisk)
            {
                string directoryName = Path.GetDirectoryName(extFilePath);
                if (directoryName != null)
                {
                    string xyzFilePath = Path.Combine(directoryName, forceFileItem.FileName);

                    XyzFile.Write(xyzFilePath, operation.GetPoints());
                }
                else
                {
                    throw new ArgumentException("Could not get directory name from file path" + extFilePath);
                }
            }

            return forceFileItem;
        }

        private ExtForceFileItem WriteInitialConditionsPolygon(string extForceFileQuantityName, SetValueOperation operation,
                                                               ExtForceFileItem existingExtForceFileItem = null,
                                                               string prefix = null)
        {
            string quantityName = prefix != null ? prefix + extForceFileQuantityName : extForceFileQuantityName;
            ExtForceFileItem extForceFileItem = existingExtForceFileItem ?? new ExtForceFileItem(quantityName)
            {
                FileName =
                    $"{extForceFileQuantityName}_{operation.Name.Replace(" ", "_").Replace("\t", "_")}{FileConstants.PolylineFileExtension}",
                FileType = ExtForceQuantNames.FileTypes.InsidePolygon,
                Method = GetSetValueMethod()
            };

            ExtForceFileHelper.AddSuffixInCaseOfDuplicateFile(extForceFileItem);

            Operator op = ExtForceQuantNames.OperatorMapping[operation.OperationType];

            extForceFileItem.Value = operation.Value;
            extForceFileItem.Enabled = operation.Enabled;
            extForceFileItem.Operand = ExtForceQuantNames.OperatorToStringMapping[op];

            if (WriteToDisk)
            {
                string directoryName = Path.GetDirectoryName(extFilePath);
                string polFilePath;
                if (directoryName != null)
                {
                    polFilePath = Path.Combine(directoryName, extForceFileItem.FileName);
                }
                else
                {
                    throw new ArgumentException("Could not get directory name from file path" + extFilePath);
                }

                if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                new PolFile<Feature2DPolygon>().Write(polFilePath, operation.Mask.Provider.Features.OfType<IFeature>());
            }

            return extForceFileItem;
        }

        private static int GetSetValueMethod()
        {
            return 4;
        }

        private static int GetAddSamplesMethod()
        {
            return 6;
        }

        private static int GetImportSamplesSpatialOperationMethod(ImportSamplesSpatialOperation operation)
        {
            switch (operation.InterpolationMethod)
            {
                case SpatialInterpolationMethod.Triangulation:
                    return 5;
                case SpatialInterpolationMethod.Averaging:
                    return 6;
                default:
                    return -1;
            }
        }

        private static int GetSpatialOperationFileType(ISpatialOperation operation)
        {
            if (operation is ImportSamplesOperation)
            {
                return 7;
            }

            if (operation is SetValueOperation)
            {
                return 10;
            }

            return -1;
        }

        private static void RemoveDisabledComponentsFromSourceAndSink(SourceAndSink sourceAndSink,
                                                                      WaterFlowFMModelDefinition modelDefinition,
                                                                      IFunction function)
        {
            if (!UseProperty(modelDefinition, KnownProperties.UseSalinity))
            {
                function.RemoveComponentByName(SourceAndSink.SalinityVariableName);
            }

            if ((HeatFluxModelType)modelDefinition.GetModelProperty(KnownProperties.Temperature).Value ==
                HeatFluxModelType.None)
            {
                function.RemoveComponentByName(SourceAndSink.TemperatureVariableName);
            }

            if (!UseProperty(modelDefinition, GuiProperties.UseMorSed))
            {
                sourceAndSink.SedimentFractionNames.ForEach(function.RemoveComponentByName);
            }

            if (!UseProperty(modelDefinition, KnownProperties.SecondaryFlow))
            {
                function.RemoveComponentByName(SourceAndSink.SecondaryFlowVariableName);
            }
        }

        private static bool UseProperty(WaterFlowFMModelDefinition modelDefinition, string useProperty)
        {
            WaterFlowFMProperty enable = modelDefinition.GetModelProperty(useProperty);
            return (bool?)enable?.Value ?? true; // default to True
        }

        private ExtForceFileItem GetExistingForceFileItemOrNull(object value)
        {
            return existingForceFileItems.Where(kvp => Equals(kvp.Value, value)).Select(kvp => kvp.Key)
                                         .FirstOrDefault();
        }

        public static string MakeXyzFileName(string quantity) => string.Join(".", quantity.Replace(" ", "_").Replace("\t", "_"), ExtForceQuantNames.XyzFileExtension);
    }
}
