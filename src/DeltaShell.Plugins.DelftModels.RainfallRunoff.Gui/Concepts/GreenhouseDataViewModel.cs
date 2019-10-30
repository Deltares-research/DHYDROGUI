using System.ComponentModel;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    [Entity(FireOnCollectionChange=false)]
    public class GreenhouseDataViewModel
    {
        private RainfallRunoffEnums.AreaUnit areaUnit;

        public GreenhouseDataViewModel(GreenhouseData data, RainfallRunoffEnums.AreaUnit areaUnit)
        {
            Data = data;
            AreaUnit = areaUnit;
        }

        public GreenhouseData Data { get; set; } //public to bubble events to causes refreshes

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
            get { return Data.RoofStorageUnit; }
            set { Data.RoofStorageUnit = value; }
        }

        public string AreaUnitLabel { get; private set; }

        public double MaximumRoofStorage
        {
            get
            {
                return RainfallRunoffUnitConverter.ConvertStorage(RainfallRunoffEnums.StorageUnit.mm, StorageUnit,
                                                                  Data.MaximumRoofStorage, Data.CalculationArea);
            }
            set
            {
                Data.MaximumRoofStorage = RainfallRunoffUnitConverter.ConvertStorage(StorageUnit,
                                                                                     RainfallRunoffEnums.StorageUnit.mm,
                                                                                     value, Data.CalculationArea);
            }
        }

        public double InitialRoofStorage
        {
            get
            {
                return RainfallRunoffUnitConverter.ConvertStorage(RainfallRunoffEnums.StorageUnit.mm, StorageUnit,
                                                                  Data.InitialRoofStorage, Data.CalculationArea);
            }
            set
            {
                Data.InitialRoofStorage = RainfallRunoffUnitConverter.ConvertStorage(StorageUnit,
                                                                                     RainfallRunoffEnums.StorageUnit.mm,
                                                                                     value, Data.CalculationArea);
            }
        }

        public double SubSoilStorageArea
        {
            get
            {
                return RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.m2, AreaUnit,
                                                               Data.SubSoilStorageArea);
            }
            set
            {
                Data.SubSoilStorageArea = RainfallRunoffUnitConverter.ConvertArea(AreaUnit,
                                                                                  RainfallRunoffEnums.AreaUnit.m2, value);
            }
        }

        public string AreaPerGreenhouseTypeLabel
        {
            get
            {
                var converter = TypeDescriptor.GetConverter(typeof (RainfallRunoffEnums.AreaUnit));
                return "Area per greenhouse type in " + converter.ConvertToString(areaUnit);
            }
        }
    }
}