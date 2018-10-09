using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.FMSuite.Common.FeatureData
{
    public class FmMeteoField : IFmMeteoField
    {
        private static readonly IDictionary<FmMeteoComponent, Unit> MeteoQuantityUnits = new Dictionary<FmMeteoComponent, Unit>
        {
            {FmMeteoComponent.Precipitation, new Unit("", "")},
        };

        public static FmMeteoField CreateMeteoPrecipitationSeries()
        {
            return new FmMeteoField() { Quantity = FmMeteoQuantity.Precipitation};
        }
        
        private readonly IList<FmMeteoComponent> components;
        private FmMeteoQuantity quantity;

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


        private static string CreateName(FmMeteoQuantity meteoQuantity, IList<FmMeteoComponent> components)
        {
            switch (meteoQuantity)
            {
                case FmMeteoQuantity.Precipitation:
                    return EnumDescriptionAttributeTypeConverter.GetEnumDescription(meteoQuantity);
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

        [EditAction]
        private void UpdateName()
        {
            Name = CreateName(Quantity, components);
        }

        public IFunction Data { get; private set; }

        public string Name { get; private set; }
    }
}