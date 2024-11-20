using DelftTools.Functions;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.ProjectExplorer
{
    /// <summary>
    /// Water quality function data wrapper
    /// </summary>
    public class WaterQualityBloomFunctionWrapper
    {
        /// <summary>
        /// Creates a water quality function data wrapper with
        /// <param name="functions"/>
        /// </summary>
        /// <param name="functions"> The functions that are wrapped by the water quality function data wrapper </param>
        public WaterQualityBloomFunctionWrapper(IEventedList<IFunction> functions)
        {
            Functions = functions;
        }

        /// <summary>
        /// The functions that are wrapped by the water quality function data wrapper
        /// </summary>
        public IEventedList<IFunction> Functions { get; private set; }
    }
}