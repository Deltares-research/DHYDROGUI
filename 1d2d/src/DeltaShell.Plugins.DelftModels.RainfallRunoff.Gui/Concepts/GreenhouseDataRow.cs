using System.ComponentModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    public class GreenhouseDataRow : RainfallRunoffDataRow<GreenhouseData>
    {
        [Description("Area Id")]
        public string AreaName
        {
            get { return data.Name; }
        }

        [Description("Total crop area (m²)")]
        public double CropArea
        {
            get { return data.CalculationArea; }
            set { data.CalculationArea = value; }
        }

        [Description("Surface level (m AD)")]
        public double SurfaceLevel
        {
            get { return data.SurfaceLevel; }
            set { data.SurfaceLevel = value; }
        }

        [Description("Area < 500 m³/ha (m²)")]
        public double lessThan500Area
        {
            get { return GetAreaForType(GreenhouseEnums.AreaPerGreenhouseType.lessThan500); }
            set { SetAreaForType(GreenhouseEnums.AreaPerGreenhouseType.lessThan500, value); }
        }

        [Description("500-1000 m³/ha")]
        public double from500to1000Area
        {
            get { return GetAreaForType(GreenhouseEnums.AreaPerGreenhouseType.from500to1000); }
            set { SetAreaForType(GreenhouseEnums.AreaPerGreenhouseType.from500to1000, value); }
        }

        [Description("1000-1500 m³/ha")]
        public double from1000to1500Area
        {
            get { return GetAreaForType(GreenhouseEnums.AreaPerGreenhouseType.from1000to1500); }
            set { SetAreaForType(GreenhouseEnums.AreaPerGreenhouseType.from1000to1500, value); }
        }

        [Description("1500-2000 m³/ha")]
        public double from15000to2000Area
        {
            get { return GetAreaForType(GreenhouseEnums.AreaPerGreenhouseType.from1500to2000); }
            set { SetAreaForType(GreenhouseEnums.AreaPerGreenhouseType.from1500to2000, value); }
        }

        [Description("2000-2500 m³/ha")]
        public double from2000to2500Area
        {
            get { return GetAreaForType(GreenhouseEnums.AreaPerGreenhouseType.from2000to2500); }
            set { SetAreaForType(GreenhouseEnums.AreaPerGreenhouseType.from2000to2500, value); }
        }

        [Description("2500-3000 m³/ha")]
        public double from2500to3000Area
        {
            get { return GetAreaForType(GreenhouseEnums.AreaPerGreenhouseType.from2500to3000); }
            set { SetAreaForType(GreenhouseEnums.AreaPerGreenhouseType.from2500to3000, value); }
        }

        [Description("3000-4000 m³/ha")]
        public double from3000to4000Area
        {
            get { return GetAreaForType(GreenhouseEnums.AreaPerGreenhouseType.from3000to4000); }
            set { SetAreaForType(GreenhouseEnums.AreaPerGreenhouseType.from3000to4000, value); }
        }

        [Description("4000-5000 m³/ha")]
        public double from4000to5000Area
        {
            get { return GetAreaForType(GreenhouseEnums.AreaPerGreenhouseType.from4000to5000); }
            set { SetAreaForType(GreenhouseEnums.AreaPerGreenhouseType.from4000to5000, value); }
        }

        [Description("5000-6000 m³/ha")]
        public double from5000to6000Area
        {
            get { return GetAreaForType(GreenhouseEnums.AreaPerGreenhouseType.from5000to6000); }
            set { SetAreaForType(GreenhouseEnums.AreaPerGreenhouseType.from5000to6000, value); }
        }

        [Description("> 6000 m³/ha")]
        public double moreThan6000Area
        {
            get { return GetAreaForType(GreenhouseEnums.AreaPerGreenhouseType.moreThan6000); }
            set { SetAreaForType(GreenhouseEnums.AreaPerGreenhouseType.moreThan6000, value); }
        }

        [Description("Use subsoil storage?")]
        public bool UseSubsoilStorage
        {
            get { return data.UseSubsoilStorage; }
            set { data.UseSubsoilStorage = value; }
        }

        [Description("Subsoil storage area (m²)")]
        public double? SubSoilStorageArea
        {
            get { return UseSubsoilStorage ? data.SubSoilStorageArea : (double?) null; }
            set
            {
                if (value != null && UseSubsoilStorage)
                {
                    data.SubSoilStorageArea = value.Value;
                }
            }
        }

        [Description("Silo capacity (m³/ha)")]
        public double? SiloCapacity
        {
            get { return UseSubsoilStorage ? data.SiloCapacity : (double?) null; }
            set
            {
                if (value != null && UseSubsoilStorage)
                {
                    data.SiloCapacity = value.Value;
                }
            }
        }

        [Description("Pump capacity (m³/s)")]
        public double? PumpCapacity
        {
            get { return UseSubsoilStorage ? data.PumpCapacity : (double?) null; }
            set
            {
                if (value != null && UseSubsoilStorage)
                {
                    data.PumpCapacity = value.Value;
                }
            }
        }

        [Description("Roof maximum storage (mm (x Area))")]
        public double MaximumRoofStorage
        {
            get { return data.MaximumRoofStorage; }
            set { data.MaximumRoofStorage = value; }
        }

        [Description("Roof initial storage (mm (x Area))")]
        public double InitialRoofStorage
        {
            get { return data.InitialRoofStorage; }
            set { data.InitialRoofStorage = value; }
        }

        [Description("Meteo station")]
        public string MeteoStationName
        {
            get { return data.MeteoStationName; }
            set { data.MeteoStationName = value; }
        }

        [Description("Area adjustment factor")]
        public double AreaAdjustmentFactor
        {
            get { return data.AreaAdjustmentFactor; }
            set { data.AreaAdjustmentFactor = value; }
        }

        private double GetAreaForType(GreenhouseEnums.AreaPerGreenhouseType areaType)
        {
            return data.AreaPerGreenhouse[areaType];
        }

        private void SetAreaForType(GreenhouseEnums.AreaPerGreenhouseType areaType, double value)
        {
            data.AreaPerGreenhouse[areaType] = value;
        }
    }
}