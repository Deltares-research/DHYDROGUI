using DelftTools.Utils.Aop;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    [Entity(FireOnCollectionChange = false)]
    public class PavedDataViewModel
    {
        private RainfallRunoffEnums.AreaUnit areaUnit;

        public PavedDataViewModel(PavedData data, RainfallRunoffEnums.AreaUnit areaUnit)
        {
            Data = data;
            AreaUnit = areaUnit;
        }

        public PavedData Data { get; set; } //public to bubble events to causes refreshes

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

        public bool SewerTypeIsMixed
        {
            get => Data.SewerType == PavedEnums.SewerType.MixedSystem;
            set
            {
                if (value)
                {
                    Data.SewerType = PavedEnums.SewerType.MixedSystem;
                }
            }
        }

        public bool SewerTypeIsNotMixed => !SewerTypeIsMixed;

        public bool SewerPumpCapacityIsFixed
        {
            get => Data.IsSewerPumpCapacityFixed;
            set => Data.IsSewerPumpCapacityFixed = value;
        }

        public bool SewerPumpCapacityIsVariable => !SewerPumpCapacityIsFixed;

        public bool SewerTypeIsNotMixedAndSewerPumpIsFixedCapacity => SewerTypeIsNotMixed && SewerPumpCapacityIsFixed;

        public bool SewerTypeIsNotMixedAndSewerPumpIsVariableCapacity => SewerTypeIsNotMixed && !SewerPumpCapacityIsFixed;

        public RainfallRunoffEnums.AreaUnit AreaUnit
        {
            get => areaUnit;
            set
            {
                areaUnit = value;
                AreaUnitLabel = value.GetDescription();
            }
        }

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

        public string AreaUnitLabel { get; private set; }

        public double TotalAreaInUnit
        {
            get => RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.m2, AreaUnit, Data.CalculationArea);
            set => Data.CalculationArea = RainfallRunoffUnitConverter.ConvertArea(AreaUnit, RainfallRunoffEnums.AreaUnit.m2, value);
        }

        public double CapacityMixedAndOrRainfall
        {
            get => RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedData.PumpCapacityUnit,
                                                                   PumpCapacityUnit,
                                                                   Data.CapacityMixedAndOrRainfall,
                                                                   Data.CalculationArea);
            set => Data.CapacityMixedAndOrRainfall = RainfallRunoffUnitConverter.ConvertPumpCapacity(PumpCapacityUnit,
                                                                                                     PavedData.PumpCapacityUnit,
                                                                                                     value,
                                                                                                     Data.CalculationArea);
        }

        public double CapacityDryWeatherFlow
        {
            get => RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedData.PumpCapacityUnit,
                                                                   PumpCapacityUnit,
                                                                   Data.CapacityDryWeatherFlow,
                                                                   Data.CalculationArea);
            set => Data.CapacityDryWeatherFlow = RainfallRunoffUnitConverter.ConvertPumpCapacity(PumpCapacityUnit,
                                                                                                 PavedData.PumpCapacityUnit,
                                                                                                 value,
                                                                                                 Data.CalculationArea);
        }

        public double MaximumStreetStorage
        {
            get => RainfallRunoffUnitConverter.ConvertStorage(PavedData.StorageUnit, StorageUnit,
                                                              Data.MaximumStreetStorage, Data.CalculationArea);
            set => Data.MaximumStreetStorage = RainfallRunoffUnitConverter.ConvertStorage(StorageUnit,
                                                                                          PavedData.StorageUnit,
                                                                                          value, Data.CalculationArea);
        }

        public double InitialStreetStorage
        {
            get => RainfallRunoffUnitConverter.ConvertStorage(PavedData.StorageUnit, StorageUnit,
                                                              Data.InitialStreetStorage, Data.CalculationArea);
            set => Data.InitialStreetStorage = RainfallRunoffUnitConverter.ConvertStorage(StorageUnit,
                                                                                          PavedData.StorageUnit,
                                                                                          value, Data.CalculationArea);
        }

        public double MaximumSewerMixedAndOrRainfallStorage
        {
            get => RainfallRunoffUnitConverter.ConvertStorage(PavedData.StorageUnit, StorageUnit,
                                                              Data.MaximumSewerMixedAndOrRainfallStorage,
                                                              Data.CalculationArea);
            set => Data.MaximumSewerMixedAndOrRainfallStorage = RainfallRunoffUnitConverter.ConvertStorage(StorageUnit,
                                                                                                           PavedData.StorageUnit,
                                                                                                           value,
                                                                                                           Data.CalculationArea);
        }

        public double InitialSewerMixedAndOrRainfallStorage
        {
            get => RainfallRunoffUnitConverter.ConvertStorage(PavedData.StorageUnit, StorageUnit,
                                                              Data.InitialSewerMixedAndOrRainfallStorage,
                                                              Data.CalculationArea);
            set => Data.InitialSewerMixedAndOrRainfallStorage = RainfallRunoffUnitConverter.ConvertStorage(StorageUnit,
                                                                                                           PavedData.StorageUnit,
                                                                                                           value, Data.CalculationArea);
        }

        public double MaximumSewerDryWeatherFlowStorage
        {
            get => RainfallRunoffUnitConverter.ConvertStorage(PavedData.StorageUnit, StorageUnit,
                                                              Data.MaximumSewerDryWeatherFlowStorage, Data.CalculationArea);
            set => Data.MaximumSewerDryWeatherFlowStorage = RainfallRunoffUnitConverter.ConvertStorage(StorageUnit,
                                                                                                       PavedData.StorageUnit,
                                                                                                       value,
                                                                                                       Data.CalculationArea);
        }

        public double InitialSewerDryWeatherFlowStorage
        {
            get => RainfallRunoffUnitConverter.ConvertStorage(PavedData.StorageUnit, StorageUnit,
                                                              Data.InitialSewerDryWeatherFlowStorage, Data.CalculationArea);
            set => Data.InitialSewerDryWeatherFlowStorage = RainfallRunoffUnitConverter.ConvertStorage(StorageUnit,
                                                                                                       PavedData.StorageUnit,
                                                                                                       value,
                                                                                                       Data.CalculationArea);
        }

        public double WaterUse
        {
            get => RainfallRunoffUnitConverter.ConvertWaterUse(PavedData.WaterUseUnit, WaterUseUnit, Data.WaterUse);
            set => Data.WaterUse = RainfallRunoffUnitConverter.ConvertWaterUse(WaterUseUnit, PavedData.WaterUseUnit, value);
        }
    }
}