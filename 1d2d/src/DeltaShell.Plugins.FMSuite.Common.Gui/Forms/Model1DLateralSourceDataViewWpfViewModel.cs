using DelftTools.Utils.Aop;
using DeltaShell.NGHS.IO.DataObjects;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Forms
{
    [Entity]
    public class Model1DLateralSourceDataViewWpfViewModel
    {
        private Model1DLateralSourceData model1DLateralSourceData = new Model1DLateralSourceData();

        #region Object model related properties

        public Model1DLateralSourceData Model1DLateralSourceData
        {
            get { return model1DLateralSourceData; }
            set
            {
                model1DLateralSourceData = value;
                if (model1DLateralSourceData == null) return;
                RefreshObjectModelRelatedData();
            }
        }

        public Model1DLateralDataType LateralDischargeDataType
        {
            get { return Model1DLateralSourceData.DataType; }
            set { Model1DLateralSourceData.DataType = value; }
        }

        public SaltLateralDischargeType SaltLateralDischargeType
        {
            get { return Model1DLateralSourceData.SaltLateralDischargeType; }
            set { Model1DLateralSourceData.SaltLateralDischargeType = value; }
        }

        public TemperatureLateralDischargeType TemperatureDischargeType
        {
            get { return Model1DLateralSourceData.TemperatureLateralDischargeType; }
            set { Model1DLateralSourceData.TemperatureLateralDischargeType = value; }
        }

        public double Flow
        {
            get { return Model1DLateralSourceData.Flow; }
            set { Model1DLateralSourceData.Flow = value; }
        }

        public double SaltConcentrationConstant
        {
            get { return Model1DLateralSourceData.SaltConcentrationDischargeConstant; }
            set { Model1DLateralSourceData.SaltConcentrationDischargeConstant = value; }
        }
        
        public double SaltMassConstant
        {
            get { return Model1DLateralSourceData.SaltMassDischargeConstant; }
            set { Model1DLateralSourceData.SaltMassDischargeConstant = value; }
        }

        public double TemperatureConstant
        {
            get { return Model1DLateralSourceData.TemperatureConstant; }
            set { Model1DLateralSourceData.TemperatureConstant = value; }
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
            UseFlowConstantForFlowData = LateralDischargeDataType == Model1DLateralDataType.FlowConstant;
            UseTimeseriesForFlowData =
                LateralDischargeDataType == Model1DLateralDataType.FlowTimeSeries ||
                LateralDischargeDataType == Model1DLateralDataType.FlowWaterLevelTable;
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
            Flow = Model1DLateralSourceData.Flow;
            SaltConcentrationConstant = Model1DLateralSourceData.SaltConcentrationDischargeConstant;
            SaltMassConstant = Model1DLateralSourceData.SaltMassDischargeConstant;
            TemperatureConstant = Model1DLateralSourceData.TemperatureConstant;

            LateralDischargeDataType = Model1DLateralSourceData.DataType;
            SaltLateralDischargeType = Model1DLateralSourceData.SaltLateralDischargeType;
            TemperatureDischargeType = Model1DLateralSourceData.TemperatureLateralDischargeType;

            SalinityEnabled = Model1DLateralSourceData.UseSalt;
            TemperatureEnabled = Model1DLateralSourceData.UseTemperature;
        }
    }
}