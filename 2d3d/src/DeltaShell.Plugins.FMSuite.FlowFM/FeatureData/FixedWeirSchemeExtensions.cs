namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    /// <summary>
    /// Contains extensions for <seealso cref="FixedWeirSchemes"/>
    /// </summary>
    public static class FixedWeirSchemeExtensions
    {
        /// <summary>
        /// Gets the minimal valid ground height for a fixed weir scheme.
        /// </summary>
        /// <param name="scheme">The fixed weir scheme.</param>
        /// <returns>Returns the minimal allowed ground height for this fixed weir scheme.</returns>
        public static double GetMinimalAllowedGroundHeight(this FixedWeirSchemes scheme)
        {
            switch (scheme)
            {
                case FixedWeirSchemes.Scheme6:
                case FixedWeirSchemes.Scheme9:
                    return 0.0d;
                case FixedWeirSchemes.Scheme8:
                    return 0.1d;
                default:
                    return 0.0d;
            }
        }
    }
}