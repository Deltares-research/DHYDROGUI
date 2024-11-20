using System;
using DelftTools.Hydro;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts
{
    [Entity(FireOnCollectionChange=false)]
    public class GreenhouseData : CatchmentModelData
    {
        private GreenhouseAreaDictionary areaPerGreenhouse;

        /// <summary>
        /// The storage unit.
        /// </summary>
        public const RainfallRunoffEnums.StorageUnit StorageUnit = RainfallRunoffEnums.StorageUnit.mm;
        
        //nhib
        protected GreenhouseData()
            : base(null)
        {
        }

        public GreenhouseData(Catchment catchment) : base(catchment)
        {
            catchment.ModelData = this;
            SurfaceLevel = 1.5;
            SiloCapacity = 200;
            PumpCapacity = 0.02;
        }

        #region properties

        public double SurfaceLevel { get; set; } // m AD

        public GreenhouseAreaDictionary AreaPerGreenhouse
        {
            get
            {
                if (areaPerGreenhouse == null)
                {
                    areaPerGreenhouse = CreateInitializedGreenhouseAreaDictionary();
                }
                return areaPerGreenhouse;
            }
        } // m2

        public override double CalculationArea // m2
        {
            get { return AreaPerGreenhouse?.Sum ?? 0.0; }
            set
            {
                if (Math.Abs(value - base.CalculationArea) < 1e-10)
                    return;

                AreaPerGreenhouse.Reset(GreenhouseEnums.AreaPerGreenhouseType.lessThan500, value);

                base.CalculationArea = value;
            }
        }

        public bool UseSubsoilStorage { get; set; }
        // the following fields are only relevant when UseSubSoilStorage is true
        public double SubSoilStorageArea { get; set; }
        public double SiloCapacity { get; set; } // m3/ha
        public double PumpCapacity { get; set; }

        public RainfallRunoffEnums.AreaUnit TotalAreaUnit { get; set; }

        /// <summary>
        /// The maximum roof storage (mm) of the area (m²).
        /// </summary>
        public double MaximumRoofStorage { get; set; }
        
        /// <summary>
        /// The initial roof storage (mm) of the area (m²).
        /// </summary>
        public double InitialRoofStorage { get; set; }

        #endregion

        public override object Clone()
        {
            var clone = (GreenhouseData) base.Clone();
            clone.areaPerGreenhouse = (GreenhouseAreaDictionary) AreaPerGreenhouse.Clone();
            return clone;
        }

        private GreenhouseAreaDictionary CreateInitializedGreenhouseAreaDictionary()
        {
            var areaPerGreenhouseAreaDictionary = new GreenhouseAreaDictionary();
            foreach (
                GreenhouseEnums.AreaPerGreenhouseType greenHouseType in
                Enum.GetValues(typeof(GreenhouseEnums.AreaPerGreenhouseType)))
            {
                areaPerGreenhouseAreaDictionary.Add(greenHouseType, 0.0);
            }

            areaPerGreenhouseAreaDictionary.SumChanged += (s,e) =>
            {
                // Synchronizes the sum of the area's with the calculation area.
                // This also sets the correct geometry
                base.CalculationArea = e.Sum;
            };

            return areaPerGreenhouseAreaDictionary;
        }
    }
}