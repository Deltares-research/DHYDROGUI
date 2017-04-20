using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    public class WindFunction : Function
    {
        public WindFunction()
            : this("wind velocity")
        {
        }

        public WindFunction(string name)
            : base (name)
        {
            Arguments.Add(new Variable<DateTime>("Time")
            {
                InterpolationType = InterpolationType.Linear, 
                ExtrapolationType = ExtrapolationType.Constant
            });
            Components.Add(new Variable<double>("Wind Velocity", new Unit("Velocity", "m/s")));
            Components.Add(new Variable<double>("Wind Direction", new Unit("Direction", "deg"))
            {
                MinValidValue = 0.0, 
                MaxValidValue = 360.0
            });

            Attributes[FunctionAttributes.LocationType] = FunctionAttributes.Global;
        }

        public IVariable Velocity
        {
            get
            {
                return Components[0];
            }
        }

        public IVariable Direction
        {
            get
            {
                return Components[1];
            }
        }
    }
}