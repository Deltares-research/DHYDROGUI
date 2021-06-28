using DelftTools.Utils.Aop;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    [Entity(FireOnCollectionChange=false)]
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
            get { return Data.SpillingDefinition == PavedEnums.SpillingDefinition.NoDelay; }
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
            get { return Data.SpillingDefinition == PavedEnums.SpillingDefinition.UseRunoffCoefficient; }
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
            get { return Data.SewerType == PavedEnums.SewerType.MixedSystem; }
            set
            {
                if (value)
                {
                    Data.SewerType = PavedEnums.SewerType.MixedSystem;
                }
            }
        }

        public bool SewerTypeIsNotMixed
        {
            get { return !SewerTypeIsMixed; }
        }

        public bool SewerPumpCapacityIsFixed
        {
            //todo:
            get { return Data.IsSewerPumpCapacityFixed; }
            set { Data.IsSewerPumpCapacityFixed = value; }
        }

        public bool SewerPumpCapacityIsVariable
        {
            get { return !SewerPumpCapacityIsFixed; }
        }

        public bool SewerTypeIsNotMixedAndSewerPumpIsFixedCapacity
        {
            get { return SewerTypeIsNotMixed && SewerPumpCapacityIsFixed; }
        }

        public bool SewerTypeIsNotMixedAndSewerPumpIsVariableCapacity
        {
            get { return SewerTypeIsNotMixed && !SewerPumpCapacityIsFixed; }
        }

        public RainfallRunoffEnums.AreaUnit AreaUnit
        {
            get { return areaUnit; }
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

        public RainfallRunoffEnums.StorageUnit StorageUnit 
        {
            get { return Data.StorageUnit; }
            set { Data.StorageUnit = value; }
        }

        public PavedEnums.WaterUseUnit WaterUseUnit
        {
            get { return Data.WaterUseUnit; }
            set { Data.WaterUseUnit = value; }
        }

        public string AreaUnitLabel { get; private set; }

        public double TotalAreaInUnit
        {
            get { return RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.m2, AreaUnit, Data.CalculationArea); }
            set
            {
                Data.CalculationArea = RainfallRunoffUnitConverter.ConvertArea(AreaUnit, RainfallRunoffEnums.AreaUnit.m2,
                                                                         value);
            }
        }

        public double CapacityMixedAndOrRainfall
        {
            get
            {
                return RainfallRunoffUnitConverter.ConvertPumpCapacity(Data.SewerPumpCapacityUnit,
                                                                       PumpCapacityUnit,
                                                                       Data.CapacityMixedAndOrRainfall,
                                                                       Data.CalculationArea);

            }
            set
            {
                Data.CapacityMixedAndOrRainfall = RainfallRunoffUnitConverter.ConvertPumpCapacity(PumpCapacityUnit,
                                                                     Data.SewerPumpCapacityUnit,
                                                                     value,
                                                                     Data.CalculationArea);
            }
        }

        public double CapacityDryWeatherFlow
        {
            get
            {
                return RainfallRunoffUnitConverter.ConvertPumpCapacity(Data.SewerPumpCapacityUnit,
                                                     PumpCapacityUnit,
                                                     Data.CapacityDryWeatherFlow,
                                                     Data.CalculationArea);
            }
            set
            {
                Data.CapacityDryWeatherFlow = RainfallRunoffUnitConverter.ConvertPumpCapacity(PumpCapacityUnit,
                                                                   Data.SewerPumpCapacityUnit,
                                                                   value,
                                                                   Data.CalculationArea);
            }
        }

        public double MaximumStreetStorage
        {
            get
            {
                return RainfallRunoffUnitConverter.ConvertStorage(RainfallRunoffEnums.StorageUnit.mm, StorageUnit,
                                                                  Data.MaximumStreetStorage, Data.CalculationArea);
            }
            set
            {
                Data.MaximumStreetStorage = RainfallRunoffUnitConverter.ConvertStorage(StorageUnit,
                                                                                       RainfallRunoffEnums.StorageUnit.
                                                                                           mm,
                                                                                       value, Data.CalculationArea);
            }
        }

        public double InitialStreetStorage
        {
            get
            {
                return RainfallRunoffUnitConverter.ConvertStorage(RainfallRunoffEnums.StorageUnit.mm, StorageUnit,
                                                                  Data.InitialStreetStorage, Data.CalculationArea);
            }
            set
            {
                Data.InitialStreetStorage = RainfallRunoffUnitConverter.ConvertStorage(StorageUnit,
                                                                                       RainfallRunoffEnums.StorageUnit.
                                                                                           mm,
                                                                                       value, Data.CalculationArea);
            }
        }

        public double MaximumSewerMixedAndOrRainfallStorage
        {
            get
            {
                return RainfallRunoffUnitConverter.ConvertStorage(RainfallRunoffEnums.StorageUnit.mm, StorageUnit,
                                                                  Data.MaximumSewerMixedAndOrRainfallStorage,
                                                                  Data.CalculationArea);
            }
            set
            {
                Data.MaximumSewerMixedAndOrRainfallStorage = RainfallRunoffUnitConverter.ConvertStorage(StorageUnit,
                                                                                                        RainfallRunoffEnums
                                                                                                            .StorageUnit
                                                                                                            .mm,
                                                                                                        value,
                                                                                                        Data.CalculationArea);
            }
        }

        public double InitialSewerMixedAndOrRainfallStorage
        {
            get
            {
                return RainfallRunoffUnitConverter.ConvertStorage(RainfallRunoffEnums.StorageUnit.mm, StorageUnit,
                                                                  Data.InitialSewerMixedAndOrRainfallStorage,
                                                                  Data.CalculationArea);
            }
            set
            {
                Data.InitialSewerMixedAndOrRainfallStorage = RainfallRunoffUnitConverter.ConvertStorage(StorageUnit,
                                                                                                        RainfallRunoffEnums.StorageUnit.mm,
                                                                                                        value, Data.CalculationArea);
            }
        }

        public double MaximumSewerDryWeatherFlowStorage
        {
            get
            {
                return RainfallRunoffUnitConverter.ConvertStorage(RainfallRunoffEnums.StorageUnit.mm, StorageUnit,
                                                                  Data.MaximumSewerDryWeatherFlowStorage, Data.CalculationArea);
            }
            set
            {
                Data.MaximumSewerDryWeatherFlowStorage = RainfallRunoffUnitConverter.ConvertStorage(StorageUnit,
                                                                                                    RainfallRunoffEnums.
                                                                                                        StorageUnit.mm,
                                                                                                    value,
                                                                                                    Data.CalculationArea);
            }
        }

        public double InitialSewerDryWeatherFlowStorage
        {
            get
            {
                return RainfallRunoffUnitConverter.ConvertStorage(RainfallRunoffEnums.StorageUnit.mm, StorageUnit,
                                                                  Data.InitialSewerDryWeatherFlowStorage, Data.CalculationArea);
            }
            set
            {
                Data.InitialSewerDryWeatherFlowStorage = RainfallRunoffUnitConverter.ConvertStorage(StorageUnit,
                                                                                                    RainfallRunoffEnums.
                                                                                                        StorageUnit.mm,
                                                                                                    value,
                                                                                                    Data.CalculationArea);
            }
        }

        public double WaterUse
        {
            get
            {
                return RainfallRunoffUnitConverter.ConvertWaterUse(PavedEnums.WaterUseUnit.l_day, WaterUseUnit,
                                                                   Data.WaterUse);
            }
            set
            {
                Data.WaterUse = RainfallRunoffUnitConverter.ConvertWaterUse(WaterUseUnit, PavedEnums.WaterUseUnit.l_day,
                                                                            value);
            }
        }
    }
}