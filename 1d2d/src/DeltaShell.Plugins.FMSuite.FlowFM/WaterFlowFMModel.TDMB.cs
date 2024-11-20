using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Units;
using DelftTools.Utils.Aop;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public partial class WaterFlowFMModel
    {
        private DateTime? cachedEndTime;
        private DateTime? cachedStartTime;
        
        public override IEnumerable<IDataItem> AllDataItems
        {
            get
            {
                var lateralDataItems = LateralSourcesData.Select(d => d.SeriesDataItem);

                return base.AllDataItems.Concat(areaDataItems.Values.SelectMany(v => v)).Concat(lateralDataItems);
            }
        }
        
        /// <inheritdoc />
        [NoNotifyPropertyChange]
        public override DateTime StartTime
        {
            get
            {
                if (cachedStartTime.HasValue)
                {
                    return cachedStartTime.Value;
                }

                return (DateTime)ModelDefinition.GetModelProperty(GuiProperties.StartTime).Value;
            }
            set
            {
                ModelDefinition.GetModelProperty(GuiProperties.StartTime).Value = value;
                cachedStartTime = null;
                // This base model setting is made to make the base logic right
                base.StartTime = value;
            }
        }
        
        /// <inheritdoc />
        [NoNotifyPropertyChange]
        public override DateTime StopTime
        {
            get
            {
                if (cachedEndTime.HasValue)
                {
                    return cachedEndTime.Value;
                }
                return (DateTime)ModelDefinition.GetModelProperty(GuiProperties.StopTime).Value;
            }
            set
            {
                ModelDefinition.GetModelProperty(GuiProperties.StopTime).Value = value;
                cachedEndTime = null;
                // This base model setting is made to make the base logic right
                base.StopTime = value;
            }
        }
        
        /// <inheritdoc />
        public override TimeSpan TimeStep
        {
            get { return (TimeSpan) ModelDefinition.GetModelProperty(KnownProperties.DtUser).Value; }
            set
            {
                ModelDefinition.GetModelProperty(KnownProperties.DtUser).Value = value;
                // This base model setting is made to make the base logic right
                base.TimeStep = value;
            }
        }
        
        private IEnumerable<object> FeatureCollections
        {
            get
            {
                yield return Area.Pumps;
                yield return Area.Weirs;
                yield return Area.Gates;
                yield return SourcesAndSinks.Select(ss => ss.Feature).ToList();
                yield return Network.Pumps;
                yield return Network.Weirs;
                yield return Network.Orifices;
                yield return Network.Gates;
                yield return Network.Culverts;
                yield return Network.LateralSources;
            }
        }

        /// <summary>
        /// Gets the output data item feature collections.
        /// </summary>
        private IEnumerable<object> OutputFeatureCollections
        {
            get
            {
                yield return Area.LeveeBreaches.OfType<LeveeBreach>().ToList();
                yield return Area.ObservationPoints;
                yield return Area.ObservationCrossSections;
                yield return Network.ObservationPoints;
            }
        }
        
        /// <inheritdoc />
        public override IProjectItem DeepClone()
        {
            var tempDir = FileUtils.CreateTempDirectory();
            var mduFileName = MduFilePath != null ? Path.GetFileName(MduFilePath) : "some_temp.mdu";
            var tempFilePath = Path.Combine(tempDir, mduFileName);
            ExportTo(tempFilePath, false);

            return new WaterFlowFMModel(tempFilePath);
        }
        
        /// <inheritdoc />
        public override IEnumerable<object> GetDirectChildren()
        {
            foreach (var item in base.GetDirectChildren())
                yield return item;

            foreach (var boundary in Boundaries)
            {
                yield return boundary;
            }
            foreach (var model1DBoundaryNodeData in BoundaryConditions1D)
            {
                yield return model1DBoundaryNodeData;
            }
            foreach (var model1DLateralSourceData in LateralSourcesData)
            {
                yield return model1DLateralSourceData;
            }
            foreach (var pipe in Pipes)
            {
                yield return pipe;
            }

            foreach (var boundaryConditionSet in BoundaryConditionSets)
            {
                yield return boundaryConditionSet;
            }

            foreach (var sourcesAndSink in SourcesAndSinks)
            {
                yield return sourcesAndSink;
            }

            if (ModelDefinition.HeatFluxModel.MeteoData != null)
            {
                yield return ModelDefinition.HeatFluxModel;
            }

            yield return WindFields;

            foreach (var windField in WindFields)
            {
                yield return windField;
            }

            yield return Links;

            foreach (var link in Links)
            {
                yield return link;
            }

            yield return InitialSalinity;
            yield return Viscosity;
            yield return Diffusivity;
            yield return Roughness;
            yield return Infiltration;
            yield return InitialWaterLevel;
            yield return InitialTemperature;
            yield return InitialTracers;
            yield return InitialFractions;
            yield return Network;

            // for QueryTimeSeries tool:
            if (OutputHisFileStore != null)
                foreach (var featureCoverage in OutputHisFileStore.Functions)
                    yield return featureCoverage;

            if (OutputMapFileStore != null)
                foreach (var function in OutputMapFileStore.Functions)
                    yield return function;
            if (Output1DFileStore != null)
                foreach (var function in Output1DFileStore.Functions)
                    yield return function;
            if (OutputClassMapFileStore != null)
            {
                foreach (IFunction function in OutputClassMapFileStore.Functions)
                {
                    yield return function;
                }
            }
        }

        /// <summary>
        /// Gets all 1D and 2D data items for a given role.
        /// </summary>
        /// <param name="role">The role to get the data items for.</param>
        /// <returns>The collection of data items.</returns>
        public override IEnumerable<IFeature> GetChildDataItemLocations(DataItemRole role)
        {
            if (role.HasFlag(DataItemRole.Input) || role.HasFlag(DataItemRole.Output))
            {
                foreach (IFeature feature in GetFeatures())
                {
                    yield return feature;
                }
            }

            if (role.HasFlag(DataItemRole.Output))
            {
                foreach (IFeature outputFeature in GetOutputSpecificFeatures())
                {
                    yield return outputFeature;
                }
            }
        }

        private IEnumerable<IFeature> GetOutputSpecificFeatures()
        {
            return OutputFeatureCollections.OfType<IEnumerable>()
                                           .SelectMany(l => l.OfType<IFeature>());
        }

        private IEnumerable<IFeature> GetFeatures()
        {
            return FeatureCollections.OfType<IEnumerable>()
                                     .SelectMany(l => l.OfType<IFeature>());
        }

        /// <inheritdoc />
        public override IEnumerable<IDataItem> GetChildDataItems(IFeature location)
        {
            if (location == null) yield break;

            areaDataItems.TryGetValue(location, out List<IDataItem> items);

            if (items != null)
            {
                foreach (var di in items)
                {
                    yield return di;
                }
            }

            if (location.Geometry is Point)
            {
                IDataItem networkAsDataItem = GetDataItemByValue(Network);
                // Engine parameters that can be set by RTC
                foreach (EngineParameter engineParameter in GetEngineParametersForLocation(location))
                {
                    // search it first in existing data items
                    IDataItem existingDataItem = networkAsDataItem.Children
                                                                .FirstOrDefault(di => di.ValueType == typeof(double)
                                                                                      && di.ValueConverter is Model1DBranchFeatureValueConverter valueConverter
                                                                                      && IsValueConverterForEngineParameter(location, valueConverter, engineParameter));

                    if (existingDataItem != null)
                    {
                        yield return existingDataItem;
                    }
                    else
                    {
                        yield return new DataItem(location)
                        {
                            Name = location.ToString(),
                            Role = engineParameter.Role,
                            Tag = engineParameter.Name,
                            ValueType = typeof(double),
                            Parent = networkAsDataItem,
                            ShouldBeRemovedAfterUnlink = true,
                            ValueConverter =
                                new Model1DBranchFeatureValueConverter(
                                    this,
                                    location,
                                    engineParameter.Name,
                                    engineParameter.QuantityType,
                                    engineParameter.ElementSet,
                                    engineParameter.Role,
                                    engineParameter.Unit.Symbol)
                        };
                    }
                }
            }
        }

        private static bool IsValueConverterForEngineParameter(IFeature location, Model1DBranchFeatureValueConverter valueConverter, EngineParameter engineParameter)
        {
            return valueConverter.ParameterName == engineParameter.Name
                   && valueConverter.Role == engineParameter.Role
                   && valueConverter.ElementSet == engineParameter.ElementSet 
                   && valueConverter.QuantityType == engineParameter.QuantityType
                   && Equals(valueConverter.Location, location);
        }

        private IEnumerable<EngineParameter> GetEngineParametersForLocation(IFeature location)
        {
            if (location is IHydroNode)
            {
                var boundary = BoundaryConditions1D.FirstOrDefault(boundaryNodeData => boundaryNodeData.Node.Equals(location));
                if (boundary == null) yield break;

                switch (boundary.DataType)
                {
                    case Model1DBoundaryNodeDataType.WaterLevelConstant:
                    case Model1DBoundaryNodeDataType.WaterLevelTimeSeries:
                        yield return new EngineParameter(QuantityType.WaterLevel, ElementSet.HBoundaries,
                            DataItemRole.Input, FunctionAttributes.StandardNames.WaterLevel,
                            new Unit("Meter above reference level", "m AD"));
                        yield return new EngineParameter(QuantityType.Discharge, ElementSet.HBoundaries,
                            DataItemRole.Output, FunctionAttributes.StandardNames.WaterDischarge,
                            new Unit("Cubic meter", Resources.WaterFlowFMModel_GetEngineParametersForLocation_CubicMeter));
                        break;
                    case Model1DBoundaryNodeDataType.FlowConstant:
                    case Model1DBoundaryNodeDataType.FlowTimeSeries:
                        yield return new EngineParameter(QuantityType.Discharge, ElementSet.QBoundaries,
                            DataItemRole.Input, FunctionAttributes.StandardNames.WaterDischarge,
                            new Unit("Cubic meter", Resources.WaterFlowFMModel_GetEngineParametersForLocation_CubicMeter));
                        yield return new EngineParameter(QuantityType.Discharge, ElementSet.QBoundaries,
                            DataItemRole.Output, FunctionAttributes.StandardNames.WaterLevel,
                            new Unit("Meter above reference level", "m AD"));
                        break;
                }
            }
            else
            {
                foreach (EngineParameter exchangeableParameter in EngineParameters.GetExchangeableParameters(
                             EngineParameters.EngineMapping(), location, UseSalinity, UseTemperature))
                {
                    yield return exchangeableParameter;
                }
            }
        }
    }
}