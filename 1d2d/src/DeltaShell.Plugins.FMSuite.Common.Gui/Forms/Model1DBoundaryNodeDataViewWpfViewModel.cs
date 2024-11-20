using DelftTools.Utils.Aop;
using DeltaShell.NGHS.IO.DataObjects;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Forms
{
    [Entity]
    public class Model1DBoundaryNodeDataViewWpfViewModel
    {
        private Model1DBoundaryNodeData model1DBoundaryNodeData = new Model1DBoundaryNodeData();

        #region Object model related properties

        public Model1DBoundaryNodeData Model1DBoundaryNodeData
        {
            get { return model1DBoundaryNodeData; }
            set
            {
                model1DBoundaryNodeData = value;
                if (model1DBoundaryNodeData == null) return;
                RefreshObjectModelRelatedData();
            }
        }

        public Model1DBoundaryNodeDataType BoundaryNodeDataType
        {
            get { return Model1DBoundaryNodeData.DataType; }
            set { Model1DBoundaryNodeData.DataType = value; }
        }

        public SaltBoundaryConditionType SaltConditionType
        {
            get { return Model1DBoundaryNodeData.SaltConditionType; }
            set { Model1DBoundaryNodeData.SaltConditionType = value; }
        }

        public TemperatureBoundaryConditionType TemperatureConditionType
        {
            get { return Model1DBoundaryNodeData.TemperatureConditionType; }
            set { Model1DBoundaryNodeData.TemperatureConditionType = value; }
        }

        public double WaterLevel
        {
            get { return Model1DBoundaryNodeData.WaterLevel; }
            set { Model1DBoundaryNodeData.WaterLevel = value; }
        }

        public double Flow
        {
            get { return Model1DBoundaryNodeData.Flow; }
            set { Model1DBoundaryNodeData.Flow = value; }
        }

        public double SaltConcentrationConstant
        {
            get { return Model1DBoundaryNodeData.SaltConcentrationConstant; }
            set { Model1DBoundaryNodeData.SaltConcentrationConstant = value; }
        }

        public double ThatcherHarlemannCoefficient
        {
            get { return Model1DBoundaryNodeData.ThatcherHarlemannCoefficient; }
            set { Model1DBoundaryNodeData.ThatcherHarlemannCoefficient = value; }
        }

        public double TemperatureConstant
        {
            get { return Model1DBoundaryNodeData.TemperatureConstant; }
            set { Model1DBoundaryNodeData.TemperatureConstant = value; }
        }

        #endregion

        #region View model related properties

        public bool UseFlowConstantForFlowData { get; set; }
        public bool UseWaterLevelConstantForFlowData { get; set; }
        public bool UseTimeseriesForFlowData { get; set; }

        public bool UseConstantForSalinityData { get; set; }
        public bool UseConstantOrTimeSeriesForSalinityData { get; set; } // ThatcherHarleman
        public bool UseTimeseriesForSalinityData { get; set; }

        public bool UseConstantForTemperatureData { get; set; }
        public bool UseTimeseriesForTemperatureData { get; set; }

        public bool SalinityEnabled { get; set; }
        public bool TemperatureEnabled { get; set; }

        #endregion

        #region Update view model logic

        public void UpdateFlowDataViewTab()
        {
            UseFlowConstantForFlowData = BoundaryNodeDataType == Model1DBoundaryNodeDataType.FlowConstant;
            UseWaterLevelConstantForFlowData = BoundaryNodeDataType == Model1DBoundaryNodeDataType.WaterLevelConstant;
            UseTimeseriesForFlowData =
                BoundaryNodeDataType == Model1DBoundaryNodeDataType.FlowTimeSeries ||
                BoundaryNodeDataType == Model1DBoundaryNodeDataType.WaterLevelTimeSeries ||
                BoundaryNodeDataType == Model1DBoundaryNodeDataType.FlowWaterLevelTable;

            // Setting FlowData Type to 'None' should disable ALL salinity and temperature data
            // (behaviour carried over from previous BoundaryNodeDataView)
            SalinityEnabled = BoundaryNodeDataType != Model1DBoundaryNodeDataType.None
                              && Model1DBoundaryNodeData.UseSalt;

            TemperatureEnabled = Model1DBoundaryNodeData.DataType != Model1DBoundaryNodeDataType.None
                                 && Model1DBoundaryNodeData.UseTemperature;
        }

        public void UpdateSalinityDataViewTab()
        {
            UseConstantForSalinityData = SaltConditionType == SaltBoundaryConditionType.Constant;
            UseConstantOrTimeSeriesForSalinityData = SaltConditionType == SaltBoundaryConditionType.Constant
                || SaltConditionType == SaltBoundaryConditionType.TimeDependent;

            UseTimeseriesForSalinityData = SaltConditionType == SaltBoundaryConditionType.TimeDependent;
        }

        public void UpdateTemperatureDataViewTab()
        {
            UseConstantForTemperatureData = TemperatureConditionType == TemperatureBoundaryConditionType.Constant;
            UseTimeseriesForTemperatureData = TemperatureConditionType == TemperatureBoundaryConditionType.TimeDependent;
        }

        #endregion
        
        private void RefreshObjectModelRelatedData()
        {
            Flow = Model1DBoundaryNodeData.Flow;
            WaterLevel = Model1DBoundaryNodeData.WaterLevel;
            SaltConcentrationConstant = Model1DBoundaryNodeData.SaltConcentrationConstant;
            ThatcherHarlemannCoefficient = Model1DBoundaryNodeData.ThatcherHarlemannCoefficient;
            TemperatureConstant = Model1DBoundaryNodeData.TemperatureConstant;

            BoundaryNodeDataType = Model1DBoundaryNodeData.DataType;
            SaltConditionType = Model1DBoundaryNodeData.SaltConditionType;
            TemperatureConditionType = Model1DBoundaryNodeData.TemperatureConditionType;
        }
    }
}