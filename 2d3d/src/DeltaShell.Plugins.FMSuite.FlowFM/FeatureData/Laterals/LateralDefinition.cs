namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals
{
    /// <summary>
    /// Class representing a lateral definition data.
    /// </summary>
    public sealed class LateralDefinition
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="LateralDefinition"/> class.
        /// </summary>
        public LateralDefinition()
        {
            Discharge = new LateralDischarge();
        }

        /// <summary>
        /// Get the lateral discharge.
        /// </summary>
        public LateralDischarge Discharge { get; }
    }
}