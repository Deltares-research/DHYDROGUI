using System.ComponentModel;
using DelftTools.Shell.Gui;
using DelftTools.Utils.ComponentModel;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts
{
    public class UnpavedDataProperties : ObjectProperties<UnpavedData>
    {
        [Category("Area")]
        [DisplayName("Runoff area [m²]")]
        public double Area
        {
            get { return data.CalculationArea; }
            set { data.CalculationArea = value; }
        }

        [Category("Ground water")]
        [DynamicReadOnly]
        [DisplayName("Area for groundwater calculations [m²]")]
        public double TotalAreaForGroundWaterCalculations
        {
            get { return data.TotalAreaForGroundWaterCalculations; }
            set { data.TotalAreaForGroundWaterCalculations = value; }
        }

        [Category("Ground water")]
        [DisplayName("Custom area for groundwater calculations [m²]")]
        public bool UseDifferentAreaForGroundWaterCalculations
        {
            get { return data.UseDifferentAreaForGroundWaterCalculations; }
            set { data.UseDifferentAreaForGroundWaterCalculations = value; }
        }

        [Category("Surface")]
        [DisplayName("Surface level [m AD]")]
        public double SurfaceLevel
        {
            get { return data.SurfaceLevel; }
            set { data.SurfaceLevel = value; }
        }

        [Category("Soil")]
        [DisplayName("Soil type")]
        public UnpavedEnums.SoilType SoilType
        {
            get { return data.SoilType; }
            set { data.SoilType = value; }
        }

        [Category("Soil")]
        [DisplayName("CapSim soil type")]
        public UnpavedEnums.SoilTypeCapsim SoilTypeCapsim
        {
            get { return data.SoilTypeCapsim; }
            set { data.SoilTypeCapsim = value; }
        }

        [Category("Ground water")]
        [DisplayName("Groundwater layer thickness [m]")]
        public double GroundWaterLayerThickness
        {
            get { return data.GroundWaterLayerThickness; }
            set { data.GroundWaterLayerThickness = value; }
        }

        [Category("Ground water")]
        [DisplayName("Maximum allowed groundwater level [m AD]")]
        public double MaximumAllowedGroundWaterLevel
        {
            get { return data.MaximumAllowedGroundWaterLevel; }
            set { data.MaximumAllowedGroundWaterLevel = value; }
        }

        [Category("Ground water")]
        [DisplayName("Initial groundwater level source")]
        public UnpavedEnums.GroundWaterSourceType InitialGroundWaterLevelSource
        {
            get { return data.InitialGroundWaterLevelSource; }
            set { data.InitialGroundWaterLevelSource = value; }
        }

        [Category("Ground water")]
        [DynamicReadOnly]
        [DisplayName("Initial groundwater level [m AD]")]
        public double InitialGroundWaterLevelConstant
        {
            get { return data.InitialGroundWaterLevelConstant; }
            set { data.InitialGroundWaterLevelConstant = value; }
        }

        [Category("Storage")]
        [DisplayName("Maximum land storage [mm]")]
        public double MaximumLandStorage
        {
            get { return data.MaximumLandStorage; }
            set { data.MaximumLandStorage = value; }
        }

        [Category("Storage")]
        [DisplayName("Initial land storage [mm]")]
        public double InitialLandStorage
        {
            get { return data.InitialLandStorage; }
            set { data.InitialLandStorage = value; }
        }
        
        [Category("Infiltration")]
        [DisplayName("Infiltration capacity [mm/h]")]
        public double InfiltrationCapacity
        {
            get { return data.InfiltrationCapacity; }
            set { data.InfiltrationCapacity = value; }
        }

        [Category("Drainage")]
        [DisplayName("Drainage formula")]
        public string DrainageFormula
        {
            get { return data.DrainageFormula.ToString(); }
        }

        [Category("Seepage")]
        [DisplayName("Seepage source")]
        public UnpavedEnums.SeepageSourceType SeepageSource
        {
            get { return data.SeepageSource; }
            set { data.SeepageSource = value; }
        }

        [Category("Seepage")]
        [DisplayName("Seepage [mm/day]")]
        [DynamicReadOnly]
        public double SeepageConstant
        {
            get { return data.SeepageConstant; }
            set { data.SeepageConstant = value; }
        }

        [Category("Seepage")]
        [DisplayName("Seepage Hydraulic Resistance")]
        [DynamicReadOnly]
        public double SeepageH0HydraulicResistance
        {
            get { return data.SeepageH0HydraulicResistance; }
            set { data.SeepageH0HydraulicResistance = value; }
        }

        [Category("Meteo")]
        [Description("Meteo station")]
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
        
        [DynamicReadOnlyValidationMethod]
        public bool IsReadOnly(string propertyName)
        {
            if (propertyName == nameof(SeepageH0HydraulicResistance))
            {
                return SeepageSource != UnpavedEnums.SeepageSourceType.H0Series;
            }

            if (propertyName == nameof(SeepageConstant))
            {
                return SeepageSource != UnpavedEnums.SeepageSourceType.Constant;
            }

            if (propertyName == nameof(InitialGroundWaterLevelConstant))
            {
                return InitialGroundWaterLevelSource != UnpavedEnums.GroundWaterSourceType.Constant;
            }

            if (propertyName == nameof(TotalAreaForGroundWaterCalculations))
            {
                return !UseDifferentAreaForGroundWaterCalculations;
            }

            return false;
        }
    }
}