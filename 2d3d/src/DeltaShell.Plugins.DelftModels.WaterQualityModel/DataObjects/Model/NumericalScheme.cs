using System.ComponentModel;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model
{
    /// <summary>
    /// Numerical scheme enumeration for 1D
    /// </summary>
    /// <remarks>
    /// The enumeration indices correspond to the following delwaq definitions:
    /// 1 - 1st order upwind in time and space
    /// 2 - Modified 2nd order Runge Kutta in time, 1st order upwind in space
    /// 3 - 2nd order Lax Wendroff
    /// 4 - Alternating direction implicit
    /// 5 - 2nd order Flux Corrected Transport (Boris and Book)
    /// 6 - Implicit steady state, direct method, 1st order upwind
    /// 7 - Implicit steady state, direct method 2nd order
    /// 8 - Iterative steady state, backward differences
    /// 9 - Iterative steady state, central differences
    /// 10 - Implicit, direct method, 1st order upwind
    /// 11 - Horizontally method 1, vertically implicit 2nd order
    /// 12 - Horizontally method 5, vertically implicit 2nd order
    /// 13 - Horizontally method 1, vertically implicit 1st order
    /// 14 - Horizontally method 5, vertically implicit 1st order
    /// 15 - Implicit iterative Method, 1st order upwind in space and time
    /// 16 - Implicit Iterative Method, 1st order upwind in horizontal and time, 2nd order vertically
    /// 17 - Iterative steady state, 1st order upwind in space
    /// 18 - Iterative steady state, 1st order upwind horizontally, 2nd order central vertically
    /// 19 - ADI horizontally, implicit 1st order upwind vertically
    /// 20 - ADI horizontally, implicit 2nd order central vertically
    /// 21 - Self adapting Theta Method, implicit vertically, FCT (Zalezac)
    /// 22 - Self adapting Theta Method, implicit vertically, FCT (Boris and Book)
    /// </remarks>
    public enum NumericalScheme
    {
        [Description("Scheme 1")]
        Scheme1 = 1,

        [Description("Scheme 5")]
        Scheme5 = 5,

        [Description("Scheme 10")]
        Scheme10 = 10,

        [Description("Scheme 11")]
        Scheme11 = 11,

        [Description("Scheme 12")]
        Scheme12 = 12,

        [Description("Scheme 13")]
        Scheme13 = 13,

        [Description("Scheme 14")]
        Scheme14 = 14,

        [Description("Scheme 15")]
        Scheme15 = 15,

        [Description("Scheme 16")]
        Scheme16 = 16,

        [Description("Scheme 21")]
        Scheme21 = 21,

        [Description("Scheme 22")]
        Scheme22 = 22
    }

    public static class NumericalScheme1DExtensions
    {
        /// <summary>
        /// Determines whether the given scheme uses iterations (convergence) or not.
        /// </summary>
        public static bool IsIterativeCalculationScheme(this NumericalScheme scheme)
        {
            return scheme == NumericalScheme.Scheme15 ||
                   scheme == NumericalScheme.Scheme16 ||
                   scheme == NumericalScheme.Scheme21 ||
                   scheme == NumericalScheme.Scheme22;
        }
    }
}