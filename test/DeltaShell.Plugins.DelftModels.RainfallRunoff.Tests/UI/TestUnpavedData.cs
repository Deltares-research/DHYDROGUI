using System;
using System.ComponentModel;
using System.Windows.Forms;
using DelftTools.Controls.Swf.DataEditorGenerator.FromType;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.Unpaved;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI
{
    [Entity(FireOnCollectionChange = false)]
    public class TestUnpavedData
    {
        private TestUnpavedEnums.GroundWaterSourceType initialGroundWaterLevelSource;
        private TestUnpavedEnums.SeepageSourceType seepageSource;
        private double totalAreaForGroundWaterCalculations;
        private bool useDifferentAreaForGroundWaterCalculations;

        public TestUnpavedData()
        {
            CropType = TestUnpavedEnums.CropType.BulbousPlants;
            CalculationArea = 3243;
            SurfaceLevel = 1.5;
            SoilType = TestUnpavedEnums.SoilType.sand_maximum;
            SoilTypeCapsim = TestUnpavedEnums.SoilTypeCapsim.soiltype_capsim_1;
            GroundWaterLayerThickness = 5;
            MaximumAllowedGroundWaterLevel = 1.5;
            InfiltrationCapacity = 5;
        }

        [Category("Area")]
        public double CalculationArea { get; set; }

        [Category("Area")]
        [Description("Crop type")]
        public TestUnpavedEnums.CropType CropType { get; set; }

        [Category("Area")]
        [SubCategory("Groundwater")]
        public bool UseDifferentAreaForGroundWaterCalculations
        {
            get { return useDifferentAreaForGroundWaterCalculations; }
            set
            {
                if (value
                    && !useDifferentAreaForGroundWaterCalculations
                    && totalAreaForGroundWaterCalculations == 0.0)
                {
                    //if turning this on for the first time, set correct initial value
                    totalAreaForGroundWaterCalculations = CalculationArea;
                }
                useDifferentAreaForGroundWaterCalculations = value;
            }
        }

        [Description("Area for groundwater calculations")]
        [Category("Area")]
        [SubCategory("Groundwater")]
        [EnabledIf("UseDifferentAreaForGroundWaterCalculations", true)]
        public double TotalAreaForGroundWaterCalculations // m2
        {
            get
            {
                if (UseDifferentAreaForGroundWaterCalculations)
                {
                    return totalAreaForGroundWaterCalculations;
                }
                return CalculationArea;
            }
            set { totalAreaForGroundWaterCalculations = value; }
        }
        

        [Category("Surface & Soil")]
        [Description("Surface level")]
        public double SurfaceLevel { get; set; }

        //m AD
        [Category("Surface & Soil")]
        [Description("Soil type")]
        public TestUnpavedEnums.SoilType SoilType { get; set; } //int = SoilType, mu = 'per m' == m/m?

        /// <summary>
        /// Soiltype for capsim calculation
        /// </summary>
        [Description("Soil type for capsim calculation")]
        [Category("Surface & Soil")]
        public TestUnpavedEnums.SoilTypeCapsim SoilTypeCapsim { get; set; }

        [Description("Groundwater layer thickness")]
        [Category("Groundwater")]
        public double GroundWaterLayerThickness { get; set; }

        //m

        [Description("Maximum allowed groundwater level")]
        [Category("Groundwater")]
        public double MaximumAllowedGroundWaterLevel { get; set; }

        //m AD
        [Category("Groundwater")]
        [SubCategory("Initial groundwater level")]
        [Description("Source")]
        public TestUnpavedEnums.GroundWaterSourceType InitialGroundWaterLevelSource
        {
            get { return initialGroundWaterLevelSource; }
            set
            {
                initialGroundWaterLevelSource = value;
                CreateInitialGroundwaterLevelFunctionIfNeeded();
            }
        }
        [Category("Groundwater")]
        [SubCategory("Initial groundwater level")]
        [Description("Constant")]
        [EnabledIf("InitialGroundWaterLevelSource", TestUnpavedEnums.GroundWaterSourceType.Constant)]
        public double InitialGroundWaterLevelConstant { get; set; }
        
        //[note: is a timeseries to pick the inital ground water level from (so only single value is used), remove?]
        [Category("Groundwater")]
        [SubCategory("Initial groundwater level")]
        [Description("Pick from table")]
        [EnabledIf("InitialGroundWaterLevelSource", TestUnpavedEnums.GroundWaterSourceType.Series)]
        public TimeSeries InitialGroundWaterLevelSeries { get; set; } //m below surface

        [Category("Drainage formula")]
        [CustomControlHelper(
            "DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.ErnstZeeuwHellingaDrainageControlHelper",
            "DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests")]
        public ErnstDeZeeuwHellingaDrainageFormulaBase DrainageFormula { get; set; }

        //De Zeeuw-Hellinga, Ernst
        
        [Category("Storage & Infiltration")]
        [Description("Maximum land storage")]
        public double MaximumLandStorage { get; set; }

        //mm (x Area)
        [Category("Storage & Infiltration")]
        [Description("Initial land storage")]
        public double InitialLandStorage { get; set; }

        //mm (x Area)
        [Hide]
        public TestUnpavedEnums.StorageUnit LandStorageUnit { get; set; }

        [Category("Storage & Infiltration")]
        [Description("Infiltration capacity")] //mm per hr
        public double InfiltrationCapacity { get; set; }

        [Hide]
        public TestUnpavedEnums.RainfallCapacityUnit InfiltrationCapacityUnit { get; set; }

        [Category("Seepage")]
        [SubCategory("Seepage")]
        [Description("Source")]
        public TestUnpavedEnums.SeepageSourceType SeepageSource
        {
            get { return seepageSource; }
            set
            {
                seepageSource = value;
                CreateSeepageFunctionsIfNeeded();
            }
        }

        //how is the seepage defined?
        [Category("Seepage")]
        [SubCategory("Seepage")]
        [EnabledIf("SeepageSource", TestUnpavedEnums.SeepageSourceType.Constant)]
        [Description("Constant")]
        public double SeepageConstant { get; set; }

        //mm/day
        //[note: is a timeseries to pick the initial seepage from (so only single value is used)]
        [Category("Seepage")]
        [SubCategory("Seepage")]
        [EnabledIf("SeepageSource", TestUnpavedEnums.SeepageSourceType.Series)]
        [Description("Series")]
        public TimeSeries SeepageSeries { get; set; } //mm/day
        
        [Category("Seepage")]
        [SubCategory("Seepage")]
        [EnabledIf("SeepageSource", TestUnpavedEnums.SeepageSourceType.H0Series)]
        [Description("Hydraulic Resistance")]
        public double SeepageH0HydraulicResistance { get; set; } //day (C)
        
        [Category("Seepage")]
        [SubCategory("Seepage")]
        [EnabledIf("SeepageSource", TestUnpavedEnums.SeepageSourceType.H0Series)]
        [Description("H0 Series")]
        public TimeSeries SeepageH0Series { get; set; } //m AD (piezometric level H0)
        
        private void CreateInitialGroundwaterLevelFunctionIfNeeded()
        {
            switch (InitialGroundWaterLevelSource)
            {
                case TestUnpavedEnums.GroundWaterSourceType.Constant:
                    break;
                case TestUnpavedEnums.GroundWaterSourceType.Series:
                    if (InitialGroundWaterLevelSeries == null)
                    {
                        InitialGroundWaterLevelSeries = new TimeSeries { Name = "Initial Groundwater Level" };
                        InitialGroundWaterLevelSeries.Components.Add(new Variable<double>
                        {
                            Name = "Initial Groundwater Level",
                            Unit = new Unit("m below surface", "m bel. surf.")
                        });
                    }
                    break;
                case TestUnpavedEnums.GroundWaterSourceType.FromLinkedNode:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CreateSeepageFunctionsIfNeeded()
        {
            switch (SeepageSource)
            {
                case TestUnpavedEnums.SeepageSourceType.Constant:
                    break; //perhaps delete series here?
                case TestUnpavedEnums.SeepageSourceType.Series:
                    if (SeepageSeries == null)
                    {
                        SeepageSeries = new TimeSeries { Name = "Seepage" };
                        SeepageSeries.Components.Add(new Variable<double> { Name = "Seepage", Unit = new Unit("mm/day", "mm/day") });
                    }
                    break;
                case TestUnpavedEnums.SeepageSourceType.H0Series:
                    if (SeepageH0Series == null)
                    {
                        SeepageH0Series = new TimeSeries { Name = "Piezometric level H0" };
                        SeepageH0Series.Components.Add(new Variable<double>
                        {
                            Name = "Piezometric level H0",
                            Unit = new Unit("m AD", "m AD")
                        });
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }

    public class ErnstZeeuwHellingaDrainageControlHelper : ICustomControlHelper
    {
        public Control CreateControl()
        {
            return new ErnstZeeuwHellingaDrainageControl();
        }

        public void SetData(Control control, object rootObject, object propertyValue)
        {
            ((ErnstZeeuwHellingaDrainageControl)control).Data = (ErnstDeZeeuwHellingaDrainageFormulaBase)propertyValue;
        }

        public bool HideCaptionAndUnitLabel()
        {
            return false;
        }
    }

    public static class TestUnpavedEnums
    {
        [TypeConverter(typeof (EnumDescriptionAttributeTypeConverter))]
        public enum CropType
        {
            Grass,
            Corn,
            Potatoes,
            Sugarbeet,
            Grain,
            Miscellaneous,
            [Description("Non-arable land")] NonArableLand,
            [Description("Greenhouse Area")] GreenhouseArea,
            Orchard,
            [Description("Bulbous Plants")] BulbousPlants,
            [Description("Foliage Forest")] FoliageForest,
            [Description("Pine Forest")] PineForest,
            Nature,
            Fallow,
            Vegetables,
            Flowers
        };

        [TypeConverter(typeof (EnumDescriptionAttributeTypeConverter))]
        public enum DrainageComputationOption
        {
            [Description("De Zeeuw / Hellinga")] DeZeeuwHellinga,
            [Description("Krayenhoff / Van de Leur")] KrayenhoffVdLeur,
            Ernst
        }

        [TypeConverter(typeof (EnumDescriptionAttributeTypeConverter))]
        public enum GroundWaterSourceType
        {
            Constant,
            Series,
            [Description("From linked node")] FromLinkedNode
        }

        public enum SeepageSourceType
        {
            Constant,
            Series,
            H0Series,
            //future: MODFLOW
        }

        [TypeConverter(typeof (EnumDescriptionAttributeTypeConverter))]
        public enum SoilType
        {
            [Description("sand (maximum) [μ = 0.117 per m]")] sand_maximum = 1,
            [Description("peat (maximum) [μ = 0.078 per m]")] peat_maximum = 2,
            [Description("clay (maximum) [μ = 0.049 per m]")] clay_maximum = 3,
            [Description("peat (average) [μ = 0.067 per m]")] peat_average = 4,
            [Description("sand (average) [μ = 0.088 per m]")] sand_average = 5,
            [Description("silt (maximum) [μ = 0.051 per m]")] silt_maximum = 6,
            [Description("peat (minimum) [μ = 0.051 per m]")] peat_minimum = 7,
            [Description("clay (average) [μ = 0.036 per m]")] clay_average = 8,
            [Description("sand (minimum) [μ = 0.060 per m]")] sand_minimum = 9,
            [Description("silt (average) [μ = 0.038 per m]")] silt_average = 10,
            [Description("clay (minimum) [μ = 0.026 per m]")] clay_minimum = 11,
            [Description("silt (minimum) [μ = 0.021 per m]")] silt_minimum = 12,
        }

        [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
        public enum SoilTypeCapsim
        {
           [Description("Veengrond met veraarde bovengrond")] soiltype_capsim_1 = 101,
           [Description("Veengrond met veraarde bovengrond, zand")] soiltype_capsim_2 = 102,
           [Description("Veengrond met kleidek")] soiltype_capsim_3 = 103,
           [Description("Veengrond met kleidek op zand")] soiltype_capsim_4 = 104,
           [Description("Veengrond met zanddek op zand")] soiltype_capsim_5 = 105,
           [Description("Veengrond op ongerijpte klei")] soiltype_capsim_6 = 106,
           [Description("Stuifzand")] soiltype_capsim_7 = 107,
           [Description("Podzol (Leemarm, fijn zand)")] soiltype_capsim_8 = 108,
           [Description("Podzol (zwak lemig, fijn zand)")] soiltype_capsim_9 = 109,
           [Description("Podzol (zwak lemig, fijn zand op grof zand)")] soiltype_capsim_10 = 110,
           [Description("Podzol (lemig keileem)")] soiltype_capsim_11 = 111,
           [Description("Enkeerd (zwak lemig, fijn zand)")] soiltype_capsim_12 = 112,
           [Description("Beekeerd (lemig fijn zand)")] soiltype_capsim_13 = 113,
           [Description("Podzol (grof zand)")] soiltype_capsim_14 = 114,
           [Description("Zavel")] soiltype_capsim_15 = 115,
           [Description("Lichte klei")] soiltype_capsim_16 = 116,
           [Description("Zware klei")] soiltype_capsim_17 = 117,
           [Description("Klei op veen")] soiltype_capsim_18 = 118,
           [Description("Klei op zand")] soiltype_capsim_19 = 119,
           [Description("Klei op grof zand")] soiltype_capsim_20 = 120,
           [Description("Leem")] soiltype_capsim_21 = 121
        }

        [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
        public enum RainfallCapacityUnit
        {
            [Description("mm/hr")]
            mm_hr,
            [Description("mm/day")]
            mm_day
        }
        
        [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
        public enum StorageUnit
        {
            [Description("mm (x Area)")]
            mm,
            [Description("m³")]
            m3,
        }
    }
}