using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    public partial class WaterFlowFMModel
    {
        /// <inheritdoc />
        public override DateTime StartTime
        {
            get => (DateTime) ModelDefinition.GetModelProperty(KnownProperties.StartDateTime).Value;
            set
            {
                ModelDefinition.GetModelProperty(KnownProperties.StartDateTime).Value = value;
                // This base model setting is made to make the base logic right
                base.StartTime = value;
            }
        }

        /// <inheritdoc />
        public override IEnumerable<IDataItem> AllDataItems
        {
            get
            {
                return base.AllDataItems.Concat(areaDataItems.Values.SelectMany(v => v)).Concat(SpatialData.DataItems);
            }
        }

        /// <inheritdoc />
        public override DateTime StopTime
        {
            get => (DateTime) ModelDefinition.GetModelProperty(KnownProperties.StopDateTime).Value;
            set
            {
                ModelDefinition.GetModelProperty(KnownProperties.StopDateTime).Value = value;
                // This base model setting is made to make the base logic right
                base.StopTime = value;
            }
        }

        /// <inheritdoc />
        public override TimeSpan TimeStep
        {
            get => (TimeSpan) ModelDefinition.GetModelProperty(KnownProperties.DtUser).Value;
            set
            {
                ModelDefinition.GetModelProperty(KnownProperties.DtUser).Value = value;
                // This base model setting is made to make the base logic right
                base.TimeStep = value;
            }
        }
        
        /// <summary>
        /// Gets the input data item feature collections.
        /// </summary>
        private IEnumerable<IEnumerable<IFeature>> InputFeatureCollections
        {
            get
            {
                yield return Area.Pumps;
                yield return Area.Structures;
            }
        }

        /// <summary>
        /// Gets the output data item feature collections.
        /// </summary>
        private IEnumerable<IEnumerable<IFeature>> OutputFeatureCollections
        {
            get
            {
                yield return Area.Pumps;
                yield return Area.Structures;
                yield return Area.ObservationPoints;
                yield return Area.ObservationCrossSections;
            }
        }

        /// <inheritdoc />
        public override IProjectItem DeepClone()
        {
            string tempDir = FileUtils.CreateTempDirectory();
            string mduFileName = MduFilePath != null ? Path.GetFileName(MduFilePath) : $"some_temp{FileConstants.MduFileExtension}";
            string tempFilePath = Path.Combine(tempDir, mduFileName);
            ExportTo(tempFilePath, false);

            var waterFlowFmModel = new WaterFlowFMModel();
            waterFlowFmModel.ImportFromMdu(tempFilePath);

            return waterFlowFmModel;
        }

        /// <summary>
        /// Gets the direct children of the parent object
        /// </summary>
        /// <returns> </returns>
        public override IEnumerable<object> GetDirectChildren()
        {
            foreach (object item in base.GetDirectChildren())
            {
                yield return item;
            }

            foreach (Feature2D boundary in Boundaries)
            {
                yield return boundary;
            }

            foreach (Feature2D pipe in Pipes)
            {
                yield return pipe;
            }

            foreach (Feature2D lateralFeature in LateralFeatures)
            {
                yield return lateralFeature;
            }

            foreach (BoundaryConditionSet boundaryConditionSet in BoundaryConditionSets)
            {
                yield return boundaryConditionSet;
            }

            foreach (SourceAndSink sourcesAndSink in SourcesAndSinks)
            {
                yield return sourcesAndSink;
            }

            foreach (Lateral lateral in Laterals)
            {
                yield return lateral;
            }

            if (ModelDefinition.HeatFluxModel.MeteoData != null)
            {
                yield return ModelDefinition.HeatFluxModel;
            }

            yield return WindFields;

            foreach (IWindField windField in WindFields)
            {
                yield return windField;
            }

            foreach (var spatialDataItem in SpatialData.DataItems)
            {
                yield return spatialDataItem;
            }

            yield return RestartInput;

            //for QueryTimeSeries tool:
            if (OutputHisFileStore != null)
            {
                foreach (IFunction function in OutputHisFileStore.Functions)
                {
                    yield return function;
                }
            }

            if (OutputMapFileStore != null)
            {
                foreach (IFunction function in OutputMapFileStore.Functions)
                {
                    yield return function;
                }
            }

            if (OutputClassMapFileStore != null)
            {
                foreach (IFunction function in OutputClassMapFileStore.Functions)
                {
                    yield return function;
                }
            }
        }

        /// <inheritdoc />
        public override IEnumerable<IFeature> GetChildDataItemLocations(DataItemRole role)
        {
            if (role.HasFlag(DataItemRole.Input))
            {
                return InputFeatureCollections.SelectMany(x => x);
            }

            if (role.HasFlag(DataItemRole.Output))
            {
                return OutputFeatureCollections.SelectMany(x => x);
            }

            return Enumerable.Empty<IFeature>();
        }

        /// <inheritdoc />
        public override IEnumerable<IDataItem> GetChildDataItems(IFeature location)
        {
            if (location == null)
            {
                yield break;
            }

            areaDataItems.TryGetValue(location, out List<IDataItem> items);

            if (items == null)
            {
                yield break;
            }

            foreach (IDataItem di in items)
            {
                yield return di;
            }
        }
    }
}