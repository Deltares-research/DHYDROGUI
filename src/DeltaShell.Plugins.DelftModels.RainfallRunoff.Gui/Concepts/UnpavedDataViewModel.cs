using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    [Entity(FireOnCollectionChange=false)]
    public class UnpavedDataViewModel
    {
        private RainfallRunoffEnums.AreaUnit areaUnit;

        public UnpavedDataViewModel(UnpavedData data, RainfallRunoffEnums.AreaUnit areaUnit)
        {
            Data = data;
            AreaUnit = areaUnit;
        }

        public UnpavedData Data { get; set; } //public to bubble events to causes refreshes

        public bool GroundWaterLevelIsConstant
        {
            get { return Data.InitialGroundWaterLevelSource == UnpavedEnums.GroundWaterSourceType.Constant; }
            set
            {
                if (value)
                {
                    Data.InitialGroundWaterLevelSource = UnpavedEnums.GroundWaterSourceType.Constant;
                }
            }
        }

        public bool ModelRunningParallelWithFlow { get; set; }

        private bool LinkedToRunoffBoundary
        {
            get { return Data.Catchment.Links.Any() && Data.Catchment.Links.First().Target is RunoffBoundary; }
        }

        private bool LinkedToFlowNode
        {
            get { return Data.Catchment.Links.Any() && !LinkedToRunoffBoundary; }
        }
        
        public bool UseWaterLevelFromLinkedNode
        {
            get { return LinkedToRunoffBoundary || (LinkedToFlowNode && ModelRunningParallelWithFlow); }
        }

        public bool OverwriteWaterLevelBoundaryEnabled
        {
            get { return !UseWaterLevelFromLinkedNode; }
        }

        public bool GroundWaterLevelIsFromLinkedNode
        {
            get { return Data.InitialGroundWaterLevelSource == UnpavedEnums.GroundWaterSourceType.FromLinkedNode; }
            set
            {
                if (value)
                {
                    Data.InitialGroundWaterLevelSource = UnpavedEnums.GroundWaterSourceType.FromLinkedNode;
                }
            }
        }

        public bool GroundWaterLevelIsSeries
        {
            get { return Data.InitialGroundWaterLevelSource == UnpavedEnums.GroundWaterSourceType.Series; }
            set
            {
                if (value)
                {
                    Data.InitialGroundWaterLevelSource = UnpavedEnums.GroundWaterSourceType.Series;
                }
            }
        }

        public bool SeepageIsConstant
        {
            get { return Data.SeepageSource == UnpavedEnums.SeepageSourceType.Constant; }
            set
            {
                if (value)
                {
                    Data.SeepageSource = UnpavedEnums.SeepageSourceType.Constant;
                }
            }
        }

        public bool SeepageIsSeries
        {
            get { return Data.SeepageSource == UnpavedEnums.SeepageSourceType.Series; }
            set
            {
                if (value)
                {
                    Data.SeepageSource = UnpavedEnums.SeepageSourceType.Series;
                }
            }
        }

        public bool SeepageIsH0Series
        {
            get { return Data.SeepageSource == UnpavedEnums.SeepageSourceType.H0Series; }
            set
            {
                if (value)
                {
                    Data.SeepageSource = UnpavedEnums.SeepageSourceType.H0Series;
                }
            }
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

        public RainfallRunoffEnums.StorageUnit StorageUnit
        {
            get { return Data.LandStorageUnit; }
            set { Data.LandStorageUnit = value; }
        }

        public RainfallRunoffEnums.RainfallCapacityUnit InfiltrationCapacityUnit
        {
            get { return Data.InfiltrationCapacityUnit; }
            set { Data.InfiltrationCapacityUnit = value; }
        }

        public string AreaUnitLabel { get; private set; }

        public double TotalAreaForGroundWaterCalculations
        {
            get
            {
                return RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.m2, AreaUnit,
                                                               Data.TotalAreaForGroundWaterCalculations);
            }
            set
            {
                Data.TotalAreaForGroundWaterCalculations = RainfallRunoffUnitConverter.ConvertArea(AreaUnit,
                                                                                                   RainfallRunoffEnums.
                                                                                                       AreaUnit.m2,
                                                                                                   value);
            }
        }

        public double MaximumLandStorage
        {
            get
            {
                return RainfallRunoffUnitConverter.ConvertStorage(RainfallRunoffEnums.StorageUnit.mm, StorageUnit,
                                                                  Data.MaximumLandStorage,
                                                                  Data.TotalAreaForGroundWaterCalculations);
            }
            set
            {
                Data.MaximumLandStorage = RainfallRunoffUnitConverter.ConvertStorage(StorageUnit,
                                                                                     RainfallRunoffEnums.StorageUnit.mm,
                                                                                     value,
                                                                                     Data.
                                                                                         TotalAreaForGroundWaterCalculations);
            }
        }

        public double InitialLandStorage
        {
            get
            {
                return RainfallRunoffUnitConverter.ConvertStorage(RainfallRunoffEnums.StorageUnit.mm, StorageUnit,
                                                                  Data.InitialLandStorage,
                                                                  Data.TotalAreaForGroundWaterCalculations);
            }
            set
            {
                Data.InitialLandStorage = RainfallRunoffUnitConverter.ConvertStorage(StorageUnit,
                                                                                     RainfallRunoffEnums.StorageUnit.mm,
                                                                                     value,
                                                                                     Data.
                                                                                         TotalAreaForGroundWaterCalculations);
            }
        }

        public double InfiltrationCapacity
        {
            get
            {
                return RainfallRunoffUnitConverter.ConvertRainfall(RainfallRunoffEnums.RainfallCapacityUnit.mm_hr,
                                                                   InfiltrationCapacityUnit, Data.InfiltrationCapacity);
            }
            set
            {
                Data.InfiltrationCapacity = RainfallRunoffUnitConverter.ConvertRainfall(InfiltrationCapacityUnit,
                                                                                        RainfallRunoffEnums.RainfallCapacityUnit.mm_hr, 
                                                                                        value);
            }
        }

        public string AreaPerCropTypeLabel
        {
            get
            {
                var converter = TypeDescriptor.GetConverter(typeof (RainfallRunoffEnums.AreaUnit));
                return "Area per crop type in " + converter.ConvertToString(areaUnit);
            }
        }
    }
}