using DHYDRO.Common.IO.ExtForce;
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
        public const int FileType = ExtForceFileConstants.FileTypes.Triangulation;
        
        /// <summary>
        /// The default method.
        /// </summary>
        public const int Method = ExtForceFileConstants.Methods.Averaging;
        
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
        public const double RelSearchCellSize = 1.01;
    }
}