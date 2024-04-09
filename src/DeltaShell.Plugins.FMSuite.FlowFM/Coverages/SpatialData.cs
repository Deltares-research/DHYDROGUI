using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Coverages
{
    /// <summary>
    /// This <see cref="SpatialData"/> manages the spatial coverages available in D-Flow FM.
    /// </summary>
    public sealed class SpatialData : ISpatialData
    {
        private readonly IWaterFlowFMModel model;
        private readonly IDataItem bathymetryDataItem;
        private readonly IDataItem initialWaterLevelDataItem;
        private readonly IDataItem initialSalinityDataItem;
        private readonly IDataItem initialTemperatureDataItem;
        private readonly IDataItem viscosityDataItem;
        private readonly IDataItem diffusivityDataItem;
        private readonly IDataItem roughnessDataItem;
        private readonly IList<IDataItem> tracerDataItems;
        private readonly IList<IDataItem> fractionDataItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpatialData"/> class.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="model"/> is <c>null</c>.
        /// </exception>
        public SpatialData(IWaterFlowFMModel model)
        {
            Ensure.NotNull(model, nameof(model));

            this.model = model;

            bathymetryDataItem = CreateDataItem<UnstructuredGridCoverage>(WaterFlowFMModelDefinition.BathymetryDataItemName);
            initialWaterLevelDataItem = CreateDataItem<UnstructuredGridCellCoverage>(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName);
            initialSalinityDataItem = CreateDataItem<UnstructuredGridCellCoverage>(WaterFlowFMModelDefinition.InitialSalinityDataItemName);
            initialTemperatureDataItem = CreateDataItem<UnstructuredGridCellCoverage>(WaterFlowFMModelDefinition.InitialTemperatureDataItemName);
            viscosityDataItem = CreateDataItem<UnstructuredGridFlowLinkCoverage>(WaterFlowFMModelDefinition.ViscosityDataItemName);
            diffusivityDataItem = CreateDataItem<UnstructuredGridFlowLinkCoverage>(WaterFlowFMModelDefinition.DiffusivityDataItemName);
            roughnessDataItem = CreateDataItem<UnstructuredGridFlowLinkCoverage>(WaterFlowFMModelDefinition.RoughnessDataItemName);
            tracerDataItems = new List<IDataItem>();
            fractionDataItems = new List<IDataItem>();

            DataItems = new EventedList<IDataItem>
            {
                bathymetryDataItem,
                initialWaterLevelDataItem,
                initialSalinityDataItem,
                initialTemperatureDataItem,
                roughnessDataItem,
                viscosityDataItem,
                diffusivityDataItem
            };
        }

        public UnstructuredGridCoverage Bathymetry
        {
            get => (UnstructuredGridCoverage) bathymetryDataItem.Value;
            set
            {
                bathymetryDataItem.Value = value;
                model.ModelDefinition.Bathymetry = Bathymetry;
            }
        }

        public UnstructuredGridCellCoverage InitialWaterLevel
        {
            get => (UnstructuredGridCellCoverage) initialWaterLevelDataItem.Value;
            set => initialWaterLevelDataItem.Value = value;
        }

        public UnstructuredGridCellCoverage InitialSalinity
        {
            get => (UnstructuredGridCellCoverage) initialSalinityDataItem.Value;
            set => initialSalinityDataItem.Value = value;
        }

        public UnstructuredGridCellCoverage InitialTemperature
        {
            get => (UnstructuredGridCellCoverage) initialTemperatureDataItem.Value;
            set => initialTemperatureDataItem.Value = value;
        }

        public UnstructuredGridFlowLinkCoverage Roughness
        {
            get => (UnstructuredGridFlowLinkCoverage) roughnessDataItem.Value;
            set => roughnessDataItem.Value = value;
        }

        public UnstructuredGridFlowLinkCoverage Viscosity
        {
            get => (UnstructuredGridFlowLinkCoverage) viscosityDataItem.Value;
            set => viscosityDataItem.Value = value;
        }

        public UnstructuredGridFlowLinkCoverage Diffusivity
        {
            get => (UnstructuredGridFlowLinkCoverage) diffusivityDataItem.Value;
            set => diffusivityDataItem.Value = value;
        }

        public IEnumerable<UnstructuredGridCellCoverage> InitialTracers => tracerDataItems.Select(d => d.Value).Cast<UnstructuredGridCellCoverage>();

        public IEnumerable<UnstructuredGridCellCoverage> InitialFractions => fractionDataItems.Select(d => d.Value).Cast<UnstructuredGridCellCoverage>();

        public IEventedList<IDataItem> DataItems { get; }

        public void AddTracer(UnstructuredGridCellCoverage coverage)
        {
            Ensure.NotNull(coverage, nameof(coverage));
            Ensure.NotNullOrEmpty(coverage.Name, nameof(coverage.Name));

            AddCoverage(tracerDataItems, coverage);
        }

        public void RemoveTracer(string name)
        {
            RemoveDataItem(tracerDataItems, name);
        }

        public void AddFraction(UnstructuredGridCellCoverage coverage)
        {
            Ensure.NotNull(coverage, nameof(coverage));
            Ensure.NotNullOrEmpty(coverage.Name, nameof(coverage.Name));

            AddCoverage(fractionDataItems, coverage);
        }

        public void RemoveFraction(string name)
        {
            RemoveDataItem(fractionDataItems, name);
        }

        private void RemoveDataItem(ICollection<IDataItem> dataItemSource, string name)
        {
            IDataItem dataItem = dataItemSource.GetByName(name);
            if (dataItem == null)
            {
                return;
            }

            dataItemSource.Remove(dataItem);
            DataItems.Remove(dataItem);
        }

        private void AddCoverage(ICollection<IDataItem> dataItemSource, UnstructuredGridCoverage coverage)
        {
            if (dataItemSource.GetByName(coverage.Name) != null)
            {
                return;
            }

            IDataItem dataItem = CreateDataItem(coverage.Name, coverage);

            dataItemSource.Add(dataItem);
            DataItems.Add(dataItem);
        }

        private IDataItem CreateDataItem<T>(string name, T value = default(T)) =>
            new DataItem(value, name, typeof(T), DataItemRole.Input, string.Empty) {Owner = model};
    }
}