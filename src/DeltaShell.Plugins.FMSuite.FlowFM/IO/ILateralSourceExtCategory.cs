namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// Represents a delft ini category specific for lateral source data from the external forcings file.
    /// </summary>
    public interface ILateralSourceExtCategory
    {
        /// <summary>
        /// The id of the lateral source.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The name of the lateral source.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The name of the node the lateral source is on.
        /// </summary>
        string NodeName { get; }

        /// <summary>
        /// The name of the branch the lateral source is on.
        /// </summary>
        string BranchName { get; }

        /// <summary>
        /// The chainage of the lateral source on the branch it is on.
        /// </summary>
        double Chainage { get; }

        /// <summary>
        /// The name of the boundary conditions file with the discharge data.
        /// </summary>
        string DischargeFile { get; }

        /// <summary>
        /// The constant discharge.
        /// </summary>
        double Discharge { get; }
    }
}