using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.IO;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DHYDRO.Common.IO.ExtForce;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.SpatialOperations;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders
{
    /// <summary>
    /// Provides methods to create <see cref="ExtForceData"/>.
    /// </summary>
    public static class ExtForceFileItemFactory
    {
        /// <summary>
        /// Gets the collection of <see cref="ExtForceData"/> based of the specified <paramref name="modelDefinition"/>.
        /// </summary>
        /// <param name="modelDefinition">The model definition.</param>
        /// <param name="writeBoundaryConditions"> Whether the boundary conditions should be written.</param>
        /// <param name="polyLineForceFileItems">The poly line external force file items.</param>
        /// <param name="existingForceFileItems">The existing external force file items.</param>
        /// <returns>
        /// A collection of <see cref="ExtForceData"/> from the specified <paramref name="modelDefinition"/>.
        /// </returns>
        public static IEnumerable<ExtForceData> GetItems(WaterFlowFMModelDefinition modelDefinition,
                                                             bool writeBoundaryConditions,
                                                             IDictionary<IFeatureData, ExtForceData> polyLineForceFileItems,
                                                             IDictionary<ExtForceData, object> existingForceFileItems)
        {
            var items = new List<ExtForceData>();

            ExtForceFileHelper.StartWritingSubFiles();

            if (writeBoundaryConditions)
            {
                items.AddRange(GetBoundaryConditionsItems(modelDefinition, polyLineForceFileItems).Values);
            }

            var uniqueFileNameProvider = new UniqueFileNameProvider();
            items.AddRange(GetSourceAndSinkItems(modelDefinition, polyLineForceFileItems).Values);
            items.AddRange(GetSpatialDataItems(ExtForceQuantNames.InitialWaterLevel, modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName), existingForceFileItems, uniqueFileNameProvider).Values);
            items.AddRange(GetSpatialDataItems(ExtForceQuantNames.InitialSalinity, modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.InitialSalinityDataItemName), existingForceFileItems, uniqueFileNameProvider).Values);
            items.AddRange(GetSpatialDataItems(ExtForceQuantNames.InitialTemperature, modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.InitialTemperatureDataItemName), existingForceFileItems, uniqueFileNameProvider).Values);
            items.AddRange(GetSpatialDataItems(ExtForceQuantNames.FrictCoef, modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.RoughnessDataItemName), existingForceFileItems, uniqueFileNameProvider).Values);
            items.AddRange(GetSpatialDataItems(ExtForceQuantNames.HorEddyViscCoef, modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.ViscosityDataItemName), existingForceFileItems, uniqueFileNameProvider).Values);
            items.AddRange(GetSpatialDataItems(ExtForceQuantNames.HorEddyDiffCoef, modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.DiffusivityDataItemName), existingForceFileItems, uniqueFileNameProvider).Values);

            items.AddRange(GetWindFieldItems(modelDefinition, existingForceFileItems).Values);

            ExtForceData heatFluxModelItem = GetHeatFluxModelItem(modelDefinition.HeatFluxModel, modelDefinition.ModelName, existingForceFileItems);
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
                    sp => sp.EndsWith(ExtForceQuantNames.SedimentConcentrationPostfix));
            foreach (string spatiallyVaryingSedimentPropertyName in sedimentConcentrationSpatiallyVarying)
            {
                IList<ISpatialOperation> spatialOperations =
                    modelDefinition.GetSpatialOperations(spatiallyVaryingSedimentPropertyName);
                if (spatialOperations?.All(s => s is ImportSamplesSpatialOperation ||
                                                s is AddSamplesOperation) != true)
                {
                    continue;
                }

                List<ExtForceData> forceFileItems =
                    GetSpatialDataItems(spatiallyVaryingSedimentPropertyName, spatialOperations, existingForceFileItems, uniqueFileNameProvider,
                                        ExtForceQuantNames.InitialSpatialVaryingSedimentPrefix).Values.ToList();

                //Remove the postfix from the quantity (it is not accepted by the kernel)
                if (spatiallyVaryingSedimentPropertyName.EndsWith(ExtForceQuantNames.SedimentConcentrationPostfix))
                {
                    forceFileItems.ForEach(ffi => ffi.Quantity =
                                                      ffi.Quantity.Substring(
                                                          0, ffi.Quantity.Length - ExtForceQuantNames.SedimentConcentrationPostfix.Length));
                }

                items.AddRange(forceFileItems);
            }

            return items.Distinct().ToArray();
        }

        /// <summary>
        /// Creates the mapping with each <see cref="FlowBoundaryCondition"/> from the specified <paramref name="modelDefinition"/>
        /// and their corresponding created <see cref="ExtForceData"/>.
        /// </summary>
        /// <param name="modelDefinition">The model definition.</param>
        /// <param name="polyLineForceFileItems">The poly line external force file items.</param>
        /// <returns>
        /// A dictionary with each <see cref="FlowBoundaryCondition"/> and their corresponding <see cref="ExtForceData"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="modelDefinition"/> or <paramref name="polyLineForceFileItems"/> is <c>null</c>.
        /// </exception>
        public static IDictionary<FlowBoundaryCondition, ExtForceData> GetBoundaryConditionsItems(
            WaterFlowFMModelDefinition modelDefinition,
            IDictionary<IFeatureData, ExtForceData> polyLineForceFileItems)
        {
            if (modelDefinition == null)
            {
                throw new ArgumentNullException(nameof(modelDefinition));
            }

            if (polyLineForceFileItems == null)
            {
                throw new ArgumentNullException(nameof(polyLineForceFileItems));
            }

            var boundaryConditionsItems = new Dictionary<FlowBoundaryCondition, ExtForceData>();

            foreach (BoundaryConditionSet boundaryConditionSet in
                modelDefinition.BoundaryConditionSets.Where(bcs => bcs.Feature.Name != null))
            {
                FlowBoundaryCondition[] flowBoundaryConditions =
                    boundaryConditionSet.BoundaryConditions.OfType<FlowBoundaryCondition>().ToArray();

                foreach (FlowBoundaryCondition flowBoundaryCondition in flowBoundaryConditions)
                {
                    if (!polyLineForceFileItems.TryGetValue(flowBoundaryCondition, out ExtForceData matchingItem))
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
        /// and their corresponding created <see cref="ExtForceData"/>.
        /// </summary>
        /// <param name="modelDefinition">The model definition.</param>
        /// <param name="polyLineForceFileItems">The poly line external force file items.</param>
        /// <returns>
        /// A dictionary with each <see cref="SourceAndSink"/> and their corresponding <see cref="ExtForceData"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="modelDefinition"/> or <paramref name="polyLineForceFileItems"/> is <c>null</c>.
        /// </exception>
        public static IDictionary<SourceAndSink, ExtForceData> GetSourceAndSinkItems(
            WaterFlowFMModelDefinition modelDefinition,
            IDictionary<IFeatureData, ExtForceData> polyLineForceFileItems)
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
        /// and their corresponding created <see cref="ExtForceData"/>.
        /// </summary>
        /// <param name="quantity">The quantity name related to these <paramref name="spatialOperations"/>.</param>
        /// <param name="spatialOperations">The spatial operations.</param>
        /// <param name="existingForceFileItems">The existing external force file items.</param>
        /// <param name="uniqueFileNameProvider"> A unique file name provider </param>
        /// <param name="prefix">The optional prefix to be written before the quantity.</param>
        /// <returns>
        /// A dictionary with each <see cref="ISpatialOperation"/> and their corresponding <see cref="ExtForceData"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown when an item in <paramref name="spatialOperations"/> is not an <see cref="ImportSamplesOperation"/>,
        /// <see cref="SetValueOperation"/> or <see cref="AddSamplesOperation"/>.
        /// </exception>
        public static IDictionary<ISpatialOperation, ExtForceData> GetSpatialDataItems(
            string quantity, IEnumerable<ISpatialOperation> spatialOperations,
            IDictionary<ExtForceData, object> existingForceFileItems,
            UniqueFileNameProvider uniqueFileNameProvider, string prefix = null)
        {
            var dictionary = new Dictionary<ISpatialOperation, ExtForceData>();

            foreach (ISpatialOperation spatialOperation in spatialOperations ?? Enumerable.Empty<ISpatialOperation>())
            {
                ExtForceData extForceFileItem;
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
        /// and their corresponding created <see cref="ExtForceData"/>.
        /// </summary>
        /// <param name="modelDefinition">The model definition.</param>
        /// <param name="existingForceFileItems">The existing external force file items.</param>
        /// <returns>
        /// A dictionary with each <see cref="IWindField"/> and their corresponding <see cref="ExtForceData"/>.
        /// </returns>
        public static IDictionary<IWindField, ExtForceData> GetWindFieldItems(
            WaterFlowFMModelDefinition modelDefinition, IDictionary<ExtForceData, object> existingForceFileItems)
        {
            var dictionary = new Dictionary<IWindField, ExtForceData>();

            ExtForceFileHelper.StartWritingSubFiles();

            foreach (IWindField windField in modelDefinition.WindFields)
            {
                if (windField is IFileBased fileBasedWindField)
                {
                    ExtForceData extForceFileItem = GetWindFieldItem(
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
                    ExtForceData extForceFileItem = GetWindFieldItem(
                        windField, fileName,
                        existingForceFileItems);

                    ExtForceFileHelper.AddSuffixInCaseOfDuplicateFile(extForceFileItem);
                    dictionary.Add(windField, extForceFileItem);
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Creates an <see cref="ExtForceData"/> for the specified <paramref name="heatFluxModel"/>.
        /// </summary>
        /// <param name="heatFluxModel">The heat flux model.</param>
        /// <param name="modelName">Name of the water flow fm model.</param>
        /// <param name="existingForceFileItems">The existing force file items.</param>
        /// <returns>
        /// An <see cref="ExtForceData"/> created for the specified <paramref name="heatFluxModel"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="heatFluxModel"/> or <paramref name="existingForceFileItems"/> is <c>null</c>.
        /// </exception>
        public static ExtForceData GetHeatFluxModelItem(HeatFluxModel heatFluxModel, string modelName,
                                                            IDictionary<ExtForceData, object> existingForceFileItems)
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

            ExtForceData item;
            if (heatFluxModel.GriddedHeatFluxFilePath != null)
            {
                item = GetExistingItem(heatFluxModelType, existingForceFileItems);
            }
            else
            {
                item = GetExistingItem(heatFluxModel.MeteoData, existingForceFileItems) ??
                       new ExtForceData
                       {
                           Quantity = heatFluxModel.ContainsSolarRadiation
                                          ? ExtForceQuantNames.MeteoDataWithRadiation
                                          : ExtForceQuantNames.MeteoData,
                           FileName = modelName + FileConstants.MeteoFileExtension,
                           FileType = ExtForceFileConstants.FileTypes.Uniform,
                           Method = ExtForceFileConstants.Methods.SpaceAndTimeKeepMeteoFields,
                           Operand = ExtForceQuantNames.OperatorToStringMapping[Operator.Overwrite]
                       };
            }

            return item;
        }

        /// <summary>
        /// Creates the mapping with each <see cref="IUnsupportedFileBasedExtForceFileItem"/> from the specified
        /// <paramref name="modelDefinition"/>
        /// and their corresponding created <see cref="ExtForceData"/>.
        /// </summary>
        /// <param name="modelDefinition">The model definition.</param>
        /// <returns>
        /// A dictionary with each <see cref="IUnsupportedFileBasedExtForceFileItem"/> and their corresponding
        /// <see cref="ExtForceData"/>.
        /// </returns>
        public static IDictionary<IUnsupportedFileBasedExtForceFileItem, ExtForceData> GetUnknownQuantitiesItems(WaterFlowFMModelDefinition modelDefinition)
        {
            return modelDefinition.UnsupportedFileBasedExtForceFileItems
                                  .ToDictionary(i => i, i => i.UnsupportedExtForceFileItem);
        }

        private static ExtForceData GetInitialConditionsSamplesItem(
            ImportSamplesSpatialOperation spatialOperation, string extForceFileQuantityName, string prefix,
            IDictionary<ExtForceData, object> existingForceFileItems, UniqueFileNameProvider uniqueFileNameProvider)
        {
            if (spatialOperation == null)
            {
                throw new ArgumentNullException(nameof(spatialOperation));
            }

            if (existingForceFileItems == null)
            {
                throw new ArgumentNullException(nameof(existingForceFileItems));
            }

            ExtForceData existingItem =
                existingForceFileItems.Where(item => item.Value is ImportSamplesSpatialOperation operation && operation.Name == spatialOperation.Name)
                                      .Select(item => item.Key)
                                      .FirstOrDefault();

            string quantityName = prefix != null ? prefix + extForceFileQuantityName : extForceFileQuantityName;
            string fileName = Path.GetFileName(spatialOperation.FilePath);
            ExtForceData extForceFileItem = existingItem ?? new ExtForceData
            {
                Quantity = quantityName,
                FileName = uniqueFileNameProvider.GetUniqueFileNameFor(fileName),
                FileType = ExtForceFileConstants.FileTypes.Triangulation,
                Method = GetImportSamplesSpatialOperationMethod(spatialOperation.InterpolationMethod)
            };
            extForceFileItem.Quantity = quantityName;
            
            if (spatialOperation.InterpolationMethod == SpatialInterpolationMethod.Averaging)
            {
                extForceFileItem.SetModelData(ExtForceFileConstants.Keys.AveragingType, (int) spatialOperation.AveragingMethod);
                extForceFileItem.SetModelData(ExtForceFileConstants.Keys.RelativeSearchCellSize, spatialOperation.RelativeSearchCellSize);
            }

            extForceFileItem.IsEnabled = spatialOperation.Enabled;
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
        public static ExtForceData GetSamplesItem(Samples samples, string quantity, 
                                                      IDictionary<ExtForceData, object> existingForceFileItems)
        {
            Ensure.NotNull(samples, nameof(samples));
            Ensure.NotNull(existingForceFileItems, nameof(existingForceFileItems));
            Ensure.NotNullOrWhiteSpace(quantity, nameof(quantity));

            ExtForceData extForceFileItem = GetExistingItem(samples, existingForceFileItems) ??
                                                new ExtForceData { Quantity = quantity };

            extForceFileItem.FileName = samples.SourceFileName;
            extForceFileItem.FileType = ExtForceFileConstants.FileTypes.Triangulation;
            extForceFileItem.Method = GetImportSamplesSpatialOperationMethod(samples.InterpolationMethod);
            extForceFileItem.Operand = ExtForceQuantNames.OperatorToStringMapping[Operator.Overwrite];
            
            switch (samples.InterpolationMethod)
            {
                case SpatialInterpolationMethod.Averaging:
                    extForceFileItem.SetModelData(ExtForceFileConstants.Keys.AveragingType, (int)samples.AveragingMethod);
                    extForceFileItem.SetModelData(ExtForceFileConstants.Keys.RelativeSearchCellSize, samples.RelativeSearchCellSize);
                    break;
                case SpatialInterpolationMethod.Triangulation:
                    extForceFileItem.SetModelData(ExtForceFileConstants.Keys.ExtrapolationTolerance, samples.ExtrapolationTolerance);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(samples), "Interpolation method of samples is undefined.");
            }

            return extForceFileItem;
        }

        private static ExtForceData GetInitialConditionsPolygonItem(SetValueOperation spatialOperation, string extForceFileQuantityName, string prefix,
                                                                        IDictionary<ExtForceData, object> existingForceFileItems, UniqueFileNameProvider uniqueFileNameProvider)
        {
            if (spatialOperation == null)
            {
                throw new ArgumentNullException(nameof(spatialOperation));
            }

            if (existingForceFileItems == null)
            {
                throw new ArgumentNullException(nameof(existingForceFileItems));
            }

            ExtForceData existingItem = GetExistingItem(spatialOperation, existingForceFileItems);

            string quantityName = prefix != null ? prefix + extForceFileQuantityName : extForceFileQuantityName;
            string fileName = $"{extForceFileQuantityName}_{spatialOperation.Name}{FileConstants.PolylineFileExtension}".ReplaceSpaces();
            ExtForceData extForceFileItem = existingItem ?? new ExtForceData
            {
                Quantity = quantityName,
                FileName = uniqueFileNameProvider.GetUniqueFileNameFor(fileName),
                FileType = ExtForceFileConstants.FileTypes.InsidePolygon,
                Method = ExtForceFileConstants.Methods.InsidePolygon
            };

            ExtForceFileHelper.AddSuffixInCaseOfDuplicateFile(extForceFileItem);

            Operator op = ExtForceQuantNames.OperatorMapping[spatialOperation.OperationType];

            extForceFileItem.Value = spatialOperation.Value;
            extForceFileItem.IsEnabled = spatialOperation.Enabled;
            extForceFileItem.Operand = ExtForceQuantNames.OperatorToStringMapping[op];

            return extForceFileItem;
        }

        private static ExtForceData GetInitialConditionsUnsupportedItem(SampleSpatialOperation spatialOperation,
                                                                            string extForceFileQuantityName, string prefix,
                                                                            UniqueFileNameProvider fileNameProvider)
        {
            if (spatialOperation == null)
            {
                throw new ArgumentNullException(nameof(spatialOperation));
            }

            string quantityName = prefix != null ? prefix + extForceFileQuantityName : extForceFileQuantityName;

            string fileName = $"{extForceFileQuantityName}{FileConstants.XyzFileExtension}".ReplaceSpaces();
            
            var extForceData = new ExtForceData
            {
                Quantity = quantityName,
                FileName = fileNameProvider.GetUniqueFileNameFor(fileName),
                FileType = AddSamplesDefaults.FileType,
                Method = AddSamplesDefaults.Method,
                IsEnabled = spatialOperation.Enabled,
                Operand = ExtForceQuantNames.OperatorToStringMapping[AddSamplesDefaults.Operand]
            };
            
            extForceData.SetModelData(ExtForceFileConstants.Keys.AveragingType, (int) AddSamplesDefaults.AveragingType);
            extForceData.SetModelData(ExtForceFileConstants.Keys.RelativeSearchCellSize, AddSamplesDefaults.RelSearchCellSize);

            return extForceData;
        }

        private static ExtForceData GetWindFieldItem(IWindField windField, string fileName,
                                                     IDictionary<ExtForceData, object> existingForceFileItems)
        {
            return GetExistingItem(windField, existingForceFileItems) ??
                   new ExtForceData
                   {
                       Quantity = ExtForceQuantNames.WindQuantityNames[windField.Quantity],
                       FileName = fileName,
                       FileType = GetFileType(windField),
                       Method = GetMethod(windField),
                       Operand = ExtForceFileConstants.Operands.Add
                   };
        }

        private static ExtForceData GetSourceAndSinkItem(SourceAndSink sourceAndSink, IDictionary<IFeatureData, ExtForceData> polyLineForceFileItems)
        {
            polyLineForceFileItems.TryGetValue(sourceAndSink, out ExtForceData existingItem);

            ExtForceData extForceFileItem = existingItem ?? new ExtForceData
            {
                Quantity = ExtForceQuantNames.SourceAndSink,
                FileName = ExtForceFileHelper.GetPliFileName(sourceAndSink),
                FileType = ExtForceFileConstants.FileTypes.PolyTim,
                Method = ExtForceFileConstants.Methods.SpaceAndTimeKeepMeteoFields,
                Operand = ExtForceQuantNames.OperatorToStringMapping[Operator.Overwrite]
            };

            if (sourceAndSink.Area > 0)
            {
                extForceFileItem.SetModelData(ExtForceFileConstants.Keys.Area, sourceAndSink.Area);
            }

            ExtForceFileHelper.AddSuffixInCaseOfDuplicateFile(extForceFileItem);

            return extForceFileItem;
        }

        private static ExtForceData GetFlowBoundaryConditionsItem(FlowBoundaryCondition flowBoundaryCondition,
                                                                      ExtForceData existingItem)
        {
            existingItem.Quantity = ExtForceQuantNames.GetQuantityString(flowBoundaryCondition);
            existingItem.Offset = Math.Abs(flowBoundaryCondition.Offset) < 1e-6 ? double.NaN : flowBoundaryCondition.Offset;
            existingItem.Factor = Math.Abs(flowBoundaryCondition.Factor - 1) < 1e-6 ? double.NaN : flowBoundaryCondition.Factor;

            ExtForceFileHelper.AddSuffixInCaseOfDuplicateFile(existingItem);

            return existingItem;
        }

        private static int? GetImportSamplesSpatialOperationMethod(SpatialInterpolationMethod interpolationMethod)
        {
            switch (interpolationMethod)
            {
                case SpatialInterpolationMethod.Triangulation:
                    return ExtForceFileConstants.Methods.Triangulation;
                case SpatialInterpolationMethod.Averaging:
                    return ExtForceFileConstants.Methods.Averaging;
                default:
                    return null;
            }
        }

        private static ExtForceData GetExistingItem(object value, IDictionary<ExtForceData, object> existingForceFileItems)
        {
            return existingForceFileItems.Where(item => Equals(item.Value, value))
                                         .Select(item => item.Key)
                                         .FirstOrDefault();
        }

        private static int? GetFileType(IWindField windField)
        {
            if (windField is UniformWindField uniformWindField)
            {
                return uniformWindField.Components.Contains(WindComponent.Magnitude)
                           ? ExtForceFileConstants.FileTypes.UniMagDir
                           : ExtForceFileConstants.FileTypes.Uniform;
            }

            if (windField is GriddedWindField)
            {
                return windField.Quantity == WindQuantity.VelocityVectorAirPressure
                           ? ExtForceFileConstants.FileTypes.Curvi
                           : ExtForceFileConstants.FileTypes.ArcInfo;
            }

            if (windField is SpiderWebWindField)
            {
                return ExtForceFileConstants.FileTypes.SpiderWeb;
            }

            return null;
        }

        private static int? GetMethod(IWindField windField)
        {
            if (windField is UniformWindField)
            {
                return ExtForceFileConstants.Methods.SpaceAndTimeKeepMeteoFields;
            }

            if (windField is GriddedWindField)
            {
                return windField.Quantity == WindQuantity.VelocityVectorAirPressure
                           ? ExtForceFileConstants.Methods.SpaceAndTimeSaveWeights 
                           : ExtForceFileConstants.Methods.SpaceAndTimeKeepFlowFields;
            }

            if (windField is SpiderWebWindField)
            {
                return ExtForceFileConstants.Methods.SpaceAndTimeKeepMeteoFields;
            }

            return null;
        }

        private static string ReplaceSpaces(this string source) => source.Replace(" ", "_").Replace("\t", "_");
    }
}