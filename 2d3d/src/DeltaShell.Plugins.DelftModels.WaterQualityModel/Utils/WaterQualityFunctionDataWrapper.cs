using DelftTools.Functions;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils
{
    /// <summary>
    /// Water quality function data wrapper
    /// </summary>
    public class WaterQualityFunctionDataWrapper
    {
        /// <summary>
        /// Creates a water quality function data wrapper with
        /// <param name="functions"/>
        /// </summary>
        /// <param name="functions"> The functions that are wrapped by the water quality function data wrapper </param>
        public WaterQualityFunctionDataWrapper(IEventedList<IFunction> functions)
        {
            Functions = functions;
        }

        /// <summary>
        /// The functions that are wrapped by the water quality function data wrapper
        /// </summary>
        public IEventedList<IFunction> Functions { get; private set; }
    }
}