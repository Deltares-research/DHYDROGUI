using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
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
            ExtFilePath = extForceFilePath;
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
            ExtFilePath = path;
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
                string targetPath = Path.Combine(Path.GetDirectoryName(ExtFilePath), relativeFilePath);
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

                yield return ExtForceFileHelper.WriteSourceAndSinkData(ExtFilePath, sourceAndSink, referenceTime,
                                                                       matchingItem, WriteToDisk, modelDefinition);
            }
        }

        private IEnumerable<ExtForceFileItem> WriteBoundaryConditions(WaterFlowFMModelDefinition modelDefinition)
        {
            var referenceTime = (DateTime)modelDefinition.GetModelProperty(KnownProperties.RefDate).Value;

            foreach (BoundaryConditionSet boundaryConditionSet in modelDefinition.BoundaryConditionSets.Where(bcs => bcs.Feature.Name != null))
            {
                List<FlowBoundaryCondition> flowBoundaryConditions = boundaryConditionSet.BoundaryConditions.OfType<FlowBoundaryCondition>().ToList();

                if (WriteToDisk && !flowBoundaryConditions.Any())
                {
                    log.WarnFormat("Boundary {0} has no boundary conditions defined for flow, and cannot be written to disc.",
                                   boundaryConditionSet.Name);
                }

                foreach (FlowBoundaryCondition flowBoundaryCondition in flowBoundaryConditions)
                {
                    if (!polyLineForceFileItems.TryGetValue(flowBoundaryCondition, out ExtForceFileItem matchingItem))
                    {
                        continue; //new boundary conditions shall be written by BndExtForceFile.
                    }

                    int index = boundaryConditionSet.BoundaryConditions
                                                    .Where(b => b.VariableName == flowBoundaryCondition.VariableName)
                                                    .ToList()
                                                    .IndexOf(flowBoundaryCondition);

                    yield return ExtForceFileHelper.WriteBoundaryData(ExtFilePath, flowBoundaryCondition, referenceTime, index,
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
                var importSamplesOperation = spatialOperation as ImportSamplesSpatialOperation;
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

                throw new NotSupportedException($"Cannot serialize operation of type {spatialOperation.GetType()} to external forcings file");
            }
        }

        public static string MakeXyzFileName(string quantity) => string.Join(".", quantity.Replace(" ", "_").Replace("\t", "_"), ExtForceQuantNames.XyzFileExtension);

        private IEnumerable<ExtForceFileItem> WriteWindItems(WaterFlowFMModelDefinition modelDefinition)
        {
            var referenceTime = (DateTime)modelDefinition.GetModelProperty(KnownProperties.RefDate).Value;
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
                        string path = GetOtherFilePathInSameDirectory(ExtFilePath, extForceFileItem.FileName);
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

        private ExtForceFileItem GetExistingForceFileItemOrNull(object value)
        {
            return existingForceFileItems.Where(kvp => Equals(kvp.Value, value)).Select(kvp => kvp.Key)
                                         .FirstOrDefault();
        }
    }
}
