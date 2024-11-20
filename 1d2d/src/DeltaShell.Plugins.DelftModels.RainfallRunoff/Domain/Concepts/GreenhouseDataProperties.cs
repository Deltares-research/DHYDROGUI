using System.ComponentModel;
using DelftTools.Shell.Gui;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts
{
    public class GreenhouseDataProperties : ObjectProperties<GreenhouseData>
    {
        [Category("Area")]
        [DisplayName("Runoff area [m²]")]
        public double Area
        {
            get { return data.CalculationArea; }
            set { data.CalculationArea = value; }
        }

        [Category("Area")]
        [DisplayName("Surface level [m AD]")]
        public double SurfaceLevel
        {
            get { return data.SurfaceLevel; }
            set { data.SurfaceLevel = value; }
        }

        [Category("Storage")]
        [DisplayName("Roof maximum [mm]")]
        public double RoofMaximum
        {
            get { return data.MaximumRoofStorage; }
            set { data.MaximumRoofStorage = value; }
        }

        [Category("Storage")]
        [DisplayName("Roof initial [mm]")]
        public double RoofInitial
        {
            get { return data.InitialRoofStorage; }
            set { data.InitialRoofStorage = value; }
        }

        [Category("Storage")]
        [DisplayName("Use subsoil storage")]
        public bool UseSubSoilStorage
        {
            get { return data.UseSubsoilStorage; }
            set { data.UseSubsoilStorage = value; }
        }

        [Category("Storage")]
        [DisplayName("Subsoil storage area [m²]")]
        public double SubSoilStorageArea
        {
            get { return data.SubSoilStorageArea; }
            set { data.SubSoilStorageArea = value; }
        }

        [Category("Capacity")]
        [DisplayName("Silo capacity [m³/ha]")]
        public double SiloCapacity
        {
            get { return data.SiloCapacity; }
            set { data.SiloCapacity = value; }
        }

        [Category("Capacity")]
        [DisplayName("Pump capacity [m³/s]")]
        public double PumpCapacity
        {
            get { return data.PumpCapacity; }
            set { data.PumpCapacity = value; }
        }

        [Category("Meteo")]
        [DisplayName("Meteo station")]
        public string MeteoStationName
        {
            get { return data.MeteoStationName; }
            set { data.MeteoStationName = value; }
        }

        [Category("Meteo")]
        [DisplayName("Area adjustment factor")]
        public double AreaAdjustmentFactor
        {
            get { return data.AreaAdjustmentFactor; }
            set { data.AreaAdjustmentFactor = value; }
        }
    }
}