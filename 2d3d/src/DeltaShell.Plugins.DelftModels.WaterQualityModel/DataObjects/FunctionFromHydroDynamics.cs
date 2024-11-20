using DelftTools.Functions;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects
{
    /// <summary>
    /// 'Placeholder' function for data coming from hydro dynamics data.
    /// </summary>
    public class FunctionFromHydroDynamics : Function
    {
        /// <summary>
        /// The file path comes directly from the hyd file, so it is a relative path.
        /// </summary>
        public virtual string FilePath { get; set; }
    }
}