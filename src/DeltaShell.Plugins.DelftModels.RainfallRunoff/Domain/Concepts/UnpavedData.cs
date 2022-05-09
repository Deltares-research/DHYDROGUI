using System;
using System.ComponentModel;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Units;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts
{
    [Entity(FireOnCollectionChange=false)]
    public class UnpavedData : CatchmentModelData
    {
        /// <summary>
        /// The land storage unit.
        /// </summary>
        public const RainfallRunoffEnums.StorageUnit LandStorageUnit = RainfallRunoffEnums.StorageUnit.mm;
        
        /// <summary>
        /// The infiltration capacity unit.
        /// </summary>
        public const RainfallRunoffEnums.RainfallCapacityUnit InfiltrationCapacityUnit = RainfallRunoffEnums.RainfallCapacityUnit.mm_hr;
        
        //nhib
        protected UnpavedData():base(null)
        {
        }

        public RainfallRunoffBoundaryData BoundaryData { get; set; }

        private UnpavedEnums.GroundWaterSourceType initialGroundWaterLevelSource;
        private UnpavedEnums.SeepageSourceType seepageSource;
        private double totalAreaForGroundWaterCalculations;
        private bool useDifferentAreaForGroundWaterCalculations;
        private CropAreaDictionary areaPerCrop;

        public UnpavedData(Catchment catchment) : base(catchment)
        {
            SurfaceLevel = 1.5;
            SoilType = UnpavedEnums.SoilType.sand_maximum;
            SoilTypeCapsim = UnpavedEnums.SoilTypeCapsim.soiltype_capsim_1;
            GroundWaterLayerThickness = 5;
            MaximumAllowedGroundWaterLevel = 1.5;
            InfiltrationCapacity = 5;
            BoundaryData = new RainfallRunoffBoundaryData();
            DrainageFormula = new DeZeeuwHellingaDrainageFormula
            {
                SurfaceRunoff = 100,
                HorizontalInflow = 0.05,
                InfiniteDrainageLevelRunoff = 0.3
            };
        }

        public CropAreaDictionary AreaPerCrop
        {
            get
            {
                if (areaPerCrop == null)
                {
                    areaPerCrop = CreateInitializedAreaCropsDictionary();
                }
                return areaPerCrop;
            }
        } // m2

        [Description("Area for groundwater calculations")]
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

        public override double CalculationArea
        {
            get { return AreaPerCrop?.Sum ?? 0.0; }
            set
            {
                if (Math.Abs(value - base.CalculationArea) < 1e-10)
                    return;

                AreaPerCrop.Reset(UnpavedEnums.CropType.Grass, value);

                base.CalculationArea = value;
            }
        }

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

        [Description("Surface level")]
        public double SurfaceLevel { get; set; }

        //m AD
        [Description("Soil type")]
        public UnpavedEnums.SoilType SoilType { get; set; } //int = SoilType, mu = 'per m' == m/m?

        /// <summary>
        /// Soiltype for capsim calculation
        /// </summary>
        [Description("Soil type for capsim calculation")]
        public UnpavedEnums.SoilTypeCapsim SoilTypeCapsim { get; set; } 

        [Description("Groundwater layer thickness")]
        public double GroundWaterLayerThickness { get; set; }

        //m

        [Description("Maximum allowed groundwater level")]
        public double MaximumAllowedGroundWaterLevel { get; set; }

        //m AD

        public UnpavedEnums.GroundWaterSourceType InitialGroundWaterLevelSource
        {
            get { return initialGroundWaterLevelSource; }
            set
            {
                initialGroundWaterLevelSource = value;
                CreateInitialGroundwaterLevelFunctionIfNeeded();
            }
        }

        [Description("Initial groundwater level")]
        public double InitialGroundWaterLevelConstant { get; set; }

        //m below surface

        //[note: is a timeseries to pick the inital ground water level from (so only single value is used), remove?]
        public TimeSeries InitialGroundWaterLevelSeries { get; set; } //m below surface

        /// <summary>
        /// The maximum land storage (mm) of the area (m²).
        /// </summary>
        [Description("Maximum land storage")]
        public double MaximumLandStorage { get; set; }
        
        /// <summary>
        /// The initial land storage (mm) of the area (m²).
        /// </summary>
        [Description("Initial land storage")]
        public double InitialLandStorage { get; set; }
        
        /// <summary>
        /// The infiltration capacity (mm/hr) of the area (m²)
        /// </summary>
        [Description("Infiltration capacity")]
        public double InfiltrationCapacity { get; set; }
        
        public IDrainageFormula DrainageFormula { get; set; } //De Zeeuw-Hellinga, Ernst, Krayenhoff van de Leur

        public UnpavedEnums.SeepageSourceType SeepageSource
        {
            get { return seepageSource; }
            set
            {
                seepageSource = value;
                CreateSeepageFunctionsIfNeeded();
            }
        }

        //how is the seepage defined?

        [Description("Seepage")]
        public double SeepageConstant { get; set; }

        //mm/day
        //[note: is a timeseries to pick the initial seepage from (so only single value is used)]
        public TimeSeries SeepageSeries { get; set; } //mm/day
        public double SeepageH0HydraulicResistance { get; set; } //day (C)
        public TimeSeries SeepageH0Series { get; set; } //m AD (piezometric level H0)

        //future: use from MODFLOW 

        #region ICloneable Members

        public override object Clone()
        {
            var clone = (UnpavedData)base.Clone();
            clone.areaPerCrop = (CropAreaDictionary) AreaPerCrop.Clone();
            clone.DrainageFormula = (IDrainageFormula) DrainageFormula.Clone();
            clone.SeepageSeries = SeepageSeries != null ? (TimeSeries) SeepageSeries.Clone() : null;
            clone.InitialGroundWaterLevelSeries = InitialGroundWaterLevelSeries != null
                                                      ? (TimeSeries) InitialGroundWaterLevelSeries.Clone()
                                                      : null;
            clone.BoundaryData = BoundaryData != null ? (RainfallRunoffBoundaryData) BoundaryData.Clone() : null;
            return clone;
        }

        #endregion
        
        public void SwitchDrainageFormula<T>() where T : IDrainageFormula, new()
        {
            if (DrainageFormula is T)
            {
                return; //nothing to change
            }

            DrainageFormula = new T();
        }

        [EditAction]
        private void CreateInitialGroundwaterLevelFunctionIfNeeded()
        {
            switch (InitialGroundWaterLevelSource)
            {
                case UnpavedEnums.GroundWaterSourceType.Constant:
                    break;
                case UnpavedEnums.GroundWaterSourceType.Series:
                    if (InitialGroundWaterLevelSeries == null)
                    {
                        InitialGroundWaterLevelSeries = new TimeSeries {Name = "Initial Groundwater Level"};
                        InitialGroundWaterLevelSeries.Components.Add(new Variable<double>
                            {
                                Name = "Initial Groundwater Level",
                                Unit = new Unit("m below surface", "m bel. surf.")
                            });
                    }
                    break;
                case UnpavedEnums.GroundWaterSourceType.FromLinkedNode:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [EditAction]
        private void CreateSeepageFunctionsIfNeeded()
        {
            switch (SeepageSource)
            {
                case UnpavedEnums.SeepageSourceType.Constant:
                    break; //perhaps delete series here?
                case UnpavedEnums.SeepageSourceType.Series:
                    if (SeepageSeries == null)
                    {
                        SeepageSeries = new TimeSeries {Name = "Seepage"};
                        SeepageSeries.Components.Add(new Variable<double>
                            {Name = "Seepage", Unit = new Unit("mm/day", "mm/day")});
                    }
                    break;
                case UnpavedEnums.SeepageSourceType.H0Series:
                    if (SeepageH0Series == null)
                    {
                        SeepageH0Series = new TimeSeries {Name = "Piezometric level H0"};
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

        private CropAreaDictionary CreateInitializedAreaCropsDictionary()
        {
            var areaPerCropDictionary = new CropAreaDictionary();
            foreach (UnpavedEnums.CropType cropType in Enum.GetValues(typeof(UnpavedEnums.CropType)))
            {
                areaPerCropDictionary.Add(cropType, 0.0);
            }

            areaPerCropDictionary.SumChanged += (s, e) =>
            {
                // Synchronizes the sum of the area's with the calculation area.
                // This also sets the correct geometry
                base.CalculationArea = e.Sum;
            };
            return areaPerCropDictionary;
        }
    }
}