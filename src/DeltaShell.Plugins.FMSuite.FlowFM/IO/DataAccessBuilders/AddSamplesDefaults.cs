using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders
{
    /// <summary>
    /// Contains all the default for an <see cref="AddSamplesOperation"/>.
    /// </summary>
    public static class AddSamplesDefaults
    {
        /// <summary>
        /// The default file type.
        /// </summary>
        public const int FileType = ExtForceQuantNames.FileTypes.Triangulation;
        
        /// <summary>
        /// The default method.
        /// </summary>
        public const int Method = 6;
        
        /// <summary>
        /// The default operand.
        /// </summary>
        public const Operator Operand = Operator.Overwrite;
        
        /// <summary>
        /// The default averaging type.
        /// </summary>
        public const GridCellAveragingMethod AveragingType = GridCellAveragingMethod.ClosestPoint;
        
        /// <summary>
        /// The default relative search cell size.
        /// </summary>
        public const double RelSearchCellSize = 1.0;
    }
}