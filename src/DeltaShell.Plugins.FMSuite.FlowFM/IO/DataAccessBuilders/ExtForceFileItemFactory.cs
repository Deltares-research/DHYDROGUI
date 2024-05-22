using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Guards;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.SpatialOperations;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders
{
    /// <summary>
    /// Provides methods to create <see cref="ExtForceFileItem"/>.
    /// </summary>
    public static class ExtForceFileItemFactory
    {
        /// <summary>
        /// Gets the collection of <see cref="ExtForceFileItem"/> based of the specified <paramref name="modelDefinition"/>.
        /// </summary>
        /// <param name="modelDefinition">The model definition.</param>
        /// <param name="writeBoundaryConditions"> Whether or not the boundary conditions should be written.</param>
        /// <param name="polyLineForceFileItems">The poly line external force file items.</param>
        /// <param name="existingForceFileItems">The existing external force file items.</param>
        /// <returns>
        /// A collection of <see cref="ExtForceFileItem"/> from the specified <paramref name="modelDefinition"/>.
        /// </returns>
        public static IEnumerable<ExtForceFileItem> GetItems(WaterFlowFMModelDefinition modelDefinition,
                                                             bool writeBoundaryConditions,
                                                             IDictionary<IFeatureData, ExtForceFileItem> polyLineForceFileItems,
                                                             IDictionary<ExtForceFileItem, object> existingForceFileItems)
        {
            var items = new List<ExtForceFileItem>();

            ExtForceFileHelper.StartWritingSubFiles();

            if (writeBoundaryConditions)
            {
                items.AddRange(GetBoundaryConditionsItems(modelDefinition, polyLineForceFileItems).Values);
            }

            var uniqueFileNameProvider = new UniqueFileNameProvider();
            items.AddRange(GetSourceAndSinkItems(modelDefinition, polyLineForceFileItems).Values);
            items.AddRange(GetSpatialDataItems(ExtForceQuantNames.InitialWaterLevel, modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName), existingForceFileItems, uniqueFileNameProvider).Values);
            items.AddRange(GetSpatialDataItems(ExtForceQuantNames.InitialSalinity, modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.InitialSalinityDataItemName), existingForceFileItems, uniqueFileNameProvider).Values);
            items.AddRange(GetSpatialDataItems(ExtForceQuantNames.InitialSalinity, modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.InitialSalinityDataItemName), existingForceFileItems, uniqueFileNameProvider, " (layer 1)").Values);
            items.AddRange(GetSpatialDataItems(ExtForceQuantNames.InitialSalinityTop, modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.InitialSalinityDataItemName), existingForceFileItems, uniqueFileNameProvider, " (layer 2)").Values);
            items.AddRange(GetSpatialDataItems(ExtForceQuantNames.InitialTemperature, modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.InitialTemperatureDataItemName), existingForceFileItems, uniqueFileNameProvider).Values);
            items.AddRange(GetSpatialDataItems(ExtForceQuantNames.FrictCoef, modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.RoughnessDataItemName), existingForceFileItems, uniqueFileNameProvider).Values);
            items.AddRange(GetSpatialDataItems(ExtForceQuantNames.HorEddyViscCoef, modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.ViscosityDataItemName), existingForceFileItems, uniqueFileNameProvider).Values);
            items.AddRange(GetSpatialDataItems(ExtForceQuantNames.HorEddyDiffCoef, modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.DiffusivityDataItemName), existingForceFileItems, uniqueFileNameProvider).Values);

            items.AddRange(GetWindFieldItems(modelDefinition, existingForceFileItems).Values);

            ExtForceFileItem heatFluxModelItem = GetHeatFluxModelItem(modelDefinition.HeatFluxModel, modelDefinition.ModelName, existingForceFileItems);
            if (heatFluxModelItem != null)
            {
                items.Add(heatFluxModelItem);
            }

            items.AddRange(GetUnknownQuantitiesItems(modelDefinition).Values);

            foreach (string tracerName in modelDefinition.InitialTracerNames)
            {
                items.AddRange(GetSpatialDataItems($"{ExtForceQuantNames.InitialTracerPrefix}{tracerName}", modelDefinition.GetSpatialOperations(tracerName), existingForceFileItems, uniqueFileNameProvider).Values);
            }

            /* DELFT3DFM-1112
             * This is only meant for SedimentConcentration */
            IEnumerable<string> sedimentConcentrationSpatiallyVarying =
                modelDefinition.InitialSpatiallyVaryingSedimentPropertyNames.Where(
                    sp => sp.EndsWith(ExtForceFileConstants.SedimentConcentrationPostfix));
            foreach (string spatiallyVaryingSedimentPropertyName in sedimentConcentrationSpatiallyVarying)
            {
                IList<ISpatialOperation> spatialOperations =
                    modelDefinition.GetSpatialOperations(spatiallyVaryingSedimentPropertyName);
                if (spatialOperations?.All(s => s is ImportSamplesSpatialOperation ||
                                                s is AddSamplesOperation) != true)
                {
                    continue;
                }

                List<ExtForceFileItem> forceFileItems =
                    GetSpatialDataItems(spatiallyVaryingSedimentPropertyName, spatialOperations, existingForceFileItems, uniqueFileNameProvider,
                                        ExtForceQuantNames.InitialSpatialVaryingSedimentPrefix).Values.ToList();

                //Remove the postfix from the quantity (it is not accepted by the kernel)
                if (spatiallyVaryingSedimentPropertyName.EndsWith(ExtForceFileConstants.SedimentConcentrationPostfix))
                {
                    forceFileItems.ForEach(ffi => ffi.Quantity =
                                                      ffi.Quantity.Substring(
                                                          0, ffi.Quantity.Length - ExtForceFileConstants.SedimentConcentrationPostfix.Length));
                }

                items.AddRange(forceFileItems);
            }

            return items.Distinct().ToArray();
        }

        /// <summary>
        /// Creates the mapping with each <see cref="FlowBoundaryCondition"/> from the specified <paramref name="modelDefinition"/>
        /// and their corresponding created <see cref="ExtForceFileItem"/>.
        /// </summary>
        /// <param name="modelDefinition">The model definition.</param>
        /// <param name="polyLineForceFileItems">The poly line external force file items.</param>
        /// <returns>
        /// A dictionary with each <see cref="FlowBoundaryCondition"/> and their corresponding <see cref="ExtForceFileItem"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="modelDefinition"/> or <paramref name="polyLineForceFileItems"/> is <c>null</c>.
        /// </exception>
        public static IDictionary<FlowBoundaryCondition, ExtForceFileItem> GetBoundaryConditionsItems(
            WaterFlowFMModelDefinition modelDefinition,
            IDictionary<IFeatureData, ExtForceFileItem> polyLineForceFileItems)
        {
            if (modelDefinition == null)
            {
                throw new ArgumentNullException(nameof(modelDefinition));
            }

            if (polyLineForceFileItems == null)
            {
                throw new ArgumentNullException(nameof(polyLineForceFileItems));
            }

            var boundaryConditionsItems = new Dictionary<FlowBoundaryCondition, ExtForceFileItem>();

            foreach (BoundaryConditionSet boundaryConditionSet in
                modelDefinition.BoundaryConditionSets.Where(bcs => bcs.Feature.Name != null))
            {
                FlowBoundaryCondition[] flowBoundaryConditions =
                    boundaryConditionSet.BoundaryConditions.OfType<FlowBoundaryCondition>().ToArray();

                foreach (FlowBoundaryCondition flowBoundaryCondition in flowBoundaryConditions)
                {
                    if (!polyLineForceFileItems.TryGetValue(flowBoundaryCondition, out ExtForceFileItem matchingItem))
                    {
                        continue; //new boundary conditions shall be written by BndExtForceFile.
                    }

                    boundaryConditionsItems.Add(flowBoundaryCondition,
                                                GetFlowBoundaryConditionsItem(flowBoundaryCondition, matchingItem));
                }
            }

            return boundaryConditionsItems;
        }

        /// <summary>
        /// Creates the mapping with each <see cref="SourceAndSink"/> from the specified <paramref name="modelDefinition"/>
        /// and their corresponding created <see cref="ExtForceFileItem"/>.
        /// </summary>
        /// <param name="modelDefinition">The model definition.</param>
        /// <param name="polyLineForceFileItems">The poly line external force file items.</param>
        /// <returns>
        /// A dictionary with each <see cref="SourceAndSink"/> and their corresponding <see cref="ExtForceFileItem"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="modelDefinition"/> or <paramref name="polyLineForceFileItems"/> is <c>null</c>.
        /// </exception>
        public static IDictionary<SourceAndSink, ExtForceFileItem> GetSourceAndSinkItems(
            WaterFlowFMModelDefinition modelDefinition,
            IDictionary<IFeatureData, ExtForceFileItem> polyLineForceFileItems)
        {
            if (modelDefinition == null)
            {
                throw new ArgumentNullException(nameof(modelDefinition));
            }

            if (polyLineForceFileItems == null)
            {
                throw new ArgumentNullException(nameof(polyLineForceFileItems));
            }

            return modelDefinition.SourcesAndSinks.Where(ss => ss.Feature.Name != null).ToDictionary(
                sourceAndSink => sourceAndSink,
                sourceAndSink => GetSourceAndSinkItem(sourceAndSink, polyLineForceFileItems));
        }

        /// <summary>
        /// Creates the mapping with each <see cref="ISpatialOperation"/> from the specified <paramref name="spatialOperations"/>
        /// and their corresponding created <see cref="ExtForceFileItem"/>.
        /// </summary>
        /// <param name="quantity">The quantity name related to these <paramref name="spatialOperations"/>.</param>
        /// <param name="spatialOperations">The spatial operations.</param>
        /// <param name="existingForceFileItems">The existing external force file items.</param>
        /// <param name="uniqueFileNameProvider"> A unique file name provider </param>
        /// <param name="prefix">The optional prefix to be written before the quantity.</param>
        /// <returns>
        /// A dictionary with each <see cref="ISpatialOperation"/> and their corresponding <see cref="ExtForceFileItem"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown when an item in <paramref name="spatialOperations"/> is not an <see cref="ImportSamplesOperation"/>,
        /// <see cref="SetValueOperation"/> or <see cref="AddSamplesOperation"/>.
        /// </exception>
        public static IDictionary<ISpatialOperation, ExtForceFileItem> GetSpatialDataItems(
            string quantity, IEnumerable<ISpatialOperation> spatialOperations,
            IDictionary<ExtForceFileItem, object> existingForceFileItems,
            UniqueFileNameProvider uniqueFileNameProvider, string prefix = null)
        {
            var dictionary = new Dictionary<ISpatialOperation, ExtForceFileItem>();

            foreach (ISpatialOperation spatialOperation in spatialOperations ?? Enumerable.Empty<ISpatialOperation>())
            {
                ExtForceFileItem extForceFileItem;
                switch (spatialOperation)
                {
                    case ImportSamplesSpatialOperation importSamplesOperation:
                        extForceFileItem = GetInitialConditionsSamplesItem(
                            importSamplesOperation, quantity,
                            prefix, existingForceFileItems,
                            uniqueFileNameProvider);
                        break;
                    case SetValueOperation polygonOperation:
                        extForceFileItem = GetInitialConditionsPolygonItem(polygonOperation, quantity, prefix, existingForceFileItems, uniqueFileNameProvider);
                        break;
                    case AddSamplesOperation addSamplesOperation:
                        extForceFileItem =
                            GetInitialConditionsUnsupportedItem(addSamplesOperation, quantity, prefix, uniqueFileNameProvider);
                        break;
                    default:
                        throw new NotSupportedException(
                            $"Cannot serialize operation of type {spatialOperation.GetType()} to external forcings file");
                }

                dictionary.Add(spatialOperation, extForceFileItem);
            }

            return dictionary;
        }

        /// <summary>
        /// Creates the mapping with each <see cref="IWindField"/> from the specified <paramref name="modelDefinition"/>
        /// and their corresponding created <see cref="ExtForceFileItem"/>.
        /// </summary>
        /// <param name="modelDefinition">The model definition.</param>
        /// <param name="existingForceFileItems">The existing external force file items.</param>
        /// <returns>
        /// A dictionary with each <see cref="IWindField"/> and their corresponding <see cref="ExtForceFileItem"/>.
        /// </returns>
        public static IDictionary<IWindField, ExtForceFileItem> GetWindFieldItems(
            WaterFlowFMModelDefinition modelDefinition, IDictionary<ExtForceFileItem, object> existingForceFileItems)
        {
            var dictionary = new Dictionary<IWindField, ExtForceFileItem>();

            ExtForceFileHelper.StartWritingSubFiles();

            foreach (IWindField windField in modelDefinition.WindFields)
            {
                if (windField is IFileBased fileBasedWindField)
                {
                    ExtForceFileItem extForceFileItem = GetWindFieldItem(
                        windField,
                        Path.GetFileName(fileBasedWindField.Path),
                        existingForceFileItems);

                    dictionary.Add(windField, extForceFileItem);
                    continue;
                }

                if (windField is UniformWindField)
                {
                    string fileName = string.Join(".", ExtForceQuantNames.WindQuantityNames[windField.Quantity],
                                                  ExtForceQuantNames.TimFileExtension);
                    ExtForceFileItem extForceFileItem = GetWindFieldItem(
                        windField, fileName,
                        existingForceFileItems);

                    ExtForceFileHelper.AddSuffixInCaseOfDuplicateFile(extForceFileItem);
                    dictionary.Add(windField, extForceFileItem);
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Creates an <see cref="ExtForceFileItem"/> for the specified <paramref name="heatFluxModel"/>.
        /// </summary>
        /// <param name="heatFluxModel">The heat flux model.</param>
        /// <param name="modelName">Name of the water flow fm model.</param>
        /// <param name="existingForceFileItems">The existing force file items.</param>
        /// <returns>
        /// An <see cref="ExtForceFileItem"/> created for the specified <paramref name="heatFluxModel"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="heatFluxModel"/> or <paramref name="existingForceFileItems"/> is <c>null</c>.
        /// </exception>
        public static ExtForceFileItem GetHeatFluxModelItem(HeatFluxModel heatFluxModel, string modelName,
                                                            IDictionary<ExtForceFileItem, object> existingForceFileItems)
        {
            if (heatFluxModel == null)
            {
                throw new ArgumentNullException(nameof(heatFluxModel));
            }

            if (existingForceFileItems == null)
            {
                throw new ArgumentNullException(nameof(existingForceFileItems));
            }

            HeatFluxModelType heatFluxModelType = heatFluxModel.Type;
            if (heatFluxModelType != HeatFluxModelType.Composite)
            {
                return null;
            }

            ExtForceFileItem item;
            if (heatFluxModel.GriddedHeatFluxFilePath != null)
            {
                item = GetExistingItem(heatFluxModelType, existingForceFileItems);
            }
            else
            {
                item = GetExistingItem(heatFluxModel.MeteoData, existingForceFileItems) ??
                       new ExtForceFileItem(
                           heatFluxModel.ContainsSolarRadiation
                               ? ExtForceQuantNames.MeteoDataWithRadiation
                               : ExtForceQuantNames.MeteoData)
                       {
                           FileName = modelName + FileConstants.MeteoFileExtension,
                           FileType = ExtForceQuantNames.FileTypes.Uniform,
                           Method = 1,
                           Operand = ExtForceQuantNames.OperatorToStringMapping[
                               Operator.Overwrite]
                       };
            }

            return item;
        }

        /// <summary>
        /// Creates the mapping with each <see cref="IUnsupportedFileBasedExtForceFileItem"/> from the specified
        /// <paramref name="modelDefinition"/>
        /// and their corresponding created <see cref="ExtForceFileItem"/>.
        /// </summary>
        /// <param name="modelDefinition">The model definition.</param>
        /// <returns>
        /// A dictionary with each <see cref="IUnsupportedFileBasedExtForceFileItem"/> and their corresponding
        /// <see cref="ExtForceFileItem"/>.
        /// </returns>
        public static IDictionary<IUnsupportedFileBasedExtForceFileItem, ExtForceFileItem> GetUnknownQuantitiesItems(WaterFlowFMModelDefinition modelDefinition)
        {
            return modelDefinition.UnsupportedFileBasedExtForceFileItems
                                  .ToDictionary(i => i, i => i.UnsupportedExtForceFileItem);
        }

        private static ExtForceFileItem GetInitialConditionsSamplesItem(
            ImportSamplesSpatialOperation spatialOperation, string extForceFileQuantityName, string prefix,
            IDictionary<ExtForceFileItem, object> existingForceFileItems, UniqueFileNameProvider uniqueFileNameProvider)
        {
            if (spatialOperation == null)
            {
                throw new ArgumentNullException(nameof(spatialOperation));
            }

            if (existingForceFileItems == null)
            {
                throw new ArgumentNullException(nameof(existingForceFileItems));
            }

            ExtForceFileItem existingItem =
                existingForceFileItems.Where(item => item.Value is ImportSamplesSpatialOperation operation && operation.Name == spatialOperation.Name)
                                      .Select(item => item.Key)
                                      .FirstOrDefault();

            string quantityName = prefix != null ? prefix + extForceFileQuantityName : extForceFileQuantityName;
            string fileName = Path.GetFileName(spatialOperation.FilePath);
            ExtForceFileItem extForceFileItem = existingItem ?? new ExtForceFileItem(quantityName)
            {
                FileName = uniqueFileNameProvider.GetUniqueFileNameFor(fileName),
                FileType = 7,
                Method = GetImportSamplesSpatialOperationMethod(spatialOperation.InterpolationMethod)
            };
            extForceFileItem.Quantity = quantityName;
            
            if (spatialOperation.InterpolationMethod == SpatialInterpolationMethod.Averaging)
            {
                extForceFileItem.ModelData[ExtForceFileConstants.AveragingTypeKey] =
                    (int) spatialOperation.AveragingMethod;
                extForceFileItem.ModelData[ExtForceFileConstants.RelSearchCellSizeKey] =
                    spatialOperation.RelativeSearchCellSize;
            }

            extForceFileItem.Enabled = spatialOperation.Enabled;
            extForceFileItem.Operand = ExtForceQuantNames.OperatorToStringMapping[Operator.Overwrite];

            return extForceFileItem;
        }

        /// <summary>
        /// Get an external forcing file item for the provided <see cref="Samples"/> instance.
        /// </summary>
        /// <param name="samples"> The samples to write. </param>
        /// <param name="quantity"> The quantity for the samples. </param>
        /// <param name="existingForceFileItems"> The existing external forcing file items. </param>
        /// <returns>
        /// A new or existing external forcing file item for the provided <see cref="Samples"/> instance.
        /// </returns>
        public static ExtForceFileItem GetSamplesItem(Samples samples, string quantity, 
                                                      IDictionary<ExtForceFileItem, object> existingForceFileItems)
        {
            Ensure.NotNull(samples, nameof(samples));
            Ensure.NotNull(existingForceFileItems, nameof(existingForceFileItems));
            Ensure.NotNullOrWhiteSpace(quantity, nameof(quantity));

            ExtForceFileItem extForceFileItem = GetExistingItem(samples, existingForceFileItems) ??
                                                new ExtForceFileItem(quantity);

            extForceFileItem.FileName = samples.SourceFileName;
            extForceFileItem.FileType = 7;
            extForceFileItem.Method = GetImportSamplesSpatialOperationMethod(samples.InterpolationMethod);
            extForceFileItem.Operand = ExtForceQuantNames.OperatorToStringMapping[Operator.Overwrite];
            
            switch (samples.InterpolationMethod)
            {
                case SpatialInterpolationMethod.Averaging:
                    extForceFileItem.ModelData[ExtForceFileConstants.AveragingTypeKey] = (int)samples.AveragingMethod;
                    extForceFileItem.ModelData[ExtForceFileConstants.RelSearchCellSizeKey] = samples.RelativeSearchCellSize;
                    break;
                case SpatialInterpolationMethod.Triangulation:
                    extForceFileItem.ExtraPolTol = samples.ExtrapolationTolerance;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(samples), "Interpolation method of samples is undefined.");
            }

            return extForceFileItem;
        }

        private static ExtForceFileItem GetInitialConditionsPolygonItem(SetValueOperation spatialOperation, string extForceFileQuantityName, string prefix,
                                                                        IDictionary<ExtForceFileItem, object> existingForceFileItems, UniqueFileNameProvider uniqueFileNameProvider)
        {
            if (spatialOperation == null)
            {
                throw new ArgumentNullException(nameof(spatialOperation));
            }

            if (existingForceFileItems == null)
            {
                throw new ArgumentNullException(nameof(existingForceFileItems));
            }

            ExtForceFileItem existingItem = GetExistingItem(spatialOperation, existingForceFileItems);

            string quantityName = prefix != null ? prefix + extForceFileQuantityName : extForceFileQuantityName;
            string fileName = $"{extForceFileQuantityName}_{spatialOperation.Name}{FileConstants.PolylineFileExtension}".ReplaceSpaces();
            ExtForceFileItem extForceFileItem = existingItem ?? new ExtForceFileItem(quantityName)
            {
                FileName = uniqueFileNameProvider.GetUniqueFileNameFor(fileName),
                FileType = ExtForceQuantNames.FileTypes.InsidePolygon,
                Method = 4
            };

            ExtForceFileHelper.AddSuffixInCaseOfDuplicateFile(extForceFileItem);

            Operator op = ExtForceQuantNames.OperatorMapping[spatialOperation.OperationType];

            extForceFileItem.Value = spatialOperation.Value;
            extForceFileItem.Enabled = spatialOperation.Enabled;
            extForceFileItem.Operand = ExtForceQuantNames.OperatorToStringMapping[op];

            return extForceFileItem;
        }

        private static ExtForceFileItem GetInitialConditionsUnsupportedItem(SampleSpatialOperation spatialOperation,
                                                                            string extForceFileQuantityName, string prefix,
                                                                            UniqueFileNameProvider fileNameProvider)
        {
            if (spatialOperation == null)
            {
                throw new ArgumentNullException(nameof(spatialOperation));
            }

            string quantityName = prefix != null ? prefix + extForceFileQuantityName : extForceFileQuantityName;

            string fileName = $"{extForceFileQuantityName}{FileConstants.XyzFileExtension}".ReplaceSpaces();
            return new ExtForceFileItem(quantityName)
            {
                FileName = fileNameProvider.GetUniqueFileNameFor(fileName),
                FileType = AddSamplesDefaults.FileType,
                Method = AddSamplesDefaults.Method,
                Enabled = spatialOperation.Enabled,
                Operand = ExtForceQuantNames.OperatorToStringMapping[AddSamplesDefaults.Operand],
                ModelData =
                {
                    [ExtForceFileConstants.AveragingTypeKey] = (int) AddSamplesDefaults.AveragingType,
                    [ExtForceFileConstants.RelSearchCellSizeKey] = AddSamplesDefaults.RelSearchCellSize
                },
            };
        }

        private static ExtForceFileItem GetWindFieldItem(IWindField windField, string fileName,
                                                         IDictionary<ExtForceFileItem, object> existingForceFileItems)
        {
            return GetExistingItem(windField, existingForceFileItems) ??
                   new ExtForceFileItem(ExtForceQuantNames.WindQuantityNames[windField.Quantity])
                   {
                       FileName = fileName,
                       FileType = GetFileType(windField),
                       Method = GetMethod(windField),
                       Operand = "+"
                   };
        }

        private static ExtForceFileItem GetSourceAndSinkItem(SourceAndSink sourceAndSink,
                                                             IDictionary<IFeatureData, ExtForceFileItem> polyLineForceFileItems)
        {
            polyLineForceFileItems.TryGetValue(sourceAndSink, out ExtForceFileItem existingItem);

            ExtForceFileItem extForceFileItem = existingItem ?? new ExtForceFileItem(ExtForceQuantNames.SourceAndSink)
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

            ExtForceFileHelper.AddSuffixInCaseOfDuplicateFile(extForceFileItem);

            return extForceFileItem;
        }

        private static ExtForceFileItem GetFlowBoundaryConditionsItem(FlowBoundaryCondition flowBoundaryCondition,
                                                                      ExtForceFileItem existingItem)
        {
            existingItem.Quantity = ExtForceQuantNames.GetQuantityString(flowBoundaryCondition);
            existingItem.Offset = Math.Abs(flowBoundaryCondition.Offset) < 1e-6 ? double.NaN : flowBoundaryCondition.Offset;
            existingItem.Factor = Math.Abs(flowBoundaryCondition.Factor - 1) < 1e-6 ? double.NaN : flowBoundaryCondition.Factor;

            ExtForceFileHelper.AddSuffixInCaseOfDuplicateFile(existingItem);

            return existingItem;
        }

        private static int GetImportSamplesSpatialOperationMethod(SpatialInterpolationMethod interpolationMethod)
        {
            switch (interpolationMethod)
            {
                case SpatialInterpolationMethod.Triangulation:
                    return 5;
                case SpatialInterpolationMethod.Averaging:
                    return 6;
                default:
                    return -1;
            }
        }

        private static ExtForceFileItem GetExistingItem(object value, IDictionary<ExtForceFileItem, object> existingForceFileItems)
        {
            return existingForceFileItems.Where(item => Equals(item.Value, value))
                                         .Select(item => item.Key)
                                         .FirstOrDefault();
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

        private static string ReplaceSpaces(this string source) => source.Replace(" ", "_").Replace("\t", "_");
    }
}