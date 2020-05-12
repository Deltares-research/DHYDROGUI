using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using DelftTools.Functions;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
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
using SharpMap;
using SharpMap.Api.SpatialOperations;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public class ExtForceFileWriter
    {
        private const string sedimentConcentrationPostfix = "_SedConc";
        private const string duplicationSuffixPattern = "{0}__{1:00000}{2}";
        private static readonly ILog log = LogManager.GetLogger(typeof(ExtForceFileWriter));

        private static readonly List<string> previousPaths = new List<string>();
        private Dictionary<string, List<string>> commentBlocks;

        private List<List<string>> headingCommentBlocks;

        // items that existed in the file when the file was read
        private IDictionary<ExtForceFileItem, object> existingForceFileItems;
        private IDictionary<IFeatureData, ExtForceFileItem> polylineForceFileItems;

        private StreamWriter writer;
        private string extFilePath;
        private CultureInfo storedCurrentCulture;
        private bool fileContentHasStarted;

        private string outputFilePath;
        private bool writeToDisk;

        public ExtForceFileWriter()
        {
            existingForceFileItems = new Dictionary<ExtForceFileItem, object>();
            polylineForceFileItems = new Dictionary<IFeatureData, ExtForceFileItem>();
            
            headingCommentBlocks = new List<List<string>>();
            commentBlocks = new Dictionary<string, List<string>>();
        }

        public IEnumerable<IBoundaryCondition> ExistingBoundaryConditions =>
            polylineForceFileItems.Keys.OfType<IBoundaryCondition>();

        /// <summary>
        /// Writes the model definition external forcings to file.
        /// </summary>
        /// <param name="extForceFilePath"> File path </param>
        /// <param name="modelDefinition"> External forcings data </param>
        /// <param name="writeBoundaryConditions"> Whether we are writing boundary conditions. </param>
        /// <param name="switchTo"> Flag denoting whether to switch to the file path directory (save) </param>
        public void Write(string extForceFilePath, WaterFlowFMModelDefinition modelDefinition,
                          bool writeBoundaryConditions = true, bool switchTo = true)
        {
            extFilePath = extForceFilePath;
            writeToDisk = true;

            Write(modelDefinition, writeBoundaryConditions, switchTo);
        }

        public void Write(string extForceFilePath, WaterFlowFMModelDefinition modelDefinition, bool writeBoundaryConditions, bool switchTo,
                          IDictionary<ExtForceFileItem, object> existingForceFileItemsDictionary, HashSet<ExtForceFileItem> supportedExtForceFileItems,
                          IDictionary<IFeatureData, ExtForceFileItem> polylineForceFileItemsDictionary,
                          List<List<string>> headingCommentBlocksList, Dictionary<string, List<string>> commentBlocksDictionary)
        {
            extFilePath = extForceFilePath;
            writeToDisk = true;

            existingForceFileItems = existingForceFileItemsDictionary;
            polylineForceFileItems = polylineForceFileItemsDictionary;
            headingCommentBlocks = headingCommentBlocksList;
            commentBlocks = commentBlocksDictionary;

            Write(modelDefinition, writeBoundaryConditions, switchTo);

        }

        /// <summary>
        /// Get the data files that are references in the extForceFile.
        /// </summary>
        /// <param name="modelDefinition"> </param>
        /// <returns> A list of tuples of name and file path. </returns>
        public IEnumerable<string[]> GetFeatureDataFiles(WaterFlowFMModelDefinition modelDefinition)
        {
            StartWritingSubFiles();

            foreach (BoundaryConditionSet boundaryConditionSet in
                modelDefinition.BoundaryConditionSets.Where(bc => bc.Feature.Name != null))
            {
                foreach (FlowBoundaryCondition bc in boundaryConditionSet
                                                     .BoundaryConditions.OfType<FlowBoundaryCondition>())
                {
                    polylineForceFileItems.TryGetValue(bc, out ExtForceFileItem matchingItem);
                    List<string[]> dataFiles =
                        GetBoundaryDataFiles(bc, boundaryConditionSet, matchingItem).ToList();

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
                    GetSourceAndSinkDataFiles(sourceAndSink, matchingItem).ToList();

                foreach (string[] dataFile in dataFiles)
                {
                    yield return dataFile;
                }
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
            writeToDisk = false;
            return WriteExtForceFileSubFiles(modelDefinition, writeBoundaryConditions, switchTo);
        }

        public void WriteCommentBlock(string line)
        {
            string contentIdentifier = CreateContentIdentifier(line);
            if (commentBlocks.ContainsKey(contentIdentifier))
            {
                foreach (string commentLine in commentBlocks[contentIdentifier])
                {
                    writer.WriteLine(commentLine);
                }
            }
        }

        private static IEnumerable<string[]> GetBoundaryDataFiles(FlowBoundaryCondition boundaryCondition,
                                                                  BoundaryConditionSet boundaryCoditionSet,
                                                                  ExtForceFileItem existingExtForceFileItem = null)
        {
            string quantityName =
                ExtForceQuantNames.GetQuantityString(boundaryCondition);

            ExtForceFileItem extForceFileItem = existingExtForceFileItem ?? new ExtForceFileItem(quantityName)
            {
                FileName = ExtForceFileHelper.GetPliFileName(boundaryCondition),
                FileType = ExtForceQuantNames.FileTypes.PolyTim
            };

            AddSuffixInCaseOfDuplicateFile(extForceFileItem);

            foreach (int i in boundaryCondition.DataPointIndices)
            {
                string quantity = String.Format("{0} {1} at {2}", boundaryCondition.VariableDescription.ToLower(),
                                                boundaryCondition.DataType.GetDescription().ToLower(),
                                                boundaryCoditionSet.SupportPointNames[i]);

                string filePath;

                if (boundaryCondition.DataType == BoundaryConditionDataType.TimeSeries &&
                    !boundaryCondition.IsVerticallyUniform)
                {
                    filePath = ExtForceFileHelper.GetNumberedFilePath(extForceFileItem.FileName,
                                                                      ExtForceQuantNames.T3DFileExtension,
                                                                      i + 1);
                }
                else
                {
                    filePath = ExtForceFileHelper.GetNumberedFilePath(extForceFileItem.FileName,
                                                                      ExtForceQuantNames.ForcingToFileExtensionMapping[
                                                                          boundaryCondition.DataType], i + 1);
                }

                if (filePath == null)
                {
                    yield break; //TODO: emit warning.
                }

                yield return new[]
                {
                    quantity,
                    filePath
                };
            }
        }

        private static IEnumerable<string[]> GetSourceAndSinkDataFiles(SourceAndSink sourceAndSink,
                                                                       ExtForceFileItem existingExtForceFileItem)
        {
            const string quantityName = ExtForceQuantNames.SourceAndSink;

            ExtForceFileItem extForceFileItem = existingExtForceFileItem ?? new ExtForceFileItem(quantityName)
            {
                FileName = ExtForceFileHelper.GetPliFileName(sourceAndSink),
                FileType = ExtForceQuantNames.FileTypes.PolyTim
            };

            AddSuffixInCaseOfDuplicateFile(extForceFileItem);

            string filePath = Path.ChangeExtension(extForceFileItem.FileName, ExtForceQuantNames.TimFileExtension);

            yield return new[]
            {
                "Source/Sink",
                filePath
            };
        }

        /// <summary>
        /// Opens the file writer to a given destination.
        /// </summary>
        /// <param name="filePath">File path to write to.</param>
        /// <exception cref="UnauthorizedAccessException">Access is denied</exception>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is an empty string ("") or contains the name of a system device (com1, com2, and so on).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null.</exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
        /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters.</exception>
        /// <exception cref="IOException">path includes an incorrect or invalid syntax for file name, directory name, or volume label syntax.</exception>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        private void OpenOutputFile(string filePath)
        {
            storedCurrentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            outputFilePath = filePath;
            var directory = Path.GetDirectoryName(outputFilePath);
            if (!String.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            writer = new StreamWriter(outputFilePath);
            fileContentHasStarted = false;
        }

        private void CloseOutputFile()
        {
            Thread.CurrentThread.CurrentCulture = storedCurrentCulture;
            writer.Close();
        }

        /// <summary>
        /// Write a line of text to file.
        /// </summary>
        /// <param name="line">Line of text to be written (should not be null).</param>
        /// <exception cref="InvalidOperationException">When calling this before <see cref="OpenOutputFile"/> has been called.</exception>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        private void WriteLine(string line)
        {
            if (writer == null)
            {
                throw new InvalidOperationException("Output file not opened for writing: " + (outputFilePath ?? "(no file)"));
            }

            CheckAndProcessOutputCommentLines(line);
            writer.WriteLine(line);
        }

        private static string CreateContentIdentifier(string line)
        {
            if (line == null)
            {
                return String.Empty;
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
                modelDefinition.GetModelProperty(KnownProperties.ExtForceFile).SetValueAsString(String.Empty);
            }
        }

        private void CheckAndProcessOutputCommentLines(string line)
        {
            if (!fileContentHasStarted)
            {
                foreach (List<string> headingCommentBlock in headingCommentBlocks)
                {
                    foreach (string commentLine in headingCommentBlock)
                    {
                        writer.WriteLine(commentLine);
                    }

                    if (!String.IsNullOrEmpty(line))
                    {
                        writer.WriteLine("*");
                    }
                }

                fileContentHasStarted = true;
            }
            else
            {
                WriteCommentBlock(line);
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
                    WriteLine((extForceFileItem.Enabled ? ExtForceFileConstants.QuantityKey : ExtForceFileConstants.DisabledQuantityKey) +
                              "=" + extForceFileItem.Quantity);
                    WriteLine(ExtForceFileConstants.FileNameKey + "=" + extForceFileItem.FileName);
                    WriteLine(ExtForceFileConstants.FileTypeKey + "=" + extForceFileItem.FileType);
                    WriteLine(ExtForceFileConstants.MethodKey + "=" + extForceFileItem.Method);
                    WriteLine(ExtForceFileConstants.OperandKey + "=" + extForceFileItem.Operand);
                    if (!double.IsNaN(extForceFileItem.Value))
                    {
                        WriteLine(ExtForceFileConstants.ValueKey + "=" + extForceFileItem.Value);
                    }

                    if (!double.IsNaN(extForceFileItem.Factor))
                    {
                        WriteLine(ExtForceFileConstants.FactorKey + "=" + extForceFileItem.Factor);
                    }

                    if (!double.IsNaN(extForceFileItem.Offset))
                    {
                        WriteLine(ExtForceFileConstants.OffsetKey + "=" + extForceFileItem.Offset);
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

        private IList<ExtForceFileItem> WriteExtForceFileSubFiles(WaterFlowFMModelDefinition modelDefinition,
                                                                  bool writeBoundaryConditions, bool switchTo = true)
        {
            var extForceFileItems = new List<ExtForceFileItem>();

            StartWritingSubFiles(); // hack: tracks & resolves duplicate file names

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
            IEnumerable<string> sedimentConcentrationSpatiallyVarying =
                modelDefinition.InitialSpatiallyVaryingSedimentPropertyNames.Where(
                    sp => sp.EndsWith(sedimentConcentrationPostfix));

            LogHandler logHandler = new LogHandler("Ext force warning handler");

            foreach (string spatiallyVaryingSedimentPropertyName in sedimentConcentrationSpatiallyVarying)
            {
                IList<ISpatialOperation> spatialOperations =
                    modelDefinition.GetSpatialOperations(spatiallyVaryingSedimentPropertyName);
                if (spatialOperations?.All(s => s is ImportSamplesSpatialOperation ||
                                                s is AddSamplesOperation) != true)
                {
                    string warnMsg = String.Format(
                        Resources
                            .SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_No_spatial_operations_of_type_Import__Add_or_Value_found_for_spatially_varying_property__0___Remember_to_interpolate_them_to_generate_the_xyz_file__Otherwise_the_model_might_not_run_as_expected_,
                        spatiallyVaryingSedimentPropertyName);
                    if (spatialOperations != null)
                    {
                        warnMsg = String.Format(
                            Resources
                                .SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_Cannot_create_xyz_file_for_spatial_varying_initial_condition__0__because_it_is_a_value_spatial_operation__please_interpolate_the_operation_to_the_grid_or,
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

        private IEnumerable<ExtForceFileItem> WriteHeatFluxModelData(WaterFlowFMModelDefinition modelDefinition,
                                                                     bool switchTo = true)
        {
            var extForceFileItems = new List<ExtForceFileItem>();
            try
            {
                var temperatureProcessNumber = (int) modelDefinition.HeatFluxModel.Type;

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

                    if (writeToDisk)
                    {
                        string path = NGHSFileBase.GetOtherFilePathInSameDirectory(extFilePath, extForceFileItem.FileName);
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

                    if (writeToDisk)
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
            var referenceTime = (DateTime) modelDefinition.GetModelProperty(KnownProperties.RefDate).Value;

            foreach (SourceAndSink sourceAndSink in modelDefinition.SourcesAndSinks.Where(ss => ss.Feature.Name != null)
            )
            {
                polylineForceFileItems.TryGetValue(sourceAndSink, out ExtForceFileItem matchingItem);

                yield return WriteSourceAndSinkData(sourceAndSink, referenceTime, matchingItem, modelDefinition);
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
                extForceFileItem.ModelData[ExtForceFileConstants.AreaKey] = sourceAndSink.Area;
            }

            AddSuffixInCaseOfDuplicateFile(extForceFileItem);

            if (writeToDisk)
            {
                string directory = Path.GetDirectoryName(extFilePath);
                string pliFilePath = Path.Combine(directory, extForceFileItem.FileName);

                new PliFile<Feature2D>().Write(pliFilePath, new EventedList<Feature2D>
                {
                    sourceAndSink.Feature
                });
                string dataFilePath = Path.ChangeExtension(pliFilePath, ExtForceQuantNames.TimFileExtension);

                IFunction originalFunction = sourceAndSink.Function;
                if (originalFunction == null)
                {
                    return extForceFileItem;
                }

                var function = (IFunction) originalFunction.Clone(true);

                RemoveDisabledComponentsFromSourceAndSink(sourceAndSink, modelDefinition, function);

                new TimFile().Write(dataFilePath, function, referenceTime);
            }

            return extForceFileItem;
        }

        private static void RemoveDisabledComponentsFromSourceAndSink(SourceAndSink sourceAndSink,
                                                                      WaterFlowFMModelDefinition modelDefinition,
                                                                      IFunction function)
        {
            if (!UseProperty(modelDefinition, KnownProperties.UseSalinity))
            {
                function.RemoveComponentByName(SourceAndSink.SalinityVariableName);
            }

            if ((HeatFluxModelType) modelDefinition.GetModelProperty(KnownProperties.Temperature).Value ==
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
            return enable == null || (bool) enable.Value; // default to True
        }

        private IEnumerable<ExtForceFileItem> WriteBoundaryConditions(WaterFlowFMModelDefinition modelDefinition)
        {
            var referenceTime = (DateTime) modelDefinition.GetModelProperty(KnownProperties.RefDate).Value;

            foreach (BoundaryConditionSet boundaryConditionSet in
                modelDefinition.BoundaryConditionSets.Where(bcs => bcs.Feature.Name != null))
            {
                List<FlowBoundaryCondition> flowBoundaryConditions =
                    boundaryConditionSet.BoundaryConditions.OfType<FlowBoundaryCondition>().ToList();

                if (writeToDisk && !flowBoundaryConditions.Any())
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
                        WriteBoundaryData(flowBoundaryCondition, referenceTime, index,  matchingItem);
                }
            }
        }

        private ExtForceFileItem WriteBoundaryData(FlowBoundaryCondition boundaryCondition,
                                                   DateTime modelReferenceDate, int bcIndex,
                                                   ExtForceFileItem existingExtForceFileItem = null)
        {
            string quantityName =
                ExtForceQuantNames.GetQuantityString(boundaryCondition);

            Operator operand = bcIndex == 0 ? Operator.Overwrite : Operator.Add;

            ExtForceFileItem extForceFileItem = existingExtForceFileItem ?? new ExtForceFileItem(quantityName)
            {
                FileName = ExtForceFileHelper.GetPliFileName(boundaryCondition),
                FileType = ExtForceQuantNames.FileTypes.PolyTim,
                Method = 3,
                Operand = ExtForceQuantNames.OperatorToStringMapping[operand]
            };

            extForceFileItem.Quantity = quantityName;
            extForceFileItem.Offset = Math.Abs(boundaryCondition.Offset) < 1e-6 ? Double.NaN : boundaryCondition.Offset;
            extForceFileItem.Factor = Math.Abs(boundaryCondition.Factor - 1) < 1e-6 ? Double.NaN : boundaryCondition.Factor;

            AddSuffixInCaseOfDuplicateFile(extForceFileItem);

            if (writeToDisk)
            {
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

            return extForceFileItem;
        }

        private ExtForceFileItem GetExistingForceFileItemOrNull(object value)
        {
            return existingForceFileItems.Where(kvp => Equals(kvp.Value, value)).Select(kvp => kvp.Key)
                                         .FirstOrDefault();
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
                        WriteInitialConditionsSamples(quantity, importSamplesOperation,
                                                      existingItem, prefix);
                    continue;
                }

                var polygonOperation = spatialOperation as SetValueOperation;
                if (polygonOperation != null)
                {
                    ExtForceFileItem existingItem = GetExistingForceFileItemOrNull(spatialOperation);
                    yield return
                        WriteInitialConditionsPolygon(quantity, polygonOperation,
                                                      existingItem, prefix);

                    continue;
                }

                var addSamplesOperation = spatialOperation as AddSamplesOperation;
                if (addSamplesOperation != null)
                {
                    yield return
                        WriteInitialConditionsUnsupported(quantity, addSamplesOperation, prefix);
                    continue;
                }

                throw new NotImplementedException(
                    $"Cannot serialize operation of type {spatialOperation.GetType()} to external forcings file");
            }
        }

        private ExtForceFileItem WriteInitialConditionsPolygon(string extForceFileQuantityName,
                                                               SetValueOperation operation,
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

            AddSuffixInCaseOfDuplicateFile(extForceFileItem);

            Operator op = ExtForceQuantNames.OperatorMapping[operation.OperationType];

            extForceFileItem.Value = operation.Value;
            extForceFileItem.Enabled = operation.Enabled;
            extForceFileItem.Operand = ExtForceQuantNames.OperatorToStringMapping[op];

            if (writeToDisk)
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

                if (!String.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
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

        private ExtForceFileItem WriteInitialConditionsSamples(string extForceFileQuantityName,
                                                               ImportSamplesSpatialOperation
                                                                   importSamplesOperation,
                                                               ExtForceFileItem existingExtForceFileItem,
                                                               string prefix = null)
        {
            string targetDirectory = Path.GetDirectoryName(Path.GetFullPath(extFilePath));
            if (writeToDisk)
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
                extForceFileItem.ModelData[ExtForceFileConstants.AveragingTypeKey] =
                    (int) importSamplesOperation.AveragingMethod;
                extForceFileItem.ModelData[ExtForceFileConstants.RelSearchCellSizeKey] =
                    importSamplesOperation.RelativeSearchCellSize;
            }

            extForceFileItem.Enabled = importSamplesOperation.Enabled;
            extForceFileItem.Operand = ExtForceQuantNames.OperatorToStringMapping[Operator.Overwrite];

            return extForceFileItem;
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

        private ExtForceFileItem WriteInitialConditionsUnsupported(string quantity,
                                                                   SampleSpatialOperation operation,
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
                ModelData =
                {
                    [ExtForceFileConstants.AveragingTypeKey] = (int) GridCellAveragingMethod.ClosestPoint,
                    [ExtForceFileConstants.RelSearchCellSizeKey] = 1.0
                }
            };

            if (writeToDisk)
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

        private static int GetAddSamplesMethod()
        {
            return 6;
        }

        private static string MakeXyzFileName(string quantity)
        {
            return String.Join(".", quantity.Replace(" ", "_").Replace("\t", "_"), ExtForceQuantNames.XyzFileExtension);
        }

        private IEnumerable<ExtForceFileItem> WriteWindItems(WaterFlowFMModelDefinition modelDefinition)
        {
            var referenceTime = (DateTime) modelDefinition.GetModelProperty(KnownProperties.RefDate).Value;
            string directory = Path.GetDirectoryName(extFilePath);
            StartWritingSubFiles();

            foreach (IWindField windField in modelDefinition.WindFields)
            {
                if (windField is IFileBased fileBasedWindField)
                {
                    ExtForceFileItem extForceFileItem = GetExistingForceFileItemOrNull(windField) ??
                                                        CreateWindFieldExtForceFileItem(windField,
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
                    string fileName = String.Join(".", ExtForceQuantNames.WindQuantityNames[windField.Quantity],
                                                  ExtForceQuantNames.TimFileExtension);
                    ExtForceFileItem extForceFileItem = GetExistingForceFileItemOrNull(windField) ??
                                                        CreateWindFieldExtForceFileItem(
                                                            windField, fileName);
                    AddSuffixInCaseOfDuplicateFile(extForceFileItem);
                    var timFile = new TimFile();
                    string timFilePath = Path.Combine(directory, extForceFileItem.FileName);
                    timFile.Write(timFilePath, windField.Data, referenceTime);
                    yield return extForceFileItem;
                }
            }
        }

        private static void StartWritingSubFiles()
        {
            previousPaths.Clear();
        }

        private static void AddSuffixInCaseOfDuplicateFile(ExtForceFileItem item)
        {
            if (previousPaths.Contains(item.FileName))
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(item.FileName);
                string extension = Path.GetExtension(item.FileName);

                // search for unique filename using recognizable suffix
                var i = 2;
                while (true)
                {
                    string newFileName =
                        string.Format(duplicationSuffixPattern, fileNameWithoutExtension, i, extension);
                    if (!previousPaths.Contains(newFileName))
                    {
                        item.FileName = newFileName;
                        break;
                    }

                    i++;
                }
            }

            previousPaths.Add(item.FileName);
        }

        private static ExtForceFileItem CreateWindFieldExtForceFileItem(IWindField windField, string filePath)
        {
            return new ExtForceFileItem(ExtForceQuantNames.WindQuantityNames[windField.Quantity])
            {
                FileName = filePath,
                FileType = GetFileType(windField),
                Method = GetMethod(windField),
                Operand = "+"
            };
        }

        private static int GetFileType(IWindField windField)
        {
            if (windField is UniformWindField uniformWindField)
            {
                return uniformWindField.Components.Contains(WindComponent.Magnitude)
                           ? ExtForceQuantNames.FileTypes.UniMagDir
                           : ExtForceQuantNames.FileTypes.Uniform;
            }

            if (windField is GriddedWindField)
            {
                return windField.Quantity == WindQuantity.VelocityVectorAirPressure
                           ? ExtForceQuantNames.FileTypes.Curvi
                           : ExtForceQuantNames.FileTypes.ArcInfo;
            }

            if (windField is SpiderWebWindField)
            {
                return ExtForceQuantNames.FileTypes.SpiderWeb;
            }

            return -1;
        }

        private static int GetMethod(IWindField windField)
        {
            if (windField is UniformWindField)
            {
                return 1;
            }

            if (windField is GriddedWindField)
            {
                return windField.Quantity == WindQuantity.VelocityVectorAirPressure ? 3 : 2;
            }

            if (windField is SpiderWebWindField)
            {
                return 1;
            }

            return -1;
        }
    }
}