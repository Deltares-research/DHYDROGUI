using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView
{
    /// <summary>
    /// Class for updating an existing water level function of the side view.
    /// </summary>
    public static class SideViewWaterLevelFunctionUpdater
    {
        /// <summary>
        /// Updates an existing water level function with two new data points for each given structure.
        /// One data point is added just before the location of structure and the other point is added
        /// right after the location of the structure.
        /// <example>
        /// If the structure has a chainage of 10. Two additional points are added to an existing function
        /// at chainage 9.999 and 10.001. The water levels for these new data points are equal to the water
        /// levels closest to the point before the structure and after the structure.
        /// </example>
        /// </summary>
        /// <param name="function">The water level function to update.</param>
        /// <param name="structureChainages">The chainages at which structures can be found.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        public static void UpdateFunctionWithExtraDataPointsForStructures(IFunction function,
                                                                          IReadOnlyList<double> structureChainages)
        {
            Ensure.NotNull(function, nameof(function));
            Ensure.NotNull(structureChainages, nameof(structureChainages));
            
            if (!structureChainages.Any())
            {
                return;
            }

            double[] chainages = GetChainagesFromFunction(function);
            double[] waterLevels = GetWaterLevelsFromFunction(function);

            var calculator = new SideViewFunctionDataCalculator();
            calculator.Calculate(chainages, waterLevels, structureChainages);

            UpdateWaterLevelFunction(function, calculator.OutputChainages, calculator.OutputValues);
        }

        private static double[] GetChainagesFromFunction(IFunction function)
        {
            return function.Arguments[0].GetValues<double>().ToArray();
        }

        private static double[] GetWaterLevelsFromFunction(IFunction function)
        {
            return function.Components[0].GetValues<double>().ToArray();
        }
        
        private static void UpdateWaterLevelFunction(IFunction function,
                                                     IEnumerable<double> updatedChainages,
                                                     IEnumerable<double> updatedWaterLevels)
        {
            function.Arguments[0].Clear();
            function.Components[0].Clear();
            function.Arguments[0].SetValues(updatedChainages);
            function.Components[0].SetValues(updatedWaterLevels);
        }
    }
}