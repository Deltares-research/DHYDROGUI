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
                    CreateDoubleVariable("conveyance", "m3/s"),
                    CreateDoubleVariable("flow area", "m²"),
                    CreateDoubleVariable("flow width", "m"),
                    CreateDoubleVariable("wetted perimeter", "m"),
                    CreateDoubleVariable("hydraulic radius", "m"),
                    CreateDoubleVariable("total width", "m"),
                    CreateDoubleVariable("conveyance negative", "m3/s")
                });
                conveyanceData.Arguments.Add(CreateDoubleVariable("depth", "m", true));

                return conveyanceData;
            }
        }

        private static Variable<double> CreateDoubleVariable(string variableName, string unitName, bool isEditable = false)
        {
            return new Variable<double>(variableName)
            {
                Unit = new Unit(unitName, unitName),
                IsEditable = isEditable
            };
        }

        public abstract IFunction GetConveyance(ICrossSection crossSection);
    }
}