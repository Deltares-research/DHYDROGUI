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
                                                           new Variable<double>("conveyance")
                                                               {Unit = new Unit("m³/s", "m³/s"), IsEditable = false},
                                                           new Variable<double>("flow area")
                                                               {Unit = new Unit("m²", "m²"), IsEditable = false},
                                                           new Variable<double>("flow width") 
                                                               {Unit = new Unit("m", "m"), IsEditable = false},
                                                           new Variable<double>("wetted perimeter")
                                                               {Unit = new Unit("m", "m"), IsEditable = false},
                                                           new Variable<double>("hydraulic radius")
                                                               {Unit = new Unit("m", "m"), IsEditable = false},
                                                           new Variable<double>("total width")
                                                               {Unit = new Unit("m", "m"), IsEditable = false},
                                                           new Variable<double>("conveyance negative")
                                                               {Unit = new Unit("m³/s", "m³/s"), IsEditable = false}
                                                       });
                conveyanceData.Arguments.Add(new Variable<double>("depth") {Unit = new Unit("m", "m")});
                return conveyanceData;
            }
        }

        public abstract IFunction GetConveyance(ICrossSection crossSection);

    }
}