using DelftTools.Functions;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.ProjectExplorer
{
    /// <summary>
    /// Water quality function wrapper
    /// </summary>
    public class WaterQualityFunctionWrapper
    {
        private readonly IFunction function;

        /// <summary>
        /// Creates a water quality function wrapper with
        /// <param name="function"/>
        /// </summary>
        /// <param name="function"> The function that is wrapped by the water quality function wrapper </param>
        public WaterQualityFunctionWrapper(IFunction function)
        {
            this.function = function;
        }

        /// <summary>
        /// The function that is wrapped by the water quality function wrapper
        /// </summary>
        public IFunction Function => function;
    }
}