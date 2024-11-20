using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Reflection;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.Common.FeatureData
{
    /// <summary>
    /// Class that contains the data object of a FMMeteoField which couples the GUI and model
    /// </summary>
    /// <seealso cref="DeltaShell.Plugins.FMSuite.Common.FeatureData.IFmMeteoField" />
    public class FmMeteoField : IFmMeteoField
    {
        private static readonly IDictionary<FmMeteoComponent, Unit> MeteoQuantityUnits = new Dictionary<FmMeteoComponent, Unit>
        {
            {FmMeteoComponent.Precipitation, new Unit("millimeters per day", "mm day-1")},
        };

        public static FmMeteoField CreateMeteoPrecipitationSeries(FmMeteoLocationType fmMeteoLocationType)
        {
            return new FmMeteoField(FmMeteoComponent.Precipitation) { Quantity = FmMeteoQuantity.Precipitation, FmMeteoLocationType = fmMeteoLocationType};
        }
        
        private readonly IList<FmMeteoComponent> components;
        private FmMeteoQuantity quantity;
        private FmMeteoLocationType fmMeteoLocationType;

        private FmMeteoField(params FmMeteoComponent[] components)
        {
            this.components = components.Distinct().ToList();

            Data = new TimeSeries { Name = "meteo data" };

            foreach (var meteoComponent in components)
            {
                Data.Components.Add(new Variable<double>(meteoComponent.ToString())
                {
                    InterpolationType = InterpolationType.Linear,
                    ExtrapolationType = ExtrapolationType.None,
                    NoDataValue = -999.0,
                    Unit = (IUnit)MeteoQuantityUnits[meteoComponent].Clone()
                });
            }
        }
        public IEnumerable<FmMeteoComponent> Components
        {
            get { return components; }
        }


        private static string CreateName(FmMeteoQuantity meteoQuantity, IList<FmMeteoComponent> components, FmMeteoLocationType fmMeteoLocationType)
        {
            switch (meteoQuantity)
            {
                case FmMeteoQuantity.Precipitation:
                    return meteoQuantity.GetDescription() + " (" + Enum.GetName(typeof(FmMeteoLocationType), fmMeteoLocationType) + ")"; 
                default:
                    throw new NotImplementedException("meteo quantity not supported");
            }
        }

        public FmMeteoQuantity Quantity
        {
            get { return quantity; }
            private set
            {
                quantity = value;
                UpdateName();
            }
        }

        private void UpdateName()
        {
            Name = CreateName(Quantity, components, FmMeteoLocationType);
        }

        public IFeatureData FeatureData { get; set; }
        public FmMeteoLocationType FmMeteoLocationType {
            get { return fmMeteoLocationType; }
            private set
            {
                fmMeteoLocationType = value;
                UpdateName();
            }
        }

        public IFunction Data { get; private set; }//also update the data in FeatureData when setting here?

        public string Name { get; private set; }
                
        public bool Equals(IFmMeteoField other)
        {
            return quantity == other?.Quantity && FmMeteoLocationType == other.FmMeteoLocationType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FmMeteoField) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) quantity;
                hashCode = (hashCode * 397) ^ (int) FmMeteoLocationType;
                return hashCode;
            }
        }

        public object Clone()
        {
            var newFMMeteoField = new FmMeteoField(Components.ToArray())
            {
                Data = (IFunction) Data.Clone(),
                Quantity = Quantity,
                FmMeteoLocationType = FmMeteoLocationType,
                FeatureData = null,//euhm how do we clone this?
            };
            return newFMMeteoField;
        }
    }
}