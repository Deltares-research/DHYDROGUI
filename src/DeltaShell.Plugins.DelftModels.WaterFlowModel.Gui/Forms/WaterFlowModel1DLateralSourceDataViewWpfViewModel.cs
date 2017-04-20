using DelftTools.Utils.Aop;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms
{
    [Entity]
    public class WaterFlowModel1DLateralSourceDataViewWpfViewModel
    {
        private WaterFlowModel1DLateralSourceData waterFlowModel1DLateralSourceData = new WaterFlowModel1DLateralSourceData();

        #region Object model related properties

        public WaterFlowModel1DLateralSourceData WaterFlowModel1DLateralSourceData
        {
            get { return waterFlowModel1DLateralSourceData; }
            set
            {
                waterFlowModel1DLateralSourceData = value;
                if (waterFlowModel1DLateralSourceData == null) return;
                RefreshObjectModelRelatedData();
            }
        }

        public WaterFlowModel1DLateralDataType LateralDischargeDataType
        {
            get { return WaterFlowModel1DLateralSourceData.DataType; }
            set { WaterFlowModel1DLateralSourceData.DataType = value; }
        }

        public SaltLateralDischargeType SaltLateralDischargeType
        {
            get { return WaterFlowModel1DLateralSourceData.SaltLateralDischargeType; }
            set { WaterFlowModel1DLateralSourceData.SaltLateralDischargeType = value; }
        }

        public TemperatureLateralDischargeType TemperatureDischargeType
        {
            get { return WaterFlowModel1DLateralSourceData.TemperatureLateralDischargeType; }
            set { WaterFlowModel1DLateralSourceData.TemperatureLateralDischargeType = value; }
        }

        public double Flow
        {
            get { return WaterFlowModel1DLateralSourceData.Flow; }
            set { WaterFlowModel1DLateralSourceData.Flow = value; }
        }

        public double SaltConcentrationConstant
        {
            get { return WaterFlowModel1DLateralSourceData.SaltConcentrationDischargeConstant; }
            set { WaterFlowModel1DLateralSourceData.SaltConcentrationDischargeConstant = value; }
        }
        
        public double SaltMassConstant
        {
            get { return WaterFlowModel1DLateralSourceData.SaltMassDischargeConstant; }
            set { WaterFlowModel1DLateralSourceData.SaltMassDischargeConstant = value; }
        }

        public double TemperatureConstant
        {
            get { return WaterFlowModel1DLateralSourceData.TemperatureConstant; }
            set { WaterFlowModel1DLateralSourceData.TemperatureConstant = value; }
        }

        #endregion

        #region View model related properties

        public bool UseFlowConstantForFlowData { get; set; }
        public bool UseTimeseriesForFlowData { get; set; }

        public bool UseConcentrationConstantForSalinityData { get; set; }
        public bool UseMassConstantForSalinityData { get; set; }
        public bool UseConcentrationTimeseriesForSalinityData { get; set; }
        public bool UseMassTimeseriesForSalinityData { get; set; }

        public bool UseConstantForTemperatureData { get; set; }
        public bool UseTimeseriesForTemperatureData { get; set; }

        public bool SalinityEnabled { get; set; }
        public bool TemperatureEnabled { get; set; }
        
        #endregion
        
        #region Update view model logic

        public void UpdateFlowDataViewTab()
        {
            UseFlowConstantForFlowData = LateralDischargeDataType == WaterFlowModel1DLateralDataType.FlowConstant;
            UseTimeseriesForFlowData =
                LateralDischargeDataType == WaterFlowModel1DLateralDataType.FlowTimeSeries ||
                LateralDischargeDataType == WaterFlowModel1DLateralDataType.FlowWaterLevelTable;
        }

        public void UpdateSalinityDataViewTab()
        {
            UseConcentrationConstantForSalinityData = SaltLateralDischargeType == SaltLateralDischargeType.ConcentrationConstant;
            UseMassConstantForSalinityData = SaltLateralDischargeType == SaltLateralDischargeType.MassConstant;

            UseConcentrationTimeseriesForSalinityData = SaltLateralDischargeType == SaltLateralDischargeType.ConcentrationTimeSeries;
            UseMassTimeseriesForSalinityData = SaltLateralDischargeType == SaltLateralDischargeType.MassTimeSeries;
        }

        public void UpdateTemperatureDataViewTab()
        {
            UseConstantForTemperatureData = TemperatureDischargeType == TemperatureLateralDischargeType.Constant;
            UseTimeseriesForTemperatureData = TemperatureDischargeType == TemperatureLateralDischargeType.TimeDependent;
        }

        #endregion

        private void RefreshObjectModelRelatedData()
        {
            Flow = WaterFlowModel1DLateralSourceData.Flow;
            SaltConcentrationConstant = WaterFlowModel1DLateralSourceData.SaltConcentrationDischargeConstant;
            SaltMassConstant = WaterFlowModel1DLateralSourceData.SaltMassDischargeConstant;
            TemperatureConstant = WaterFlowModel1DLateralSourceData.TemperatureConstant;

            LateralDischargeDataType = WaterFlowModel1DLateralSourceData.DataType;
            SaltLateralDischargeType = WaterFlowModel1DLateralSourceData.SaltLateralDischargeType;
            TemperatureDischargeType = WaterFlowModel1DLateralSourceData.TemperatureLateralDischargeType;

            SalinityEnabled = WaterFlowModel1DLateralSourceData.UseSalt;
            TemperatureEnabled = WaterFlowModel1DLateralSourceData.UseTemperature;
        }
    }
}