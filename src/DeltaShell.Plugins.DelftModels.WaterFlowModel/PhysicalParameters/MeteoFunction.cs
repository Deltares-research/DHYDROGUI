using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters
{
    public class MeteoFunction : Function
    {
        public MeteoFunction()
            : this("Meteo Data")
        {
        }

        public MeteoFunction(string name)
            : base(name)
        {
            Arguments.Add(new Variable<DateTime>("Time")
            {
                InterpolationType = InterpolationType.Linear,
                ExtrapolationType = ExtrapolationType.Constant
            });

            Components.Add(new Variable<double>("Air temperature", new Unit("Degree Celsius", "°C")));
            Components.Add(new Variable<double>("Relative humidity", new Unit("Percentage", "%")));
            Components.Add(new Variable<double>("Cloudiness", new Unit("Percentage", "%")));

            Attributes[FunctionAttributes.LocationType] = FunctionAttributes.Global;
        }

        public IVariable AirTemperature
        {
            get
            {
                return Components[0];
            }
        }

        public IVariable RelativeHumidity
        {
            get
            {
                return Components[1];
            }
        }

        public IVariable Cloudiness
        {
            get
            {
                return Components[2];
            }
        }
    }
}