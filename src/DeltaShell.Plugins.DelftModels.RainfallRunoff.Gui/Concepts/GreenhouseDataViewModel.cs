using DelftTools.Utils.Aop;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    /// <summary>
    /// The view model for the <see cref="GreenHouseDataView"/>.
    /// </summary>
    [Entity(FireOnCollectionChange = false)]
    public class GreenhouseDataViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GreenhouseDataViewModel"/> class.
        /// </summary>
        /// <param name="data">The green house data. </param>
        /// <param name="areaUnit"> The area unit. </param>
        public GreenhouseDataViewModel(GreenhouseData data, RainfallRunoffEnums.AreaUnit areaUnit)
        {
            Data = data;
            AreaUnit = areaUnit;
        }

        /// <summary>
        /// The green house data.
        /// </summary>
        public GreenhouseData Data { get; set; } //public to bubble events to causes refreshes

        /// <summary>
        /// The unit for the area.
        /// </summary>
        public RainfallRunoffEnums.AreaUnit AreaUnit { get; set; }

        /// <summary>
        /// The unit for the storage.
        /// </summary>
        public RainfallRunoffEnums.StorageUnit StorageUnit { get; set; }

        /// <summary>
        /// The unit label for the area.
        /// </summary>
        public string AreaUnitLabel => AreaUnit.GetDescription();

        /// <summary>
        /// The maximum roof storage.
        /// </summary>
        public double MaximumRoofStorage
        {
            get => GetStorage(Data.MaximumRoofStorage);
            set => Data.MaximumRoofStorage = GetConvertedStorage(value);
        }

        /// <summary>
        /// The initial roof storage.
        /// </summary>
        public double InitialRoofStorage
        {
            get => GetStorage(Data.InitialRoofStorage);
            set => Data.InitialRoofStorage = GetConvertedStorage(value);
        }

        /// <summary>
        /// The sub soil storage area.
        /// </summary>
        public double SubSoilStorageArea
        {
            get => GetStorageArea(Data.SubSoilStorageArea);
            set => Data.SubSoilStorageArea = GetConvertedStorageArea(value);
        }

        /// <summary>
        /// The area per green house type label.
        /// </summary>
        public string AreaPerGreenhouseTypeLabel => "Area per greenhouse type in " + AreaUnitLabel;

        private double GetStorage(double value) =>
            RainfallRunoffUnitConverter.ConvertStorage(GreenhouseData.StorageUnit, StorageUnit, value, Data.CalculationArea, AreaUnit);

        private double GetConvertedStorage(double value) =>
            RainfallRunoffUnitConverter.ConvertStorage(StorageUnit, GreenhouseData.StorageUnit, value, Data.CalculationArea, AreaUnit);

        private double GetStorageArea(double value) =>
            RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.m2, AreaUnit, value);

        private double GetConvertedStorageArea(double value) =>
            RainfallRunoffUnitConverter.ConvertArea(AreaUnit, RainfallRunoffEnums.AreaUnit.m2, value);
    }
}