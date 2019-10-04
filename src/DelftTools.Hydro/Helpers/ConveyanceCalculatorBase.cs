using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.CrossSections;
using DelftTools.Units;

namespace DelftTools.Hydro.Helpers
{
    /// <summary>
    /// Adds a get empty conveyance function to the IConveyanceCalculator interface.
    /// </summary>
    public abstract class ConveyanceCalculatorBase : IConveyanceCalculator
    {
        protected static IFunction GetEmptyConveyanceFunction()
        {
            {
                var conveyanceData = new Function("cross_section_processed_data");

                conveyanceData.Components.AddRange(new[]
                {
                    GetDoubleVariable("conveyance", "m3/s"),
                    GetDoubleVariable("flow area", "m²"),
                    GetDoubleVariable("flow width", "m"),
                    GetDoubleVariable("wetted perimeter", "m"),
                    GetDoubleVariable("hydraulic radius", "m"),
                    GetDoubleVariable("total width", "m"),
                    GetDoubleVariable("conveyance negative", "m3/s")
                });
                conveyanceData.Arguments.Add(new Variable<double>("depth")
                {
                    Unit = new Unit("m", "m")
                });
                return conveyanceData;
            }
        }

        private static Variable<double> GetDoubleVariable(string variableName, string unitName)
        {
            return new Variable<double>(variableName)
            {
                Unit = new Unit(unitName, unitName),
                IsEditable = false
            };
        }

        public abstract IFunction GetConveyance(ICrossSection crossSection);
    }
}