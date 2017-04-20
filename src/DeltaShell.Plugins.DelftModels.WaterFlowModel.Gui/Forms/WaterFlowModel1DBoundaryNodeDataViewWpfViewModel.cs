using DelftTools.Utils.Aop;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms
{
    [Entity]
    public class WaterFlowModel1DBoundaryNodeDataViewWpfViewModel
    {
        private WaterFlowModel1DBoundaryNodeData waterFlowModel1DBoundaryNodeData = new WaterFlowModel1DBoundaryNodeData();

        #region Object model related properties

        public WaterFlowModel1DBoundaryNodeData WaterFlowModel1DBoundaryNodeData
        {
            get { return waterFlowModel1DBoundaryNodeData; }
            set
            {
                waterFlowModel1DBoundaryNodeData = value;
                if (waterFlowModel1DBoundaryNodeData == null) return;
                RefreshObjectModelRelatedData();
            }
        }

        public WaterFlowModel1DBoundaryNodeDataType BoundaryNodeDataType
        {
            get { return WaterFlowModel1DBoundaryNodeData.DataType; }
            set { WaterFlowModel1DBoundaryNodeData.DataType = value; }
        }

        public SaltBoundaryConditionType SaltConditionType
        {
            get { return WaterFlowModel1DBoundaryNodeData.SaltConditionType; }
            set { WaterFlowModel1DBoundaryNodeData.SaltConditionType = value; }
        }

        public TemperatureBoundaryConditionType TemperatureConditionType
        {
            get { return WaterFlowModel1DBoundaryNodeData.TemperatureConditionType; }
            set { WaterFlowModel1DBoundaryNodeData.TemperatureConditionType = value; }
        }

        public double WaterLevel
        {
            get { return WaterFlowModel1DBoundaryNodeData.WaterLevel; }
            set { WaterFlowModel1DBoundaryNodeData.WaterLevel = value; }
        }

        public double Flow
        {
            get { return WaterFlowModel1DBoundaryNodeData.Flow; }
            set { WaterFlowModel1DBoundaryNodeData.Flow = value; }
        }

        public double SaltConcentrationConstant
        {
            get { return WaterFlowModel1DBoundaryNodeData.SaltConcentrationConstant; }
            set { WaterFlowModel1DBoundaryNodeData.SaltConcentrationConstant = value; }
        }

        public double ThatcherHarlemannCoefficient
        {
            get { return WaterFlowModel1DBoundaryNodeData.ThatcherHarlemannCoefficient; }
            set { WaterFlowModel1DBoundaryNodeData.ThatcherHarlemannCoefficient = value; }
        }

        public double TemperatureConstant
        {
            get { return WaterFlowModel1DBoundaryNodeData.TemperatureConstant; }
            set { WaterFlowModel1DBoundaryNodeData.TemperatureConstant = value; }
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
            UseFlowConstantForFlowData = BoundaryNodeDataType == WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
            UseWaterLevelConstantForFlowData = BoundaryNodeDataType == WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
            UseTimeseriesForFlowData =
                BoundaryNodeDataType == WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries ||
                BoundaryNodeDataType == WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries ||
                BoundaryNodeDataType == WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable;

            // Setting FlowData Type to 'None' should disable ALL salinity and temperature data
            // (behaviour carried over from previous BoundaryNodeDataView)
            SalinityEnabled = BoundaryNodeDataType != WaterFlowModel1DBoundaryNodeDataType.None
                              && WaterFlowModel1DBoundaryNodeData.UseSalt;

            TemperatureEnabled = WaterFlowModel1DBoundaryNodeData.DataType != WaterFlowModel1DBoundaryNodeDataType.None
                                 && WaterFlowModel1DBoundaryNodeData.UseTemperature;
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
            Flow = WaterFlowModel1DBoundaryNodeData.Flow;
            WaterLevel = WaterFlowModel1DBoundaryNodeData.WaterLevel;
            SaltConcentrationConstant = WaterFlowModel1DBoundaryNodeData.SaltConcentrationConstant;
            ThatcherHarlemannCoefficient = WaterFlowModel1DBoundaryNodeData.ThatcherHarlemannCoefficient;
            TemperatureConstant = WaterFlowModel1DBoundaryNodeData.TemperatureConstant;

            BoundaryNodeDataType = WaterFlowModel1DBoundaryNodeData.DataType;
            SaltConditionType = WaterFlowModel1DBoundaryNodeData.SaltConditionType;
            TemperatureConditionType = WaterFlowModel1DBoundaryNodeData.TemperatureConditionType;
        }
    }
}