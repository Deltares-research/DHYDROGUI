using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;

namespace DeltaShell.Plugins.FMSuite.Common.FeatureData
{
    public class UniformWindField : IWindField
    {
        private static readonly IDictionary<WindComponent, Unit> WindQuantityUnits = new Dictionary<WindComponent, Unit>
        {
            {WindComponent.X, new Unit("meter per second", "m/s")},
            {WindComponent.Y, new Unit("meter per second", "m/s")},
            {WindComponent.Pressure, new Unit("pascal", "Pa")},
            {WindComponent.Magnitude, new Unit("meter per second", "m/s")},
            {WindComponent.Angle, new Unit("degrees", "deg")}
        };

        private readonly IList<WindComponent> components;
        private WindQuantity quantity;

        private UniformWindField(params WindComponent[] components)
        {
            this.components = components.Distinct().ToList();

            Data = new TimeSeries {Name = "wind velocity"};

            foreach (WindComponent windComponent in components)
            {
                Data.Components.Add(new Variable<double>(windComponent.ToString())
                {
                    InterpolationType = InterpolationType.Linear,
                    ExtrapolationType = ExtrapolationType.None,
                    NoDataValue = -999.0,
                    Unit = (IUnit) WindQuantityUnits[windComponent].Clone()
                });
            }
        }

        public IEnumerable<WindComponent> Components => components;

        public WindQuantity Quantity
        {
            get => quantity;
            private set
            {
                quantity = value;
                UpdateName();
            }
        }

        public IFunction Data { get; private set; }

        public string Name { get; private set; }

        public static UniformWindField CreateWindXSeries()
        {
            return new UniformWindField(WindComponent.X) {Quantity = WindQuantity.VelocityX};
        }

        public static UniformWindField CreateWindYSeries()
        {
            return new UniformWindField(WindComponent.Y) {Quantity = WindQuantity.VelocityY};
        }

        public static UniformWindField CreateWindXYSeries()
        {
            return new UniformWindField(WindComponent.X, WindComponent.Y) {Quantity = WindQuantity.VelocityVector};
        }

        public static UniformWindField CreateWindPolarSeries()
        {
            return new UniformWindField(WindComponent.Magnitude, WindComponent.Angle) {Quantity = WindQuantity.VelocityVector};
        }

        public static UniformWindField CreatePressureSeries()
        {
            return new UniformWindField(WindComponent.Pressure) {Quantity = WindQuantity.AirPressure};
        }

        private static string CreateName(WindQuantity windQuantity, IList<WindComponent> components)
        {
            switch (windQuantity)
            {
                case WindQuantity.VelocityX:
                    return "Uniform x-field";
                case WindQuantity.VelocityY:
                    return "Uniform y-field";
                case WindQuantity.VelocityVector:
                    if (components.Contains(WindComponent.Magnitude))
                    {
                        return "Uniform mag/dir";
                    }

                    return "Uniform xy-field";
                case WindQuantity.AirPressure:
                    return "Uniform pressure";
                default:
                    throw new NotImplementedException("wind quantity not supported");
            }
        }

        private void UpdateName()
        {
            Name = CreateName(Quantity, components);
        }
    }
}