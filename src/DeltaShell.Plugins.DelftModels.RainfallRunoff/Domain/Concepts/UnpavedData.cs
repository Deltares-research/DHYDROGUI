using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Units;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts
{
    /// <summary>
    /// Represents data for unpaved catchments.
    /// </summary>
    [Entity(FireOnCollectionChange = false)]
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

        private UnpavedEnums.GroundWaterSourceType initialGroundWaterLevelSource;
        private UnpavedEnums.SeepageSourceType seepageSource;
        private double totalAreaForGroundWaterCalculations;
        private bool useDifferentAreaForGroundWaterCalculations;
        private CropAreaDictionary areaPerCrop;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnpavedData"/> class with the specified catchment.
        /// </summary>
        /// <param name="catchment">The catchment associated with the data.</param>
        public UnpavedData(Catchment catchment) : base(catchment)
        {
            catchment.ModelData = this;
            SurfaceLevel = 1.5;
            SoilType = UnpavedEnums.SoilType.sand_maximum;
            SoilTypeCapsim = UnpavedEnums.SoilTypeCapsim.soiltype_capsim_1;
            GroundWaterLayerThickness = 5;
            MaximumAllowedGroundWaterLevel = 1.5;
            InfiltrationCapacity = 5;
            BoundarySettings = new RainfallRunoffBoundarySettings(new RainfallRunoffBoundaryData(), false);
            DrainageFormula = new DeZeeuwHellingaDrainageFormula
            {
                SurfaceRunoff = 100,
                HorizontalInflow = 0.05,
                InfiniteDrainageLevelRunoff = 0.3
            };
        }

        // Required for NHibernate
        protected UnpavedData() : base(null) {}

        /// <summary>
        /// Gets or sets the calculation area for the unpaved catchment data.
        /// </summary>
        /// <remarks>
        /// When setting the calculation area, the <see cref="AreaPerCrop"/> is updated accordingly, and the base class's calculation area is set.
        /// </remarks>
        public override double CalculationArea
        {
            get
            {
                return AreaPerCrop?.Sum ?? 0.0;
            }
            set
            {
                if (Math.Abs(value - base.CalculationArea) < 1e-10)
                {
                    return;
                }

                AreaPerCrop.Reset(UnpavedEnums.CropType.Grass, value);

                base.CalculationArea = value;
            }
        }

        /// <summary>
        /// Gets or sets the boundary settings for the unpaved data.
        /// </summary>
        public RainfallRunoffBoundarySettings BoundarySettings { get; private set; }

        /// <summary>
        /// Gets the area per crop dictionary in m².
        /// </summary>
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
        }

        /// <summary>
        /// Gets or sets the total area for groundwater calculations in m².
        /// </summary>
        [Description("Area for groundwater calculations")]
        public double TotalAreaForGroundWaterCalculations
        {
            get
            {
                if (UseDifferentAreaForGroundWaterCalculations)
                {
                    return totalAreaForGroundWaterCalculations;
                }

                return CalculationArea;
            }
            set
            {
                totalAreaForGroundWaterCalculations = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use a different area for groundwater calculations.
        /// </summary>
        public bool UseDifferentAreaForGroundWaterCalculations
        {
            get
            {
                return useDifferentAreaForGroundWaterCalculations;
            }
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

        /// <summary>
        /// Gets or sets the surface level of the unpaved area in m AD.
        /// </summary>
        [Description("Surface level")]
        public double SurfaceLevel { get; set; }
        
        /// <summary>
        /// Gets or sets the soil type of the unpaved area.
        /// </summary>
        [Description("Soil type")]
        public UnpavedEnums.SoilType SoilType { get; set; }

        /// <summary>
        /// Gets or sets the soil type for capsim calculations.
        /// </summary>
        [Description("Soil type for capsim calculation")]
        public UnpavedEnums.SoilTypeCapsim SoilTypeCapsim { get; set; }

        /// <summary>
        /// Gets or sets the thickness of the groundwater layer in m.
        /// </summary>
        [Description("Groundwater layer thickness")]
        public double GroundWaterLayerThickness { get; set; }
        

        /// <summary>
        /// Gets or sets the maximum allowed groundwater level in m AD.
        /// </summary>
        [Description("Maximum allowed groundwater level")]
        public double MaximumAllowedGroundWaterLevel { get; set; }


        /// <summary>
        /// Gets or sets the source of the initial groundwater level.
        /// </summary>
        public UnpavedEnums.GroundWaterSourceType InitialGroundWaterLevelSource
        {
            get
            {
                return initialGroundWaterLevelSource;
            }
            set
            {
                initialGroundWaterLevelSource = value;
                CreateInitialGroundwaterLevelFunctionIfNeeded();
            }
        }

        /// <summary>
        /// Gets or sets the constant value of the initial groundwater level in m below surface.
        /// </summary>
        [Description("Initial groundwater level")]
        public double InitialGroundWaterLevelConstant { get; set; }
        

        /// <summary>
        /// Gets or sets the time series for the initial groundwater level in m below surface.
        /// </summary>
        public TimeSeries InitialGroundWaterLevelSeries { get; set; }

        /// <summary>
        /// Gets or sets the maximum land storage (mm) of the area (m²).
        /// </summary>
        [Description("Maximum land storage")]
        public double MaximumLandStorage { get; set; }

        /// <summary>
        /// Gets or sets the initial land storage (mm) of the area (m²).
        /// </summary>
        [Description("Initial land storage")]
        public double InitialLandStorage { get; set; }

        /// <summary>
        /// Gets or sets the infiltration capacity (mm/hr) of the area (m²).
        /// </summary>
        [Description("Infiltration capacity")]
        public double InfiltrationCapacity { get; set; }
        
        /// <summary>
        /// Gets or sets the drainage formula used for the unpaved area.
        /// </summary>
        /// <remarks>De Zeeuw-Hellinga, Ernst, Krayenhoff van de Leur.</remarks>
        public IDrainageFormula DrainageFormula { get; set; }

        /// <summary>
        /// Gets or sets the source of seepage in the unpaved area.
        /// </summary>
        public UnpavedEnums.SeepageSourceType SeepageSource
        {
            get => seepageSource;
            set
            {
                seepageSource = value;
                CreateSeepageFunctionsIfNeeded();
            }
        }

        /// <summary>
        /// Gets or sets the constant value of seepage in the unpaved area.
        /// </summary>
        [Description("Seepage")]
        public double SeepageConstant { get; set; }

        /// <summary>
        /// Gets or sets the time series for seepage in the unpaved area in mm/day.
        /// </summary>
        /// <remarks>This is a time series to pick the initial seepage from (so only single value is used).</remarks>
        public TimeSeries SeepageSeries { get; set; }

        /// <summary>
        /// Gets or sets the hydraulic resistance of seepage in the unpaved area in day (C).
        /// </summary>
        public double SeepageH0HydraulicResistance { get; set; }
        
        /// <summary>
        /// Gets or sets the time series for the piezometric level H0 in the unpaved area in m AD.
        /// </summary>
        public TimeSeries SeepageH0Series { get; set; }

        /// <summary>
        /// Switches the drainage formula used for the unpaved area to the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the drainage formula.</typeparam>
        public void SwitchDrainageFormula<T>() where T : IDrainageFormula, new()
        {
            if (DrainageFormula is T)
            {
                return; //nothing to change
            }

            DrainageFormula = new T();
        }

        #region ICloneable Members

        /// <summary>
        /// Creates a new instance that is a copy of the current <see cref="UnpavedData"/> instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            var clone = (UnpavedData)base.Clone();

            clone.areaPerCrop = (CropAreaDictionary)AreaPerCrop.Clone();
            clone.DrainageFormula = (IDrainageFormula)DrainageFormula.Clone();
            clone.SeepageSeries = SeepageSeries != null ? (TimeSeries)SeepageSeries.Clone() : null;
            clone.InitialGroundWaterLevelSeries = InitialGroundWaterLevelSeries != null
                                                      ? (TimeSeries)InitialGroundWaterLevelSeries.Clone()
                                                      : null;
            clone.BoundarySettings = (RainfallRunoffBoundarySettings)BoundarySettings?.Clone();

            return clone;
        }

        #endregion

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
                        InitialGroundWaterLevelSeries = new TimeSeries { Name = "Initial Groundwater Level" };
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
                        SeepageSeries = new TimeSeries { Name = "Seepage" };
                        SeepageSeries.Components.Add(new Variable<double>
                        {
                            Name = "Seepage",
                            Unit = new Unit("mm/day", "mm/day")
                        });
                    }

                    break;
                case UnpavedEnums.SeepageSourceType.H0Series:
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

        /// <inheritdoc />
        public override void DoAfterLinking(IHydroObject target)
        {
            if (!(target is ILateralSource lateralSource))
            {
                return;
            }

            SetBoundarySettingsToSharedInstance(lateralSource);
        }

        private void SetBoundarySettingsToSharedInstance(ILateralSource lateralSource)
        {
            RainfallRunoffBoundarySettings boundarySettingsOfExistingUnpavedData = GetBoundarySettingsOfExistingUnpavedData(lateralSource);
            if (boundarySettingsOfExistingUnpavedData != null)
            {
                BoundarySettings = boundarySettingsOfExistingUnpavedData;
            }
        }

        private static RainfallRunoffBoundarySettings GetBoundarySettingsOfExistingUnpavedData(ILateralSource lateralSource)
        {
            foreach (HydroLink link in lateralSource.Links)
            {
                if (link.Source is Catchment catchment && catchment.ModelData is UnpavedData unpavedData)
                {
                    return unpavedData.BoundarySettings;
                }
            }

            return null;
        }
        
        /// <inheritdoc />
        public override void DoAfterUnlinking()
        {
            CreateUnsharedBoundarySettingCopyOfSharedInstance();
        }

        private void CreateUnsharedBoundarySettingCopyOfSharedInstance()
        {
            BoundarySettings = (RainfallRunoffBoundarySettings)BoundarySettings.Clone();
        }
    }
}