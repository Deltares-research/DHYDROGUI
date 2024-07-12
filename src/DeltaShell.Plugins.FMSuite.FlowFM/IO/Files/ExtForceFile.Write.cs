using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security;
using DelftTools.Functions;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using Deltares.Infrastructure.IO;
using Deltares.Infrastructure.Logging;
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
using DHYDRO.Common.IO.ExtForce;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;
using SharpMap;
using SharpMap.Api.SpatialOperations;
using SharpMap.SpatialOperations;
using EnumerableExtensions = DelftTools.Utils.Collections.EnumerableExtensions;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public partial class ExtForceFile
    {
        private bool switchToNewPath;
        private bool writeBoundaryData;
        
        /// <summary>
        /// Writes the model definition external forcings to file.
        /// </summary>
        /// <param name="filePath"> File path </param>
        /// <param name="referenceFilePath">
        /// Path to which the data file references in the external forcings file are relative to.
        /// In practice, can be either the MDU file or the external forcings file.
        /// </param>
        /// <param name="definition"> External forcings data </param>
        /// <param name="writeBoundaryConditions"> Whether we are writing boundary conditions. </param>
        /// <param name="switchTo"> Whether the path of the referenced files should be switched to the new file location. </param>
        public void Write(string filePath, string referenceFilePath, WaterFlowFMModelDefinition definition, bool writeBoundaryConditions, bool switchTo)
        {
            extFilePath = filePath;
            extSubFilesReferenceFilePath = referenceFilePath;
            writeBoundaryData = writeBoundaryConditions;
            switchToNewPath = switchTo;
            modelDefinition = definition;
            
            Write();
        }

        private void Write()
        {
            IList<ExtForceData> extForceFileItems = WriteExtForceFileSubFiles();

            if (extForceFileItems.Any())
            {
                CreateExtForceFileDirectory();
                WriteExtForceFile(new ExtForceFileData(extForceFileItems));
            }
            else
            {
                FileUtils.DeleteIfExists(extFilePath);
                modelDefinition.GetModelProperty(KnownProperties.ExtForceFile).SetValueFromString(string.Empty);
            }
        }

        private void CreateExtForceFileDirectory()
        {
            string directory = fileSystem.Path.GetDirectoryName(extFilePath);

            if (!string.IsNullOrEmpty(directory))
            {
                fileSystem.CreateDirectoryIfNotExists(directory);
            }
        }
        
        private void WriteExtForceFile(ExtForceFileData extForceFileData)
        {
            using (FileSystemStream stream = fileSystem.File.Open(extFilePath, FileMode.Create))
            {
                var formatter = new ExtForceFileFormatter();
                formatter.Format(extForceFileData, stream);
            }
        }

        private IList<ExtForceData> WriteExtForceFileSubFiles()
        {
            var extForceFileItems = new List<ExtForceData>();

            ExtForceFileHelper.StartWritingSubFiles(); // hack: tracks & resolves duplicate file names

            if (writeBoundaryData)
            {
                extForceFileItems.AddRange(WriteBoundaryConditions().Distinct());
            }

            extForceFileItems.AddRange(WriteSourcesAndSinks().Distinct());

            var uniqueFileNameProvider = new UniqueFileNameProvider();

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.InitialSalinity,
                                                        modelDefinition.GetSpatialOperations(
                                                            WaterFlowFMModelDefinition.InitialSalinityDataItemName), uniqueFileNameProvider).Distinct());

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.InitialTemperature,
                                                        modelDefinition.GetSpatialOperations(
                                                            WaterFlowFMModelDefinition.InitialTemperatureDataItemName), uniqueFileNameProvider).Distinct());

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.HorEddyViscCoef,
                                                        modelDefinition.GetSpatialOperations(
                                                            WaterFlowFMModelDefinition.ViscosityDataItemName), uniqueFileNameProvider).Distinct());

            extForceFileItems.AddRange(WriteSpatialData(ExtForceQuantNames.HorEddyDiffCoef,
                                                        modelDefinition.GetSpatialOperations(
                                                            WaterFlowFMModelDefinition.DiffusivityDataItemName), uniqueFileNameProvider).Distinct());

            extForceFileItems.AddRange(WriteWindItems().Distinct());

            ExtForceData heatFluxModelDataItem = WriteHeatFluxModelData();
            if (heatFluxModelDataItem != null)
            {
                extForceFileItems.Add(heatFluxModelDataItem);
            }
            
            extForceFileItems.AddRange(WriteVelocityItems());

            extForceFileItems.AddRange(WriteUnknownQuantities());

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
                    sp => sp.EndsWith(ExtForceQuantNames.SedimentConcentrationPostfix));

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

                List<ExtForceData> forceFileItems = WriteSpatialData(spatiallyVaryingSedimentPropertyName,
                                                                         spatialOperations, uniqueFileNameProvider,
                                                                         ExtForceQuantNames
                                                                             .InitialSpatialVaryingSedimentPrefix)
                                                        .Distinct().ToList();

                //Remove the postfix from the quantity (it is not accepted by the kernel)
                if (spatiallyVaryingSedimentPropertyName.EndsWith(ExtForceQuantNames.SedimentConcentrationPostfix))
                {
                    forceFileItems.ForEach(ffi => ffi.Quantity =
                                                      ffi.Quantity.Substring(
                                                          0, ffi.Quantity.Length - ExtForceQuantNames.SedimentConcentrationPostfix.Length));
                }

                extForceFileItems.AddRange(forceFileItems);
            }

            logHandler.LogReport();

            return extForceFileItems;
        }

        private IEnumerable<ExtForceData> WriteVelocityItems()
        {
            if (modelDefinition.InitialVelocityX.HasData)
            {
                ExtForceData extForceFileItem = ExtForceFileItemFactory.GetSamplesItem(modelDefinition.InitialVelocityX,
                                                                                           ExtForceQuantNames.initialVelocityXQuantity, 
                                                                                           ExistingForceFileItems);
                WriteInitialVelocity(modelDefinition.InitialVelocityX, extForceFileItem.FileName);
                yield return extForceFileItem;
            }

            if (modelDefinition.InitialVelocityY.HasData)
            {
                ExtForceData extForceFileItem = ExtForceFileItemFactory.GetSamplesItem(modelDefinition.InitialVelocityY,
                                                                                           ExtForceQuantNames.initialVelocityYQuantity, 
                                                                                           ExistingForceFileItems);
                WriteInitialVelocity(modelDefinition.InitialVelocityY, extForceFileItem.FileName);
                yield return extForceFileItem;
            }
        }

        private void WriteInitialVelocity(Samples initialVelocity, string fileName)
        {
            string path = GetOtherFilePathInSameDirectory(extSubFilesReferenceFilePath, fileName);
            XyzFile.Write(path, initialVelocity.PointValues);
        }
        
        private IEnumerable<ExtForceData> WriteUnknownQuantities()
        {
            foreach (KeyValuePair<IUnsupportedFileBasedExtForceFileItem, ExtForceData> unknownQuantitiesItem in
                ExtForceFileItemFactory.GetUnknownQuantitiesItems(modelDefinition))
            {
                ExtForceData extForceFileItem = unknownQuantitiesItem.Value;

                string targetPath = GetOtherFilePathInSameDirectory(extSubFilesReferenceFilePath, extForceFileItem.FileName);
                unknownQuantitiesItem.Key.CopyTo(targetPath);
                
                if (switchToNewPath)
                {
                    unknownQuantitiesItem.Key.SwitchTo(targetPath);
                }

                yield return extForceFileItem;
            }
        }

        private IEnumerable<ExtForceData> WriteSourcesAndSinks()
        {
            IDictionary<SourceAndSink, ExtForceData> sourceAndSinkItemsToWrite =
                ExtForceFileItemFactory.GetSourceAndSinkItems(modelDefinition, PolyLineForceFileItems);

            var referenceTime = modelDefinition.GetReferenceDateAsDateTime();

            foreach (KeyValuePair<SourceAndSink, ExtForceData> sourceAndSink in sourceAndSinkItemsToWrite)
            {
                WriteSourceAndSinkData(sourceAndSink, referenceTime);
                yield return sourceAndSink.Value;
            }
        }

        private void WriteSourceAndSinkData(KeyValuePair<SourceAndSink, ExtForceData> sourceAndSinkFileItem, DateTime referenceTime)
        {
            ExtForceData extForceFileItem = sourceAndSinkFileItem.Value;
            SourceAndSink sourceAndSink = sourceAndSinkFileItem.Key;

            string pliFilePath = GetOtherFilePathInSameDirectory(extSubFilesReferenceFilePath, extForceFileItem.FileName);

            new PliFile<Feature2D>().Write(pliFilePath, new EventedList<Feature2D> {sourceAndSink.Feature});
            string dataFilePath = Path.ChangeExtension(pliFilePath, ExtForceQuantNames.TimFileExtension);

            IFunction originalFunction = sourceAndSink.Function;
            if (originalFunction != null)
            {
                var function = (IFunction) originalFunction.Clone(true);

                RemoveDisabledComponentsFromSourceAndSink(sourceAndSink, function);

                new TimFile().Write(dataFilePath, function, referenceTime);
            }
        }

        private IEnumerable<ExtForceData> WriteBoundaryConditions()
        {
            IDictionary<FlowBoundaryCondition, ExtForceData> boundaryConditionsToWrite =
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

            foreach (KeyValuePair<FlowBoundaryCondition, ExtForceData> boundaryCondition in boundaryConditionsToWrite)
            {
                WriteBoundaryData(boundaryCondition, referenceTime);
                yield return boundaryCondition.Value;
            }
        }

        private void WriteBoundaryData(
            KeyValuePair<FlowBoundaryCondition, ExtForceData> boundaryConditionWithExtForceFileItem,
            DateTime modelReferenceDate)
        {
            ExtForceData extForceFileItem = boundaryConditionWithExtForceFileItem.Value;
            FlowBoundaryCondition boundaryCondition = boundaryConditionWithExtForceFileItem.Key;

            string pliFilePath = GetOtherFilePathInSameDirectory(extSubFilesReferenceFilePath, extForceFileItem.FileName);

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

        private IEnumerable<ExtForceData> WriteSpatialData(string quantity, IEnumerable<ISpatialOperation> spatialOperations, UniqueFileNameProvider uniqueFileNameProvider,
                                                               string prefix = null)
        {
            IDictionary<ISpatialOperation, ExtForceData> spatialDataItems =
                ExtForceFileItemFactory.GetSpatialDataItems(quantity, spatialOperations, ExistingForceFileItems, uniqueFileNameProvider,
                                                            prefix);

            foreach (KeyValuePair<ISpatialOperation, ExtForceData> spatialDataItem in spatialDataItems)
            {
                ExtForceData extForceFileItem = spatialDataItem.Value;

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

        private void WriteInitialConditionsSamples(ImportSamplesOperation importSamplesOperation, ExtForceData extForceFileItem)
        {
            string targetPath = GetOtherFilePathInSameDirectory(extSubFilesReferenceFilePath, extForceFileItem.FileName);
            FileUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(targetPath));
            
            importSamplesOperation.CopyTo(targetPath, switchToNewPath);
        }

        private void WriteInitialConditionsUnsupported(SampleSpatialOperation spatialOperation, ExtForceData extForceFileItem)
        {
            string xyzFilePath = GetOtherFilePathInSameDirectory(extSubFilesReferenceFilePath, extForceFileItem.FileName);
            FileUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(xyzFilePath));

            XyzFile.Write(xyzFilePath, spatialOperation.GetPoints());
        }

        private void WriteInitialConditionsPolygon(SpatialOperation spatialOperation, ExtForceData extForceFileItem)
        {
            string polFilePath = GetOtherFilePathInSameDirectory(extSubFilesReferenceFilePath, extForceFileItem.FileName);
            FileUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(polFilePath));
            
            new PolFile<Feature2DPolygon>().Write(polFilePath, spatialOperation.Mask.Provider.Features.OfType<IFeature>());
        }
        
        private IEnumerable<ExtForceData> WriteWindItems()
        {
            var referenceTime = modelDefinition.GetReferenceDateAsDateTime();

            IDictionary<IWindField, ExtForceData> items =
                ExtForceFileItemFactory.GetWindFieldItems(modelDefinition, ExistingForceFileItems);

            foreach (KeyValuePair<IWindField, ExtForceData> windFieldItem in items)
            {
                IWindField windField = windFieldItem.Key;
                ExtForceData extForceFileItem = windFieldItem.Value;

                if (windField is IFileBased fileBasedWindField)
                {
                    string newPath = GetOtherFilePathInSameDirectory(extSubFilesReferenceFilePath, extForceFileItem.FileName);
                    fileBasedWindField.CopyTo(newPath);

                    if (switchToNewPath)
                    {
                        fileBasedWindField.SwitchTo(newPath);
                    }
                }

                if (windField is UniformWindField)
                {
                    var timFile = new TimFile();
                    string timFilePath = GetOtherFilePathInSameDirectory(extSubFilesReferenceFilePath, extForceFileItem.FileName);
                    timFile.Write(timFilePath, windField.Data, referenceTime);
                }

                yield return extForceFileItem;
            }
        }

        private ExtForceData WriteHeatFluxModelData()
        {
            ExtForceData extForceFileItem = null;
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

                    string newPath = GetOtherFilePathInSameDirectory(extSubFilesReferenceFilePath, extForceFileItem.FileName);
                    heatFluxModel.CopyTo(newPath, switchToNewPath);
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

        private void RemoveDisabledComponentsFromSourceAndSink(SourceAndSink sourceAndSink, IFunction function)
        {
            if (!UseProperty(KnownProperties.UseSalinity))
            {
                function.RemoveComponentByName(SourceSinkVariableInfo.SalinityVariableName);
            }

            if ((HeatFluxModelType) modelDefinition.GetModelProperty(KnownProperties.Temperature).Value ==
                HeatFluxModelType.None)
            {
                function.RemoveComponentByName(SourceSinkVariableInfo.TemperatureVariableName);
            }

            if (!UseProperty(GuiProperties.UseMorSed))
            {
                EnumerableExtensions.ForEach(sourceAndSink.SedimentFractionNames, function.RemoveComponentByName);
            }

            if (!UseProperty(KnownProperties.SecondaryFlow))
            {
                function.RemoveComponentByName(SourceSinkVariableInfo.SecondaryFlowVariableName);
            }
        }

        private bool UseProperty(string useProperty)
        {
            WaterFlowFMProperty enable = modelDefinition.GetModelProperty(useProperty);
            return (bool?) enable?.Value ?? true; // default to True
        }
    }
}