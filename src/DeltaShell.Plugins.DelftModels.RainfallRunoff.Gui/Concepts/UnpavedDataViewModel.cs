using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    /// <summary>
    /// View model for the <see cref="UnpavedDataView"/>.
    /// </summary>
    [Entity(FireOnCollectionChange = false)]
    public class UnpavedDataViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnpavedDataViewModel"/> class.
        /// </summary>
        /// <param name="data">The unpaved data. </param>
        /// <param name="areaUnit"> The area unit. </param>
        public UnpavedDataViewModel(UnpavedData data, RainfallRunoffEnums.AreaUnit areaUnit)
        {
            Data = data;
            AreaUnit = areaUnit;
        }

        /// <summary>
        /// The unpaved data.
        /// </summary>
        public UnpavedData Data { get; set; } //public to bubble events to causes refreshes

        /// <summary>
        /// Whether or not the ground water level is constant.
        /// </summary>
        public bool GroundWaterLevelIsConstant
        {
            get => Data.InitialGroundWaterLevelSource == UnpavedEnums.GroundWaterSourceType.Constant;
            set
            {
                if (value)
                {
                    Data.InitialGroundWaterLevelSource = UnpavedEnums.GroundWaterSourceType.Constant;
                }
            }
        }

        /// <summary>
        /// Whether or not the <see cref="RainfallRunoffModel"/> is running in parallel.
        /// </summary>
        public bool ModelRunningParallelWithFlow { get; set; }

        /// <summary>
        /// Whether or not to use the water level from the linked node.
        /// </summary>
        public bool UseWaterLevelFromLinkedNode => LinkedToRunoffBoundary || LinkedToFlowNode && ModelRunningParallelWithFlow;

        /// <summary>
        /// Whether or not to overwrite the water level boundary.
        /// </summary>
        public bool OverwriteWaterLevelBoundaryEnabled => !UseWaterLevelFromLinkedNode;

        /// <summary>
        /// Whether or not the ground water level is from the linked node.
        /// </summary>
        public bool GroundWaterLevelIsFromLinkedNode
        {
            get => Data.InitialGroundWaterLevelSource == UnpavedEnums.GroundWaterSourceType.FromLinkedNode;
            set
            {
                if (value)
                {
                    Data.InitialGroundWaterLevelSource = UnpavedEnums.GroundWaterSourceType.FromLinkedNode;
                }
            }
        }

        /// <summary>
        /// Whether or not the ground water level is variable over time.
        /// </summary>
        public bool GroundWaterLevelIsSeries
        {
            get => Data.InitialGroundWaterLevelSource == UnpavedEnums.GroundWaterSourceType.Series;
            set
            {
                if (value)
                {
                    Data.InitialGroundWaterLevelSource = UnpavedEnums.GroundWaterSourceType.Series;
                }
            }
        }

        /// <summary>
        /// Whether or not the seepage is constant.
        /// </summary>
        public bool SeepageIsConstant
        {
            get => Data.SeepageSource == UnpavedEnums.SeepageSourceType.Constant;
            set
            {
                if (value)
                {
                    Data.SeepageSource = UnpavedEnums.SeepageSourceType.Constant;
                }
            }
        }

        /// <summary>
        /// Whether or not the seepage is variable over time.
        /// </summary>
        public bool SeepageIsSeries
        {
            get => Data.SeepageSource == UnpavedEnums.SeepageSourceType.Series;
            set
            {
                if (value)
                {
                    Data.SeepageSource = UnpavedEnums.SeepageSourceType.Series;
                }
            }
        }

        /// <summary>
        /// Whether or not the seepage is calculated from a variable H0 value.
        /// </summary>
        public bool SeepageIsH0Series
        {
            get => Data.SeepageSource == UnpavedEnums.SeepageSourceType.H0Series;
            set
            {
                if (value)
                {
                    Data.SeepageSource = UnpavedEnums.SeepageSourceType.H0Series;
                }
            }
        }

        /// <summary>
        /// The unit for the area.
        /// </summary>
        public RainfallRunoffEnums.AreaUnit AreaUnit { get; set; }

        /// <summary>
        /// The unit for the storage.
        /// </summary>
        public RainfallRunoffEnums.StorageUnit StorageUnit { get; set; }

        /// <summary>
        /// The unit for the infiltration capacity.
        /// </summary>
        public RainfallRunoffEnums.RainfallCapacityUnit InfiltrationCapacityUnit { get; set; }

        /// <summary>
        /// The unit label for the area.
        /// </summary>
        public string AreaUnitLabel => AreaUnit.GetDescription();

        /// <summary>
        /// The total area.
        /// </summary>
        public double TotalAreaForGroundWaterCalculations
        {
            get => GetArea(Data.TotalAreaForGroundWaterCalculations);
            set => Data.TotalAreaForGroundWaterCalculations = GetConvertedArea(value);
        }

        /// <summary>
        /// The maximum land storage.
        /// </summary>
        public double MaximumLandStorage
        {
            get => GetStorage(Data.MaximumLandStorage);
            set => Data.MaximumLandStorage = GetConvertedStorage(value);
        }

        /// <summary>
        /// The initial land storage.
        /// </summary>
        public double InitialLandStorage
        {
            get => GetStorage(Data.InitialLandStorage);
            set => Data.InitialLandStorage = GetConvertedStorage(value);
        }

        /// <summary>
        /// The infiltration capacity.
        /// </summary>
        public double InfiltrationCapacity
        {
            get => GetCapacity(Data.InfiltrationCapacity);
            set => Data.InfiltrationCapacity = GetConvertedCapacity(value);
        }

        /// <summary>
        /// The area per crop type label.
        /// </summary>
        public string AreaPerCropTypeLabel => "Area per crop type in " + AreaUnitLabel;

        private bool LinkedToRunoffBoundary => Data.Catchment.Links.FirstOrDefault()?.Target is RunoffBoundary;

        private bool LinkedToFlowNode => Data.Catchment.Links.Any() && !LinkedToRunoffBoundary;

        private double GetArea(double value) =>
            RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.m2, AreaUnit, value);

        private double GetConvertedArea(double value) =>
            RainfallRunoffUnitConverter.ConvertArea(AreaUnit, RainfallRunoffEnums.AreaUnit.m2, value);

        private double GetStorage(double value) =>
            RainfallRunoffUnitConverter.ConvertStorage(UnpavedData.LandStorageUnit, StorageUnit, value, Data.TotalAreaForGroundWaterCalculations);

        private double GetConvertedStorage(double value) =>
            RainfallRunoffUnitConverter.ConvertStorage(StorageUnit, UnpavedData.LandStorageUnit, value, Data.TotalAreaForGroundWaterCalculations);

        private double GetCapacity(double value) =>
            RainfallRunoffUnitConverter.ConvertRainfall(UnpavedData.InfiltrationCapacityUnit, InfiltrationCapacityUnit, value);

        private double GetConvertedCapacity(double value) =>
            RainfallRunoffUnitConverter.ConvertRainfall(InfiltrationCapacityUnit, UnpavedData.InfiltrationCapacityUnit, value);
    }
}