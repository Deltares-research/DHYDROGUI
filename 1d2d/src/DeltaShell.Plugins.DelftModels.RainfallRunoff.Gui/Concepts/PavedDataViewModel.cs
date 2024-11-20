using DelftTools.Utils.Aop;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    /// <summary>
    /// The view model for the <see cref="PavedDataView"/>.
    /// </summary>
    [Entity(FireOnCollectionChange = false)]
    public class PavedDataViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PavedDataViewModel"/> class.
        /// </summary>
        /// <param name="data">The paved data. </param>
        /// <param name="areaUnit"> The area unit. </param>
        public PavedDataViewModel(PavedData data, RainfallRunoffEnums.AreaUnit areaUnit)
        {
            Data = data;
            AreaUnit = areaUnit;
        }

        /// <summary>
        /// The paved data.
        /// </summary>
        public PavedData Data { get; set; } //public to bubble events to causes refreshes

        /// <summary>
        /// Whether or not the splitting definition is no delay.
        /// </summary>
        public bool SplittingDefinitionIsNoDelay
        {
            get => Data.SpillingDefinition == PavedEnums.SpillingDefinition.NoDelay;
            set
            {
                if (value)
                {
                    Data.SpillingDefinition = PavedEnums.SpillingDefinition.NoDelay;
                }
            }
        }

        /// <summary>
        /// Whether or not the splitting definition uses a runoff coefficient.
        /// </summary>
        public bool SplittingDefinitionUseRunoffCoefficient
        {
            get => Data.SpillingDefinition == PavedEnums.SpillingDefinition.UseRunoffCoefficient;
            set
            {
                if (value)
                {
                    Data.SpillingDefinition = PavedEnums.SpillingDefinition.UseRunoffCoefficient;
                }
            }
        }

        /// <summary>
        /// Whether or not the sewer is not a mixed system.
        /// </summary>
        public bool SewerTypeIsNotMixed => Data.SewerType != PavedEnums.SewerType.MixedSystem;

        /// <summary>
        /// Whether or not the pump capacity is variable over time.
        /// </summary>
        public bool SewerPumpCapacityIsVariable => !Data.IsSewerPumpCapacityFixed;

        /// <summary>
        /// Whether or not the sewer is not a mixed system and the pump capacity is fixed.
        /// </summary>
        public bool SewerTypeIsNotMixedAndSewerPumpIsFixedCapacity => SewerTypeIsNotMixed && !SewerPumpCapacityIsVariable;

        /// <summary>
        /// Whether or not the sewer is not a mixed system and the pump capacity is variable.
        /// </summary>
        public bool SewerTypeIsNotMixedAndSewerPumpIsVariableCapacity => SewerTypeIsNotMixed && SewerPumpCapacityIsVariable;

        /// <summary>
        /// The unit for the area.
        /// </summary>
        public RainfallRunoffEnums.AreaUnit AreaUnit { get; set; }

        /// <summary>
        /// The unit for the pump capacity.
        /// </summary>
        public PavedEnums.SewerPumpCapacityUnit PumpCapacityUnit { get; set; }

        /// <summary>
        /// The unit for the storage.
        /// </summary>
        public RainfallRunoffEnums.StorageUnit StorageUnit { get; set; }

        /// <summary>
        /// The unit for the water use.
        /// </summary>
        public PavedEnums.WaterUseUnit WaterUseUnit { get; set; }

        /// <summary>
        /// The unit label for the area.
        /// </summary>
        public string AreaUnitLabel => AreaUnit.GetDescription();

        /// <summary>
        /// The total area.
        /// </summary>
        public double TotalAreaInUnit
        {
            get => GetArea(Data.CalculationArea);
            set => Data.CalculationArea = GetConvertedArea(value);
        }

        /// <summary>
        /// The fixed capacity of a mixed/rainfall sewer pump.
        /// </summary>
        public double CapacityMixedAndOrRainfall
        {
            get => GetCapacity(Data.CapacityMixedAndOrRainfall);
            set => Data.CapacityMixedAndOrRainfall = GetConvertedCapacity(value);
        }

        /// <summary>
        /// The fixed capacity of a dry weather flow sewer pump.
        /// </summary>
        public double CapacityDryWeatherFlow
        {
            get => GetCapacity(Data.CapacityDryWeatherFlow);
            set => Data.CapacityDryWeatherFlow = GetConvertedCapacity(value);
        }

        /// <summary>
        /// The maximum street storage.
        /// </summary>
        public double MaximumStreetStorage
        {
            get => GetStorage(Data.MaximumStreetStorage);
            set => Data.MaximumStreetStorage = GetConvertedStorage(value);
        }

        /// <summary>
        /// The initial street storage.
        /// </summary>
        public double InitialStreetStorage
        {
            get => GetStorage(Data.InitialStreetStorage);
            set => Data.InitialStreetStorage = GetConvertedStorage(value);
        }

        /// <summary>
        /// The maximum mixed/rainfall sewer storage.
        /// </summary>
        public double MaximumSewerMixedAndOrRainfallStorage
        {
            get => GetStorage(Data.MaximumSewerMixedAndOrRainfallStorage);
            set => Data.MaximumSewerMixedAndOrRainfallStorage = GetConvertedStorage(value);
        }

        /// <summary>
        /// The initial mixed/rainfall sewer storage.
        /// </summary>
        public double InitialSewerMixedAndOrRainfallStorage
        {
            get => GetStorage(Data.InitialSewerMixedAndOrRainfallStorage);
            set => Data.InitialSewerMixedAndOrRainfallStorage = GetConvertedStorage(value);
        }

        /// <summary>
        /// The maximum dry weather flow sewer storage.
        /// </summary>
        public double MaximumSewerDryWeatherFlowStorage
        {
            get => GetStorage(Data.MaximumSewerDryWeatherFlowStorage);
            set => Data.MaximumSewerDryWeatherFlowStorage = GetConvertedStorage(value);
        }

        /// <summary>
        /// The initial dry weather flow sewer storage.
        /// </summary>
        public double InitialSewerDryWeatherFlowStorage
        {
            get => GetStorage(Data.InitialSewerDryWeatherFlowStorage);
            set => Data.InitialSewerDryWeatherFlowStorage = GetConvertedStorage(value);
        }

        /// <summary>
        /// The water use per capita.
        /// </summary>
        public double WaterUse
        {
            get => GetWaterUse(Data.WaterUse);
            set => Data.WaterUse = GetConvertedWaterUse(value);
        }

        private double GetArea(double value) =>
            RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.m2, AreaUnit, value);

        private double GetConvertedArea(double value) =>
            RainfallRunoffUnitConverter.ConvertArea(AreaUnit, RainfallRunoffEnums.AreaUnit.m2, value);

        private double GetStorage(double value) =>
            RainfallRunoffUnitConverter.ConvertStorage(PavedData.StorageUnit, StorageUnit, value, Data.CalculationArea, AreaUnit);

        private double GetConvertedStorage(double value) =>
            RainfallRunoffUnitConverter.ConvertStorage(StorageUnit, PavedData.StorageUnit, value, Data.CalculationArea, AreaUnit);

        private double GetCapacity(double value) =>
            RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedData.PumpCapacityUnit, PumpCapacityUnit, value, Data.CalculationArea, AreaUnit);

        private double GetConvertedCapacity(double value) =>
            RainfallRunoffUnitConverter.ConvertPumpCapacity(PumpCapacityUnit, PavedData.PumpCapacityUnit, value, Data.CalculationArea, AreaUnit);

        private double GetWaterUse(double value) =>
            RainfallRunoffUnitConverter.ConvertWaterUse(PavedData.WaterUseUnit, WaterUseUnit, value);

        private double GetConvertedWaterUse(double value) =>
            RainfallRunoffUnitConverter.ConvertWaterUse(WaterUseUnit, PavedData.WaterUseUnit, value);
    }
}