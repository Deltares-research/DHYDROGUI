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
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
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
                          bool writeBoundaryConditions, bool switchTo)
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
                    WriteExtForceFileItem(extForceFileItem);
                }
            }
            finally
            {
                CloseOutputFile();
            }
        }
        private void WriteExtForceFileItem(ExtForceFileItem extForceFileItem)
        {
            WriteMandatoryExtForceFileItemData(extForceFileItem);
            WriteOptionalExtForceFileItemData(extForceFileItem);
        }

        private void WriteMandatoryExtForceFileItemData(ExtForceFileItem extForceFileItem)
        {
            WriteLine("");
            WriteLine((extForceFileItem.Enabled ? ExtForceFileConstants.QuantityKey : disabledQuantityKey) +
                      "=" + extForceFileItem.Quantity);
            WriteLine(ExtForceFileConstants.FileNameKey + "=" + extForceFileItem.FileName);
            WriteLine(ExtForceFileConstants.FileTypeKey + "=" + extForceFileItem.FileType);
            WriteLine(ExtForceFileConstants.MethodKey + "=" + extForceFileItem.Method);
            WriteLine(ExtForceFileConstants.OperandKey + "=" + extForceFileItem.Operand);
        }

        private void WriteOptionalExtForceFileItemData(ExtForceFileItem extForceFileItem)
        {
            if (!string.IsNullOrEmpty(extForceFileItem.VarName))
            {
                WriteLine(ExtForceFileConstants.VarNameKey + "=" + extForceFileItem.VarName);   
            }
            
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

        private IList<ExtForceFileItem> WriteExtForceFileSubFiles(WaterFlowFMModelDefinition modelDefinition,
                                                                  bool writeBoundaryConditions, bool switchTo)
        {
            var extForceFileItems = new List<ExtForceFileItem>();

            ExtForceFileHelper.StartWritingSubFiles(); // hack: tracks & resolves duplicate file names

            if (writeBoundaryConditions)
            {
                extForceFileItems.AddRange(WriteBoundaryConditions(modelDefinition).Distinct());
            }

            extForceFileItems.AddRange(WriteSourcesAndSinks(modelDefinition).Distinct());

            var uniqueFileNameProvider = new UniqueFileNameProvider();
            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.InitialWaterLevel,
                                                        modelDefinition.GetSpatialOperations(
                                                            WaterFlowFMModelDefinition.InitialWaterLevelDataItemName), uniqueFileNameProvider).Distinct());

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.InitialSalinity,
                                                        modelDefinition.GetSpatialOperations(
                                                            WaterFlowFMModelDefinition.InitialSalinityDataItemName), uniqueFileNameProvider).Distinct());

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.InitialSalinity,
                                                        modelDefinition.GetSpatialOperations(
                                                            WaterFlowFMModelDefinition.InitialSalinityDataItemName +
                                                            " (layer 1)"), uniqueFileNameProvider).Distinct());

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.InitialSalinityTop,
                                                        modelDefinition.GetSpatialOperations(
                                                            WaterFlowFMModelDefinition.InitialSalinityDataItemName +
                                                            " (layer 2)"), uniqueFileNameProvider).Distinct());

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.InitialTemperature,
                                                        modelDefinition.GetSpatialOperations(
                                                            WaterFlowFMModelDefinition.InitialTemperatureDataItemName), uniqueFileNameProvider).Distinct());

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.FrictCoef,
                                                        modelDefinition.GetSpatialOperations(
                                                            WaterFlowFMModelDefinition.RoughnessDataItemName), uniqueFileNameProvider).Distinct());

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.HorEddyViscCoef,
                                                        modelDefinition.GetSpatialOperations(
                                                            WaterFlowFMModelDefinition.ViscosityDataItemName), uniqueFileNameProvider).Distinct());

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.HorEddyDiffCoef,
                                                        modelDefinition.GetSpatialOperations(
                                                            WaterFlowFMModelDefinition.DiffusivityDataItemName), uniqueFileNameProvider).Distinct());

            extForceFileItems.AddRange(WriteWindItems(modelDefinition).Distinct());

            ExtForceFileItem heatFluxModelDataItem = WriteHeatFluxModelData(modelDefinition, switchTo);
            if (heatFluxModelDataItem != null)
            {
                extForceFileItems.Add(heatFluxModelDataItem);
            }

            extForceFileItems.AddRange(WriteUnknownQuantities(modelDefinition));

            foreach (string tracerName in modelDefinition.InitialTracerNames)
            {
                extForceFileItems.AddRange(
                    WriteSpatialData(ExtForceQuantNames.InitialTracerPrefix + tracerName,
                                     modelDefinition.GetSpatialOperations(tracerName), uniqueFileNameProvider)
                        .Distinct());
            }

            /* DELFT3DFM-1112
             * This is only meant for SedimentConcentration */
            IEnumerable<string> sedimentConcentrationSpatiallyVarying =
                modelDefinition.InitialSpatiallyVaryingSedimentPropertyNames.Where(
                    sp => sp.EndsWith(ExtForceFileConstants.SedimentConcentrationPostfix));

            var logHandler = new LogHandler("import of data from the External Forcing file");

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
                                                                         spatialOperations, uniqueFileNameProvider,
                                                                         ExtForceQuantNames
                                                                             .InitialSpatialVaryingSedimentPrefix)
                                                        .Distinct().ToList();

                //Remove the postfix from the quantity (it is not accepted by the kernel)
                if (spatiallyVaryingSedimentPropertyName.EndsWith(ExtForceFileConstants.SedimentConcentrationPostfix))
                {
                    forceFileItems.ForEach(ffi => ffi.Quantity =
                                                      ffi.Quantity.Substring(
                                                          0, ffi.Quantity.Length - ExtForceFileConstants.SedimentConcentrationPostfix.Length));
                }

                extForceFileItems.AddRange(forceFileItems);
            }

            logHandler.LogReport();

            return extForceFileItems;
        }

        private IEnumerable<ExtForceFileItem> WriteUnknownQuantities(WaterFlowFMModelDefinition modelDefinition)
        {
            foreach (KeyValuePair<IUnsupportedFileBasedExtForceFileItem, ExtForceFileItem> unknownQuantitiesItem in
                ExtForceFileItemFactory.GetUnknownQuantitiesItems(modelDefinition))
            {
                ExtForceFileItem extForceFileItem = unknownQuantitiesItem.Value;
                string relativeFilePath = extForceFileItem.FileName;
                string targetPath = Path.Combine(GetDirectoryName(), relativeFilePath);
                unknownQuantitiesItem.Key.CopyTo(targetPath);

                yield return extForceFileItem;
            }
        }

        private IEnumerable<ExtForceFileItem> WriteSourcesAndSinks(WaterFlowFMModelDefinition modelDefinition)
        {
            IDictionary<SourceAndSink, ExtForceFileItem> sourceAndSinkItemsToWrite =
                ExtForceFileItemFactory.GetSourceAndSinkItems(modelDefinition, PolyLineForceFileItems);

            var referenceTime = modelDefinition.GetReferenceDateAsDateTime();

            foreach (KeyValuePair<SourceAndSink, ExtForceFileItem> sourceAndSink in sourceAndSinkItemsToWrite)
            {
                WriteSourceAndSinkData(sourceAndSink, referenceTime, modelDefinition);
                yield return sourceAndSink.Value;
            }
        }

        private void WriteSourceAndSinkData(KeyValuePair<SourceAndSink, ExtForceFileItem> sourceAndSinkFileItem,
                                            DateTime referenceTime,
                                            WaterFlowFMModelDefinition modelDefinition)
        {
            ExtForceFileItem extForceFileItem = sourceAndSinkFileItem.Value;
            SourceAndSink sourceAndSink = sourceAndSinkFileItem.Key;

            string directory = GetDirectoryName();
            string pliFilePath = Path.Combine(directory, extForceFileItem.FileName);

            new PliFile<Feature2D>().Write(pliFilePath, new EventedList<Feature2D> {sourceAndSink.Feature});
            string dataFilePath = Path.ChangeExtension(pliFilePath, ExtForceQuantNames.TimFileExtension);

            IFunction originalFunction = sourceAndSink.Function;
            if (originalFunction != null)
            {
                var function = (IFunction) originalFunction.Clone(true);

                RemoveDisabledComponentsFromSourceAndSink(sourceAndSink, modelDefinition, function);

                new TimFile().Write(dataFilePath, function, referenceTime);
            }
        }

        private IEnumerable<ExtForceFileItem> WriteBoundaryConditions(WaterFlowFMModelDefinition modelDefinition)
        {
            IDictionary<FlowBoundaryCondition, ExtForceFileItem> boundaryConditionsToWrite =
                ExtForceFileItemFactory.GetBoundaryConditionsItems(modelDefinition, PolyLineForceFileItems);

            foreach (BoundaryConditionSet boundaryConditionSet in modelDefinition.BoundaryConditionSets.Where(bcs => bcs.Feature.Name != null))
            {
                if (!boundaryConditionSet.BoundaryConditions.OfType<FlowBoundaryCondition>().Any())
                {
                    log.WarnFormat("Boundary {0} has no boundary conditions defined for flow, and cannot be written to disc.",
                                   boundaryConditionSet.Name);
                }
            }

            var referenceTime = modelDefinition.GetReferenceDateAsDateTime();

            foreach (KeyValuePair<FlowBoundaryCondition, ExtForceFileItem> boundaryCondition in boundaryConditionsToWrite)
            {
                WriteBoundaryData(boundaryCondition, referenceTime);
                yield return boundaryCondition.Value;
            }
        }

        private void WriteBoundaryData(
            KeyValuePair<FlowBoundaryCondition, ExtForceFileItem> boundaryConditionWithExtForceFileItem,
            DateTime modelReferenceDate)
        {
            ExtForceFileItem extForceFileItem = boundaryConditionWithExtForceFileItem.Value;
            FlowBoundaryCondition boundaryCondition = boundaryConditionWithExtForceFileItem.Key;

            string directory = GetDirectoryName();
            string pliFilePath = Path.Combine(directory, extForceFileItem.FileName);

            new PliFile<Feature2D>().Write(pliFilePath, new EventedList<Feature2D> {boundaryCondition.Feature});

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

                    continue;
                }

                switch (boundaryCondition.DataType)
                {
                    case BoundaryConditionDataType.HarmonicCorrection:
                    case BoundaryConditionDataType.Harmonics:
                    case BoundaryConditionDataType.AstroCorrection:
                    case BoundaryConditionDataType.AstroComponents:
                        new CmpFile().Write(dataFilePath, ExtForceFileHelper.ToHarmonicComponents(data));
                        break;
                    case BoundaryConditionDataType.TimeSeries:
                        WriteTimeSeries(modelReferenceDate, dataFilePath, data, boundaryCondition.GetDepthLayerDefinitionAtPoint(i));
                        break;
                    case BoundaryConditionDataType.Qh:
                        new QhFile().Write(dataFilePath, data);
                        break;
                    default:
                        throw new NotSupportedException("Writing boundary condition type " + boundaryCondition.DataType +
                                                        " not supported.");
                }
            }
        }

        private static void WriteTimeSeries(DateTime modelReferenceDate, string dataFilePath, IFunction data, VerticalProfileDefinition depthLayerDefinition)
        {
            if (depthLayerDefinition != null && depthLayerDefinition.Type != VerticalProfileType.Uniform)
            {
                new T3DFile().Write(dataFilePath.Replace(ExtForceQuantNames.TimFileExtension, ExtForceQuantNames.T3DFileExtension),
                                    data, depthLayerDefinition, modelReferenceDate);
            }
            else
            {
                new TimFile().Write(dataFilePath, data, modelReferenceDate);
            }
        }

        private IEnumerable<ExtForceFileItem> WriteSpatialData(string quantity, IEnumerable<ISpatialOperation> spatialOperations, UniqueFileNameProvider uniqueFileNameProvider,
                                                               string prefix = null)
        {
            IDictionary<ISpatialOperation, ExtForceFileItem> spatialDataItems =
                ExtForceFileItemFactory.GetSpatialDataItems(quantity, spatialOperations, ExistingForceFileItems, uniqueFileNameProvider,
                                                            prefix);

            foreach (KeyValuePair<ISpatialOperation, ExtForceFileItem> spatialDataItem in spatialDataItems)
            {
                ExtForceFileItem extForceFileItem = spatialDataItem.Value;

                switch (spatialDataItem.Key)
                {
                    case ImportSamplesSpatialOperation importSamplesOperation:
                        WriteInitialConditionsSamples(importSamplesOperation, extForceFileItem);
                        break;
                    case SetValueOperation polygonOperation:
                        WriteInitialConditionsPolygon(polygonOperation, extForceFileItem);
                        break;
                    case AddSamplesOperation addSamplesOperation:
                        WriteInitialConditionsUnsupported(addSamplesOperation, extForceFileItem);
                        break;
                    default:
                        throw new NotImplementedException(
                            $"Cannot serialize operation of type {spatialDataItem.Key.GetType()} to external forcings file");
                }
                
                yield return extForceFileItem;
            }
        }

        private void WriteInitialConditionsSamples(ImportSamplesOperation importSamplesOperation,
                                                   ExtForceFileItem extForceFileItem)
        {
            string targetDirectory = GetDirectoryName(Path.GetFullPath(extFilePath));

            if (GetDirectoryName(importSamplesOperation.FilePath) != targetDirectory)
            {
                try
                {
                    CopyImportSamplesOperation(importSamplesOperation, targetDirectory, extForceFileItem.FileName);
                }
                catch (Exception e)
                {
                    log.Warn("Unable to export samples " + e.Message);
                }
            }
        }

        private static void CopyImportSamplesOperation(ImportSamplesOperation operation, string targetDir, string newFileName)
        {
            string targetPath = Path.Combine(targetDir, newFileName);
            File.Copy(operation.FilePath, targetPath, true);
            operation.FilePath = targetPath;
        }

        private void WriteInitialConditionsUnsupported(SampleSpatialOperation spatialOperation,
                                                       ExtForceFileItem extForceFileItem)
        {
            string directoryName = GetDirectoryName();
            if (directoryName == null)
            {
                throw new ArgumentException("Could not get directory name from file path" + extFilePath);
            }

            string xyzFilePath = Path.Combine(directoryName, extForceFileItem.FileName);

            XyzFile.Write(xyzFilePath, spatialOperation.GetPoints());
        }

        private void WriteInitialConditionsPolygon(SpatialOperation spatialOperation,
                                                   ExtForceFileItem extForceFileItem)
        {
            string directoryName = GetDirectoryName();
            if (directoryName == null)
            {
                throw new ArgumentException("Could not get directory name from file path" + extFilePath);
            }

            if (directoryName != string.Empty && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            string polFilePath = Path.Combine(directoryName, extForceFileItem.FileName);

            new PolFile<Feature2DPolygon>().Write(polFilePath, spatialOperation.Mask.Provider.Features.OfType<IFeature>());
        }

        private IEnumerable<ExtForceFileItem> WriteWindItems(WaterFlowFMModelDefinition modelDefinition)
        {
            var referenceTime = modelDefinition.GetReferenceDateAsDateTime();
            string directory = GetDirectoryName();

            IDictionary<IWindField, ExtForceFileItem> items =
                ExtForceFileItemFactory.GetWindFieldItems(modelDefinition, ExistingForceFileItems);

            foreach (KeyValuePair<IWindField, ExtForceFileItem> windFieldItem in items)
            {
                IWindField windField = windFieldItem.Key;
                ExtForceFileItem extForceFileItem = windFieldItem.Value;

                if (windField is IFileBased fileBasedWindField)
                {
                    string newPath = Path.Combine(directory, extForceFileItem.FileName);
                    fileBasedWindField.CopyTo(newPath);
                }

                if (windField is UniformWindField)
                {
                    var timFile = new TimFile();
                    string timFilePath = Path.Combine(directory, extForceFileItem.FileName);
                    timFile.Write(timFilePath, windField.Data, referenceTime);
                }

                yield return extForceFileItem;
            }
        }

        private ExtForceFileItem WriteHeatFluxModelData(WaterFlowFMModelDefinition modelDefinition,
                                                        bool switchTo)
        {
            ExtForceFileItem extForceFileItem = null;
            try
            {
                HeatFluxModel heatFluxModel = modelDefinition.HeatFluxModel;

                extForceFileItem = ExtForceFileItemFactory.GetHeatFluxModelItem(heatFluxModel, modelDefinition.ModelName,
                                                                                ExistingForceFileItems);

                if (heatFluxModel.GriddedHeatFluxFilePath != null)
                {
                    if (extForceFileItem == null)
                    {
                        throw new InvalidOperationException("heat flux model was not correctly imported");
                    }

                    string newPath = Path.Combine(GetDirectoryName(), extForceFileItem.FileName);
                    heatFluxModel.CopyTo(newPath, switchTo);
                }
                else
                {
                    if (extForceFileItem != null)
                    {
                        string path = GetOtherFilePathInSameDirectory(extFilePath, extForceFileItem.FileName);
                        new TimFile().Write(path, heatFluxModel.MeteoData,
                                            modelDefinition.GetReferenceDateAsDateTime());
                    }
                }

                return extForceFileItem;
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is InvalidOperationException ||
                                       ex is ArgumentException || ex is PathTooLongException ||
                                       ex is UnauthorizedAccessException || ex is DirectoryNotFoundException ||
                                       ex is IOException || ex is SecurityException)
            {
                log.ErrorFormat("Error during writing the heat flux model: {0}", ex.Message);
                return extForceFileItem;
            }
        }

        private static void RemoveDisabledComponentsFromSourceAndSink(SourceAndSink sourceAndSink,
                                                                      WaterFlowFMModelDefinition modelDefinition,
                                                                      IFunction function)
        {
            if (!UseProperty(modelDefinition, KnownProperties.UseSalinity))
            {
                function.RemoveComponentByName(SourceSinkVariableInfo.SalinityVariableName);
            }

            if ((HeatFluxModelType) modelDefinition.GetModelProperty(KnownProperties.Temperature).Value ==
                HeatFluxModelType.None)
            {
                function.RemoveComponentByName(SourceSinkVariableInfo.TemperatureVariableName);
            }

            if (!UseProperty(modelDefinition, GuiProperties.UseMorSed))
            {
                sourceAndSink.SedimentFractionNames.ForEach(function.RemoveComponentByName);
            }

            if (!UseProperty(modelDefinition, KnownProperties.SecondaryFlow))
            {
                function.RemoveComponentByName(SourceSinkVariableInfo.SecondaryFlowVariableName);
            }
        }

        private static bool UseProperty(WaterFlowFMModelDefinition modelDefinition, string useProperty)
        {
            WaterFlowFMProperty enable = modelDefinition.GetModelProperty(useProperty);
            return (bool?) enable?.Value ?? true; // default to True
        }

        private string GetDirectoryName(string directory = null)
        {
            return Path.GetDirectoryName(directory ?? extFilePath);
        }
    }
}