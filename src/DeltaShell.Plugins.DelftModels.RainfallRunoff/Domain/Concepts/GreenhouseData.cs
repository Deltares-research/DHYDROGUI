using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts
{
    [Entity(FireOnCollectionChange=false)]
    public class GreenhouseData : CatchmentModelData
    {
        /// <summary>
        /// The storage unit.
        /// </summary>
        public const RainfallRunoffEnums.StorageUnit StorageUnit = RainfallRunoffEnums.StorageUnit.mm;
        
        //nhib
        protected GreenhouseData()
            : base(null)
        {
            InitializeAreas();
        }

        public GreenhouseData(Catchment catchment) : base(catchment)
        {
            InitializeAreas();
            CalculationArea = catchment.AreaSize;
            SurfaceLevel = 1.5;
            SiloCapacity = 200;
            PumpCapacity = 0.02;
        }

        private void InitializeAreas()
        {
            AreaPerGreenhouse = new GreenhouseAreaDictionary();
            foreach (
                GreenhouseEnums.AreaPerGreenhouseType greenHouseType in
                    Enum.GetValues(typeof (GreenhouseEnums.AreaPerGreenhouseType)))
            {
                AreaPerGreenhouse.Add(greenHouseType, 0.0);
            }
        }

        #region properties

        public double SurfaceLevel { get; set; } // m AD

        public GreenhouseAreaDictionary AreaPerGreenhouse { get; private set; } // m2

        public override double CalculationArea // m2
        {
            get { return AreaPerGreenhouse.Values.Sum(); }
            set
            {
                if (value == CalculationArea)
                    return;

                AfterCalculationAreaSet(value);
            }
        }

        [EditAction]
        private void AfterCalculationAreaSet(double value)
        {
            AreaPerGreenhouse.Reset(GreenhouseEnums.AreaPerGreenhouseType.lessThan500, value);
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
            clone.AreaPerGreenhouse = (GreenhouseAreaDictionary) AreaPerGreenhouse.Clone();
            return clone;
        }
    }
}