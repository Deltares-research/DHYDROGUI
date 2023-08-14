using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Deserialization
{
    /// <summary>
    /// Class to set the time series data on <see cref="LateralDischargeFunction"/>.
    /// </summary>
    public interface ILateralTimeSeriesSetter
    {
        /// <summary>
        /// Sets the discharge time series data on the provided function.
        /// </summary>
        /// <param name="lateralId"> The lateral ID. </param>
        /// <param name="dischargeFunction"> The (empty) lateral discharge function to be updated with the time series data. </param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="lateralId"/> is null or white space.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="dischargeFunction"/> is <c>null</c>.
        /// </exception>
        void SetDischargeFunction(string lateralId, LateralDischargeFunction dischargeFunction);
    }
}