using System;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DelftTools.Utils.Reflection;
using GeoAPI.Geometries;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Represents the base class for catchment data.
    /// </summary>
    [Entity]
    public abstract class CatchmentModelData : Unique<long>, INameable, ICloneable
    {
        private double calculationArea;
        private Catchment catchment;

        /// <summary>
        /// Initializes a new instance of the <see cref="CatchmentModelData"/> class with the specified catchment.
        /// </summary>
        /// <param name="catchment">The catchment associated with the data.</param>
        protected CatchmentModelData(Catchment catchment)
        {
            Catchment = catchment;
            CalculationArea = catchment?.GeometryArea ?? 0.0;
            AreaAdjustmentFactor = 1.0;
            MeteoStationName = "";
        }

        /// <summary>
        /// Gets or sets the catchment associated with the model data.
        /// </summary>
        /// <remarks>
        /// When setting a new value, we ensure that the model data on this new catchment is correctly pointing to this model data instance.
        /// </remarks>
        [Aggregation]
        public Catchment Catchment
        {
            get => catchment;
            set
            {
                if (catchment == value)
                {
                    return;
                }
                
                catchment = value;
                if (catchment != null)
                {
                    catchment.ModelData = this;
                }
                
            }
        }

        /// <summary>
        /// Gets or sets the calculation area of the catchment model data.
        /// </summary>
        public virtual double CalculationArea
        {
            get => calculationArea;
            set
            {
                calculationArea = value;

                if (Catchment == null
                    || (Catchment.Geometry != null
                        && !(Catchment.Geometry is IPoint)
                        && !Catchment.IsGeometryDerivedFromAreaSize))
                {
                    return;
                }

                Catchment.IsGeometryDerivedFromAreaSize = true;
                Catchment.SetAreaSize(value);
            }
        }

        /// <summary>
        /// Gets or sets the name of the meteorological station associated with the catchment model data.
        /// </summary>
        public string MeteoStationName { get; set; }

        /// <summary>
        /// Gets or sets the name of the temperature station associated with the catchment model data.
        /// </summary>
        public string TemperatureStationName { get; set; }

        /// <summary>
        /// Gets or sets the area adjustment factor for the catchment model data.
        /// </summary>
        public double AreaAdjustmentFactor { get; set; }

        /// <summary>
        /// Gets or sets the name of the catchment associated with the model data.
        /// </summary>
        public string Name
        {
            get => Catchment.Name;
            set => Catchment.Name = value;
        }
        
        /// <summary>
        /// Function that should be called after the catchment related to the model data has been linked to the given target.
        /// </summary>
        /// <param name="target">The target object that has been linked to.</param>
        public virtual void DoAfterLinking(IHydroObject target){}
        
        /// <summary>
        /// Function that should be called after the catchment related to the model data has been unlinked.
        /// </summary>
        public virtual void DoAfterUnlinking(){}

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public virtual object Clone()
        {
            CatchmentModelData clone = TypeUtils.MemberwiseClone(this); //copies members of subclasses
            clone.Catchment = Catchment;                                //aggregation, so no cloning
            clone.CalculationArea = CalculationArea;
            return clone;
        }
    }
}