using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Units;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;
using PointwiseOperationType = SharpMap.SpatialOperations.PointwiseOperationType;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel
{
    public static class WaterQualityModelSyncExtensions
    {
        public const string InitialValueOperationName = "Initial Value";
        private static bool syncing;

        /// <summary>
        /// Synchronizes <paramref name="waterQualityModel"/> after property changed
        /// </summary>
        /// <param name="waterQualityModel"> The water quality model to sync </param>
        /// <param name="sender"> The item that has changed </param>
        /// <param name="e"> The related event arguments </param>
        public static void InputPropertyChanged(this WaterQualityModel waterQualityModel, object sender,
                                                PropertyChangedEventArgs e)
        {
            if (syncing)
            {
                return;
            }

            syncing = true;

            try
            {
                sender = sender is IDataItem item
                             ? item.Value
                             : sender; // Set sender to its value if it is a data item

                var modelSettings = sender as WaterQualityModelSettings;
                if (modelSettings != null && e.PropertyName == "MonitoringOutputLevel")
                {
                    MonitoringOutputLevelChanged(waterQualityModel,
                                                 WaterQualityModel.MonitoringOutputDataItemMetaData.Tag);
                }

                var coverage = sender as WaterQualityObservationAreaCoverage;
                if (coverage != null && e.PropertyName == "IsEditing" && !coverage.IsEditing)
                {
                    UpdateMonitoringOutputDataItems(waterQualityModel);
                }

                var waterQualityOutputParameter = sender as WaterQualityOutputParameter;
                if (waterQualityOutputParameter != null) // Occurs when editing output parameters
                {
                    if (e.PropertyName == "ShowInMap")
                    {
                        UpdateOutputParameterOutputCoverageDataItems(waterQualityModel, waterQualityOutputParameter);
                    }

                    if (e.PropertyName == "ShowInHis")
                    {
                        UpdateMonitoringOutputDataItemOutputParameterTimeSeries(
                            waterQualityModel, waterQualityOutputParameter);
                    }
                }

                var observationPoint = sender as WaterQualityObservationPoint;
                if (observationPoint != null && e.PropertyName == "Name") // Occurs when renaming observation points
                {
                    UpdateMonitoringOutputDataItemName(waterQualityModel, observationPoint);
                }
            }
            finally
            {
                syncing = false;
            }
        }

        public static void InputCollectionChanged(this WaterQualityModel waterQualityModel,
                                                  NotifyCollectionChangedEventArgs e)
        {
            if (syncing)
            {
                return;
            }

            syncing = true;

            try
            {
                object removedOrAddedItem = e.GetRemovedOrAddedItem();
                if (removedOrAddedItem is IDataItem dataItem && IsChildOfWaterQualityModelDataItemSet(waterQualityModel, dataItem))
                {
                    HandleFunctionListCollectionChanged(waterQualityModel.Grid, e.Action, dataItem);
                }

                var substance = removedOrAddedItem as WaterQualitySubstance;
                if (substance != null)
                {
                    UpdateInitialConditions(waterQualityModel, substance, e.Action);
                    UpdateSubstanceOutputCoverageDataItems(waterQualityModel, substance, e.Action);
                    UpdateMonitoringOutputDataItems(waterQualityModel, e);
                }

                var parameter = removedOrAddedItem as WaterQualityParameter;
                if (parameter != null)
                {
                    UpdateProcessCoefficients(waterQualityModel, parameter, e.Action);
                }

                var observationPoint = removedOrAddedItem as WaterQualityObservationPoint;
                if (observationPoint != null)
                {
                    UpdateMonitoringOutputDataItems(waterQualityModel, e);
                }

                var outputParameter = removedOrAddedItem as WaterQualityOutputParameter;
                if (outputParameter != null)
                {
                    UpdateOutputParameterOutputCoverageDataItems(waterQualityModel, e);
                    UpdateMonitoringOutputDataItems(waterQualityModel, e);
                }

                var coverage = removedOrAddedItem as UnstructuredGridCoverage;
                if (coverage != null)
                {
                    // Occurs while adding/removing a Coverage by one of the function list views (initial conditions, process coefficients, etc.)
                    UpdateUnstructuredGridCoverage(waterQualityModel, coverage, e);
                }
            }
            finally
            {
                syncing = false;
            }
        }

        public static void SetGridExtentsAsInputMask(ISpatialOperation operation,
                                                     UnstructuredGridCoverage unstructuredGridCoverage)
        {
            if (unstructuredGridCoverage.Grid.IsEmpty)
            {
                return;
            }

            // calculate the extents of the grid
            Envelope extents = unstructuredGridCoverage.Grid.GetExtents();
            var polygonCollection = new FeatureCollection(new[]
            {
                new Feature
                {
                    Geometry = new Polygon(
                        new LinearRing(new Coordinate[]
                        {
                            new Coordinate(extents.MinX, extents.MinY),
                            new Coordinate(extents.MaxX, extents.MinY),
                            new Coordinate(extents.MaxX, extents.MaxY),
                            new Coordinate(extents.MinX, extents.MaxY),
                            new Coordinate(extents.MinX, extents.MinY)
                        }))
                }
            }, typeof(Feature));

            operation.SetInputData(SpatialOperation.MaskInputName, polygonCollection);
        }

        public static void InsertMonitoringLocationsDataItem(WaterQualityModel waterQualityModel,
                                                             string monitoringOutputTag)
        {
            var dataItemSet = new DataItemSet(new EventedList<WaterQualityObservationVariableOutput>(),
                                              "Monitoring locations",
                                              DataItemRole.Output, true, monitoringOutputTag,
                                              typeof(WaterQualityObservationVariableOutput));
            waterQualityModel.DataItems.Insert(GetMonitoringOutputDataItemSetPosition(waterQualityModel), dataItemSet);
            UpdateMonitoringOutputDataItems(waterQualityModel);
            // Update the monitoring output data items after adding the new monitoring output data item set; all relevant monitoring output data items will be added
        }

        private static IEnumerable<string> GetCurrentMonitoringAreas(WaterQualityModel waterQualityModel)
        {
            return waterQualityModel.ObservationVariableOutputs
                                    .Where(o => !(o.ObservationVariable is WaterQualityObservationPoint))
                                    .Select(v => v.Name);
        }

        private static void UpdateMonitoringOutputDataItemName(WaterQualityModel waterQualityModel,
                                                               WaterQualityObservationPoint observationPoint)
        {
            if (waterQualityModel.ModelSettings.MonitoringOutputLevel == MonitoringOutputLevel.None ||
                waterQualityModel.ModelSettings.MonitoringOutputLevel == MonitoringOutputLevel.Areas)
            {
                return; // Only perform observation variable name updates in case of monitoring output level "Points" or "PointsAndAreas"
            }

            int observationVariableOutputIndex =
                waterQualityModel.ObservationPoints.ToList().IndexOf(observationPoint); // Note: ugly but effective

            waterQualityModel.MonitoringOutputDataItemSet.DataItems[observationVariableOutputIndex].Name =
                observationPoint.Name;
        }

        private static void UpdateMonitoringOutputDataItemOutputParameterTimeSeries(
            WaterQualityModel waterQualityModel, WaterQualityOutputParameter outputParameter)
        {
            if (outputParameter.ShowInHis &&
                waterQualityModel.ObservationVariableOutputs.Any(
                    ovo => ovo.TimeSeriesList.All(ts => ts.Name != outputParameter.Name)))
            {
                // Add a new output parameter time series to all monitoring output data items
                AddMonitoringOutputDataItemTimeSeries(waterQualityModel, outputParameter, "");
            }
            else if (!outputParameter.ShowInHis &&
                     waterQualityModel.ObservationVariableOutputs.Any(
                         ovo => ovo.TimeSeriesList.Any(ts => ts.Name == outputParameter.Name)))
            {
                // Remove the existing output parameter time series from all monitoring output data items
                RemoveMonitoringOutputDataItemTimeSeries(waterQualityModel, outputParameter.Name);
            }
        }

        private static void UpdateOutputParameterOutputCoverageDataItems(
            WaterQualityModel waterQualityModel, NotifyCollectionChangedEventArgs e)
        {
            var outputParameter = (WaterQualityOutputParameter)e.GetRemovedOrAddedItem();
            if (!outputParameter.ShowInMap)
            {
                return; // Only perform output parameter output coverage updates for output parameters that should be shown in map
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        // Add a new output parameter output coverage data item
                        int insertPosition = waterQualityModel
                                             .SubstanceProcessLibrary.OutputParameters.Where(op => op.ShowInMap).ToList()
                                             .IndexOf(outputParameter);
                        AddOutputCoverageDataItem(waterQualityModel, waterQualityModel.OutputParametersDataItemSet,
                                                  insertPosition, outputParameter.Name);
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        // Remove the existing output parameter output coverage data item
                        RemoveOutputCoverageDataItem(outputParameter.Name,
                                                     waterQualityModel.OutputParametersDataItemSet.DataItems);
                        break;
                    }
            }
        }

        private static void UpdateOutputParameterOutputCoverageDataItems(
            WaterQualityModel waterQualityModel, WaterQualityOutputParameter outputParameter)
        {
            IDataItem existingOutputCoverageDataItem =
                waterQualityModel.OutputParametersDataItemSet.DataItems
                                 .Where(dataItem => dataItem.Role.HasFlag(DataItemRole.Output))
                                 .FirstOrDefault(di => di.Name == outputParameter.Name);

            if (outputParameter.ShowInMap && existingOutputCoverageDataItem == null)
            {
                // Add a new output parameter output coverage data item
                int insertPosition = waterQualityModel
                                     .SubstanceProcessLibrary.OutputParameters.Where(op => op.ShowInMap).ToList()
                                     .IndexOf(outputParameter);
                AddOutputCoverageDataItem(waterQualityModel, waterQualityModel.OutputParametersDataItemSet,
                                          insertPosition, outputParameter.Name);
            }
            else if (!outputParameter.ShowInMap && existingOutputCoverageDataItem != null)
            {
                // Remove the existing output parameter output coverage data item
                RemoveOutputCoverageDataItem(outputParameter.Name,
                                             waterQualityModel.OutputParametersDataItemSet.DataItems);
            }
        }

        private static void UpdateMonitoringOutputDataItems(WaterQualityModel waterQualityModel)
        {
            if (waterQualityModel.ModelSettings.MonitoringOutputLevel == MonitoringOutputLevel.None)
            {
                return; // Don't perform monitoring output data item updates when the monitoring output is set to "None"
            }

            IEnumerable<Tuple<string, string>> monitoringOutputVariables =
                GetMonitoringOutputVariables(waterQualityModel);
            MonitoringOutputLevel monitoringOutputLevel = waterQualityModel.ModelSettings.MonitoringOutputLevel;
            IEventedList<WaterQualityObservationPoint> observationPoints = waterQualityModel.ObservationPoints;
            List<string> monitoringAreaOutputDataItemNames =
                GetMonitoringAreaOutputDataItemNames(waterQualityModel).ToList();

            bool showPoints = monitoringOutputLevel == MonitoringOutputLevel.Points ||
                              monitoringOutputLevel == MonitoringOutputLevel.PointsAndAreas;

            bool showAreas = monitoringOutputLevel == MonitoringOutputLevel.Areas ||
                             monitoringOutputLevel == MonitoringOutputLevel.PointsAndAreas;

            // If relevant, remove any existing monitoring point output data items
            if (!showPoints)
            {
                observationPoints.ForEach(p => RemoveMonitoringOutputDataItem(waterQualityModel, p));
            }

            // If relevant, remove any existing monitoring area output data items
            if (!showAreas)
            {
                GetCurrentMonitoringAreas(waterQualityModel)
                    .ToList().ForEach(a => RemoveMonitoringOutputDataItem(waterQualityModel, a));
            }

            // If relevant, add any missing monitoring point output data items
            if (showPoints)
            {
                observationPoints.ForEach(
                    p => AddMonitoringOutputDataItem(waterQualityModel, p, monitoringOutputVariables));
            }

            // If relevant, add any missing monitoring area output data items
            if (showAreas)
            {
                // remove old areas 
                List<string> itemsToRemove = GetCurrentMonitoringAreas(waterQualityModel)
                                             .Except(monitoringAreaOutputDataItemNames).ToList();
                itemsToRemove.ForEach(i => RemoveMonitoringOutputDataItem(waterQualityModel, i));

                // add new items
                IEnumerable<string> itemsToAdd =
                    monitoringAreaOutputDataItemNames.Except(GetCurrentMonitoringAreas(waterQualityModel));
                itemsToAdd.ForEach(i => AddMonitoringOutputDataItem(waterQualityModel, i, monitoringOutputVariables));
            }
        }

        private static IEnumerable<string> GetMonitoringAreaOutputDataItemNames(WaterQualityModel waterQualityModel)
        {
            return waterQualityModel.ObservationAreas.GetLabelList();
        }

        private static void UpdateMonitoringOutputDataItems(WaterQualityModel waterQualityModel,
                                                            NotifyCollectionChangedEventArgs e)
        {
            // Add/remove a monitoring output data item for added/removed observation points
            object removedOrAddedItem = e.GetRemovedOrAddedItem();
            var observationPoint = removedOrAddedItem as WaterQualityObservationPoint;
            if (observationPoint != null &&
                (waterQualityModel.ModelSettings.MonitoringOutputLevel == MonitoringOutputLevel.Points ||
                 waterQualityModel.ModelSettings.MonitoringOutputLevel == MonitoringOutputLevel.PointsAndAreas)
            ) // Only perform observation variable output item updates for monitoring output level "Points" or "PointsAndAreas"
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        AddMonitoringOutputDataItem(waterQualityModel, observationPoint,
                                                    GetMonitoringOutputVariables(waterQualityModel));
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        RemoveMonitoringOutputDataItem(waterQualityModel, observationPoint);
                        break;
                }
            }

            // Update all monitoring output data item time series for added/removed substances
            var substance = removedOrAddedItem as WaterQualitySubstance;
            if (substance != null
            ) // Only perform monitoring output data item substance time series updates for substance calculations
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            // Add a new substance time series to all monitoring output data items
                            AddMonitoringOutputDataItemTimeSeries(waterQualityModel, substance, "");

                            break;
                        }
                    case NotifyCollectionChangedAction.Remove:
                        {
                            // Remove the existing substance time series from all monitoring output data items
                            RemoveMonitoringOutputDataItemTimeSeries(waterQualityModel, substance.Name);

                            break;
                        }
                }
            }

            // Update all monitoring output data item time series for added/removed output parameters that should be shown in his
            var outputParameter = removedOrAddedItem as WaterQualityOutputParameter;
            if (outputParameter != null && outputParameter.ShowInHis
            ) // Only perform monitoring output data item output parameter time series updates for substance calculations
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            // Add a new output parameter time series to all monitoring output data items
                            AddMonitoringOutputDataItemTimeSeries(waterQualityModel, outputParameter, "");

                            break;
                        }
                    case NotifyCollectionChangedAction.Remove:
                        {
                            // Remove the existing output parameter time series from all monitoring output data items
                            RemoveMonitoringOutputDataItemTimeSeries(waterQualityModel, outputParameter.Name);
                            break;
                        }
                }
            }
        }

        private static void RemoveMonitoringOutputDataItemTimeSeries(WaterQualityModel waterQualityModel, string name)
        {
            foreach (WaterQualityObservationVariableOutput observationVariableOutput in waterQualityModel
                .ObservationVariableOutputs)
            {
                observationVariableOutput.RemoveTimeSeries(name);
            }
        }

        private static void AddMonitoringOutputDataItemTimeSeries(WaterQualityModel waterQualityModel,
                                                                  object outputItem, string unit)
        {
            GetMonitoringOutputTimeSeriesPositionAndName(waterQualityModel, outputItem,
                                                         out string monitoringOutputTimeSeriesName,
                                                         out int monitoringOutputInsertPosition);

            foreach (WaterQualityObservationVariableOutput observationVariableOutput in waterQualityModel
                .ObservationVariableOutputs)
            {
                observationVariableOutput.AddTimeSeries(new Tuple<string, string>(monitoringOutputTimeSeriesName, unit),
                                                        monitoringOutputInsertPosition);
            }
        }

        private static void GetMonitoringOutputTimeSeriesPositionAndName(
            WaterQualityModel waterQualityModel, object outputItem, out string monitoringOutputTimeSeriesName,
            out int monitoringOutputTimeSeriesPosition)
        {
            monitoringOutputTimeSeriesName = "monitoring time series";
            monitoringOutputTimeSeriesPosition = 0;

            var substance = outputItem as WaterQualitySubstance;
            if (substance != null)
            {
                monitoringOutputTimeSeriesName = substance.Name;
                monitoringOutputTimeSeriesPosition +=
                    waterQualityModel.SubstanceProcessLibrary.Substances.IndexOf(substance);

                return;
            }

            var outputParameter = outputItem as WaterQualityOutputParameter;
            if (outputParameter != null)
            {
                monitoringOutputTimeSeriesName = outputParameter.Name;
                monitoringOutputTimeSeriesPosition += waterQualityModel.SubstanceProcessLibrary.Substances.Count +
                                                      waterQualityModel.SubstanceProcessLibrary.OutputParameters
                                                                       .Where(op => op.ShowInHis)
                                                                       .ToList()
                                                                       .IndexOf(outputParameter);
            }
        }

        private static void RemoveMonitoringOutputDataItem(WaterQualityModel waterQualityModel,
                                                           object monitoringOutputDataItemObject)
        {
            var observationPoint = monitoringOutputDataItemObject as WaterQualityObservationPoint;
            if (observationPoint != null)
            {
                WaterQualityObservationVariableOutput monitoringPointOutputDataItemToRemove =
                    waterQualityModel.ObservationVariableOutputs.FirstOrDefault(
                        ovo => observationPoint.Equals(ovo.ObservationVariable));
                if (monitoringPointOutputDataItemToRemove == null)
                {
                    return;
                }

                // Remove the observation point related monitoring output data item
                waterQualityModel.ObservationVariableOutputs.Remove(monitoringPointOutputDataItemToRemove);
            }

            var monitoringAreaName = monitoringOutputDataItemObject.ToString();
            if (!string.IsNullOrEmpty(monitoringAreaName))
            {
                WaterQualityObservationVariableOutput monitoringPointOutputDataItemToRemove =
                    waterQualityModel.ObservationVariableOutputs
                                     .Where(o => !(o.ObservationVariable is WaterQualityObservationPoint))
                                     .FirstOrDefault(ovo => monitoringAreaName.Equals(ovo.Name));

                if (monitoringPointOutputDataItemToRemove == null)
                {
                    return;
                }

                // Remove the surface water type related monitoring output data item
                waterQualityModel.ObservationVariableOutputs.Remove(monitoringPointOutputDataItemToRemove);
            }
        }

        private static IEnumerable<Tuple<string, string>> GetMonitoringOutputVariables(
            WaterQualityModel waterQualityModel)
        {
            var outputVariables = new List<Tuple<string, string>>();

            // Add a DelftTools.Utils.Tuple for all substances
            outputVariables.AddRange(waterQualityModel.SubstanceProcessLibrary.Substances
                                                      .Select(s => new Tuple<string, string>(
                                                                  s.Name, s.ConcentrationUnit)));

            // Add a DelftTools.Utils.Tuple for all output parameters that should be shown in his
            outputVariables.AddRange(waterQualityModel.SubstanceProcessLibrary.OutputParameters
                                                      .Where(op => op.ShowInHis)
                                                      .Select(op => new Tuple<string, string>(op.Name, "")));

            return outputVariables;
        }

        private static void AddMonitoringOutputDataItem(WaterQualityModel waterQualityModel,
                                                        object monitoringOutputDataItemObject,
                                                        IEnumerable<Tuple<string, string>> outputVariables)
        {
            var observationPoint = monitoringOutputDataItemObject as WaterQualityObservationPoint;
            if (observationPoint != null)
            {
                // Check if the observation point related monitoring output data item is already present
                if (waterQualityModel.ObservationVariableOutputs.Any(
                    ovo => observationPoint.Equals(ovo.ObservationVariable)))
                {
                    return;
                }

                int insertIndex = waterQualityModel.ObservationPoints.ToList().IndexOf(observationPoint);
                if (insertIndex == -1)
                {
                    return;
                }

                // Insert the new observation point related monitoring output data item
                waterQualityModel.MonitoringOutputDataItemSet.DataItems.Insert(
                    insertIndex,
                    new DataItem(
                        new WaterQualityObservationVariableOutput(outputVariables) { ObservationVariable = observationPoint })
                    {
                        Owner = waterQualityModel,
                        Role = DataItemRole.Output
                    });

                return;
            }

            var surfaceWaterType = monitoringOutputDataItemObject.ToString();
            if (!string.IsNullOrEmpty(surfaceWaterType))
            {
                int observationPointMonitoringoutputDataItemCount =
                    waterQualityModel.ModelSettings.MonitoringOutputLevel != MonitoringOutputLevel.Areas
                        ? waterQualityModel.ObservationPoints.Count()
                        : 0;

                // Check if the surface water type related monitoring output data item is already present
                if (waterQualityModel.ObservationVariableOutputs.Skip(observationPointMonitoringoutputDataItemCount)
                                     .Any(ovo => surfaceWaterType.Equals(ovo.Name)))
                {
                    return;
                }

                List<string> monitoringAreaOutputDataItemNames =
                    GetMonitoringAreaOutputDataItemNames(waterQualityModel).ToList();
                int insertIndex = monitoringAreaOutputDataItemNames.IndexOf(surfaceWaterType) +
                                  observationPointMonitoringoutputDataItemCount;

                // Insert the new surface water type related related monitoring output data item
                waterQualityModel.MonitoringOutputDataItemSet.DataItems.Insert(
                    insertIndex,
                    new DataItem(
                        new WaterQualityObservationVariableOutput(outputVariables) { Name = surfaceWaterType })
                    {
                        Owner = waterQualityModel,
                        Role = DataItemRole.Output
                    });
            }
        }

        private static bool IsChildOfWaterQualityModelDataItemSet(WaterQualityModel waterQualityModel,
                                                                  IDataItem dataItem)
        {
            // All collections in a DataItemSet should be mentioned here:
            return waterQualityModel.GetDataItemByTag(WaterQualityModel.InitialConditionsDataItemMetaData.Tag)
                                    .Equals(dataItem.Owner) ||
                   waterQualityModel.GetDataItemByTag(WaterQualityModel.DispersionDataItemMetaData.Tag)
                                    .Equals(dataItem.Owner) ||
                   waterQualityModel.GetDataItemByTag(WaterQualityModel.ProcessCoefficientsDataItemMetaData.Tag)
                                    .Equals(dataItem.Owner);
        }

        private static void HandleFunctionListCollectionChanged(UnstructuredGrid grid,
                                                                NotifyCollectionChangedAction
                                                                    notifyCollectionChangeAction, IDataItem dataItem)
        {
            switch (notifyCollectionChangeAction)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        var unstructuredGridCoverage = dataItem.Value as UnstructuredGridCellCoverage;
                        // when an initial condition or other list of functions/coverages was altered (changed from constant to coverage in this case)
                        // make sure that the coverage is cleared, because it was replaced and a new coverage is created.
                        // It will receive a new grid with the right number of cells.
                        if (unstructuredGridCoverage != null)
                        {
                            unstructuredGridCoverage.AssignNewGridToCoverage(grid);

                            // create a spatial operation value converter and add a set value operation
                            SpatialOperationSetValueConverter valueConverter =
                                SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(
                                    dataItem, unstructuredGridCoverage.Name);
                            var operation = new SetValueOperation
                            {
                                Name = InitialValueOperationName,
                                Value = WaterQualityFunctionFactory.GetDefaultValue(unstructuredGridCoverage),
                                OperationType = PointwiseOperationType.OverwriteWhereMissing
                            };

                            // set the input mask of the set value operation
                            SetGridExtentsAsInputMask(operation, unstructuredGridCoverage);

                            // add the operation to the set
                            valueConverter.SpatialOperationSet.AddOperation(operation);
                        }
                    }
                    break;
            }
        }

        private static void UpdateUnstructuredGridCoverage(WaterQualityModel waterQualityModel,
                                                           UnstructuredGridCoverage coverage,
                                                           NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        coverage.Grid = waterQualityModel.Grid;
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        coverage.Grid = null;
                        break;
                    }
            }
        }

        private static void UpdateProcessCoefficients(WaterQualityModel waterQualityModel,
                                                      WaterQualityParameter parameter,
                                                      NotifyCollectionChangedAction action)
        {
            string name = parameter.Name;
            double defaultValue = parameter.DefaultValue;
            string unit = parameter.Unit;
            string description = parameter.Description;

            if (action == NotifyCollectionChangedAction.Add && waterQualityModel.HasDataInHydroDynamics(name))
            {
                FunctionFromHydroDynamics functionFromHydroData =
                    WaterQualityFunctionFactory.CreateFunctionFromHydroDynamics(
                        name, defaultValue, unit, unit, description);
                functionFromHydroData.FilePath = waterQualityModel.GetFilePathFromHydroDynamics(functionFromHydroData);
                waterQualityModel.ProcessCoefficients.Add(functionFromHydroData);
            }
            else
            {
                UpdateFunctionCollection(action, waterQualityModel.ProcessCoefficients, name,
                                         defaultValue, unit, description);
            }
        }

        private static void UpdateInitialConditions(WaterQualityModel waterQualityModel,
                                                    WaterQualitySubstance substanceVariable,
                                                    NotifyCollectionChangedAction action)
        {
            UpdateFunctionCollection(action, waterQualityModel.InitialConditions, substanceVariable.Name,
                                     substanceVariable.InitialValue, substanceVariable.ConcentrationUnit,
                                     substanceVariable.Description);
        }

        private static void UpdateFunctionCollection(NotifyCollectionChangedAction action,
                                                     ICollection<IFunction> functionCollection, string functionName,
                                                     double defaultValue, string componentUnitName, string description)
        {
            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        AddNewConstantFunction(functionCollection, functionName, defaultValue, componentUnitName,
                                               description);
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        RemoveFunction(functionCollection, functionName);
                        break;
                    }
            }
        }

        private static void AddNewConstantFunction(ICollection<IFunction> functionCollection, string functionName,
                                                   double defaultValue, string componentUnitName, string description)
        {
            IFunction newConstantFunction =
                WaterQualityFunctionFactory.CreateConst(functionName, defaultValue, functionName, componentUnitName,
                                                        description);
            functionCollection.Add(newConstantFunction);
        }

        private static void RemoveFunction(ICollection<IFunction> functionCollection, string functionName)
        {
            IFunction processCoefficient = functionCollection.FirstOrDefault(icd => icd.Name == functionName);
            if (processCoefficient != null)
            {
                functionCollection.Remove(processCoefficient);
            }
        }

        private static void UpdateSubstanceOutputCoverageDataItems(WaterQualityModel waterQualityModel,
                                                                   WaterQualitySubstance substance,
                                                                   NotifyCollectionChangedAction action)
        {
            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        // Add a new substance output coverage data item
                        AddOutputCoverageDataItem(waterQualityModel, waterQualityModel.OutputSubstancesDataItemSet,
                                                  waterQualityModel.SubstanceProcessLibrary.Substances.IndexOf(substance),
                                                  substance.Name, substance.ConcentrationUnit);
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        // Remove the existing substance output coverage data item
                        RemoveOutputCoverageDataItem(substance.Name,
                                                     waterQualityModel.OutputSubstancesDataItemSet.DataItems);
                        break;
                    }
            }
        }

        private static void AddOutputCoverageDataItem(WaterQualityModel waterQualityModel, IDataItemSet dataItemSet,
                                                      int insertPosition, string outputDataItemName, string unit = null)
        {
            var unstructuredGridCellCoverage = new UnstructuredGridCellCoverage(waterQualityModel.Grid, true)
            {
                Name = outputDataItemName,
                IsEditable = false,
                Store = waterQualityModel.MapFileFunctionStore
            };

            unstructuredGridCellCoverage.Components[0].Name = outputDataItemName;
            unstructuredGridCellCoverage.Components[0].Unit =
                !string.IsNullOrEmpty(unit) ? new Unit(unit, unit) : new Unit("-", "-");
            unstructuredGridCellCoverage.Components[0].NoDataValue = -999;

            waterQualityModel.MapFileFunctionStore.Functions.AddRange(GetAllFunctions(unstructuredGridCellCoverage));

            var dataItem =
                new DataItem(unstructuredGridCellCoverage, DataItemRole.Output, outputDataItemName) { Name = outputDataItemName };
            dataItemSet.DataItems.Insert(insertPosition, dataItem);
        }

        private static IEnumerable<IFunction> GetAllFunctions(IFunction function)
        {
            return function.Arguments.Concat(function.Components).Concat(new[]
            {
                function
            });
        }

        private static void RemoveOutputCoverageDataItem(string outputCoverageToRemoveName,
                                                         IEventedList<IDataItem> dataItems)
        {
            IDataItem outputCoverageDataItem = dataItems.First(di => di.Role.HasFlag(DataItemRole.Output)
                                                                     && di.Name == outputCoverageToRemoveName
                                                                     && !(di.Value is FeatureCoverage));

            var coverage = (UnstructuredGridCellCoverage)outputCoverageDataItem.Value;
            if (coverage.Store is LazyMapFileFunctionStore functionStore)
            {
                foreach (IFunction function in GetAllFunctions(coverage))
                {
                    functionStore.Functions.Remove(function);
                }
            }

            dataItems.Remove(outputCoverageDataItem);
        }

        private static void MonitoringOutputLevelChanged(WaterQualityModel waterQualityModel,
                                                         string monitoringOutputTag)
        {
            IDataItemSet existingMonitoringOutputDataItemSet = waterQualityModel.MonitoringOutputDataItemSet;

            // If relevant, add a new monitoring output data item set and update the monitoring output data items
            if (existingMonitoringOutputDataItemSet == null &&
                waterQualityModel.ModelSettings.MonitoringOutputLevel != MonitoringOutputLevel.None)
            {
                InsertMonitoringLocationsDataItem(waterQualityModel, monitoringOutputTag);

                return;
            }

            // If relevant, update the monitoring output data items and remove the existing monitoring output data item set
            if (existingMonitoringOutputDataItemSet != null &&
                waterQualityModel.ModelSettings.MonitoringOutputLevel == MonitoringOutputLevel.None)
            {
                UpdateMonitoringOutputDataItems(
                    waterQualityModel); // Update the monitoring output data items before removing the monitoring output data item set; all existing monitoring output data items will be correctly removed
                waterQualityModel.DataItems.Remove(existingMonitoringOutputDataItemSet);

                return;
            }

            // Update the monitoring output data items after having switched from a not-None type to another not-None type
            UpdateMonitoringOutputDataItems(waterQualityModel);
        }

        private static int GetMonitoringOutputDataItemSetPosition(WaterQualityModel waterQualityModel)
        {
            int startingPosition = waterQualityModel.DataItems.Count(di => !di.Role.HasFlag(DataItemRole.Output));

            startingPosition += waterQualityModel.SubstanceProcessLibrary.Substances.Count();
            startingPosition += waterQualityModel.SubstanceProcessLibrary.OutputParameters.Count(op => op.ShowInMap);

            return startingPosition;
        }
    }
}