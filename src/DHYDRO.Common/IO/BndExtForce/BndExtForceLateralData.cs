using System.Collections.Generic;

namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// Represents a lateral discharge definition in the new style external forcings file (*_bnd.ext).
    /// </summary>
    public sealed class BndExtForceLateralData
    {
        /// <summary>
        /// Gets or sets the line number where the lateral discharge data is located.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Gets or sets the lateral discharge identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the lateral discharge name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the lateral discharge location type.
        /// </summary>
        /// <remarks>
        /// Only used when <see cref="XCoordinates"/> and <see cref="YCoordinates"/> are specified.
        /// This is an optional value. Defaults to <see cref="BndExtForceLocationType.All"/>.
        /// </remarks>
        public BndExtForceLocationType LocationType { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the 1D network node to connect to.
        /// </summary>
        /// <remarks>
        /// The lateral discharge will be connected to the grid point on/closest to the network node.
        /// For a 1D point lateral.
        /// This is an optional value.
        /// </remarks>
        public string NodeId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the branch on which the lateral discharge is located.
        /// </summary>
        /// <remarks>
        /// For a 1D point lateral. Used in combination with <see cref="Chainage"/>.
        /// This is an optional value.
        /// </remarks>
        public string BranchId { get; set; }

        /// <summary>
        /// Gets or sets the lateral discharge location (m) on the branch.
        /// </summary>
        /// <remarks>
        /// For a 1D point lateral. Used in combination with <see cref="BranchId"/>.
        /// This is an optional value, defaults to <see cref="double.NaN"/>.
        /// </remarks>
        public double Chainage { get; set; } = double.NaN;

        /// <summary>
        /// Gets or sets the number of values in <see cref="XCoordinates"/> and <see cref="YCoordinates"/>.
        /// </summary>
        /// <remarks>
        /// Only used when <see cref="XCoordinates"/> and <see cref="YCoordinates"/> are specified.
        /// This is an optional value.
        /// </remarks>
        public int NumCoordinates { get; set; }

        /// <summary>
        /// Gets or sets the x-coordinates of the lateral discharge location polygon (m).
        /// </summary>
        /// <remarks>
        /// This is an optional value.
        /// </remarks>
        public IEnumerable<double> XCoordinates { get; set; }

        /// <summary>
        /// Gets or sets the y-coordinates of the lateral discharge location polygon (m).
        /// </summary>
        /// <remarks>
        /// This is an optional value.
        /// </remarks>
        public IEnumerable<double> YCoordinates { get; set; }

        /// <summary>
        /// Gets or sets the name of the lateral discharge polygon file (∗.pol).
        /// </summary>
        /// <remarks>
        /// This is an optional value.
        /// </remarks>
        public string LocationFile { get; set; }

        /// <summary>
        /// Gets or sets the prescribed discharge for the lateral.
        /// </summary>
        /// <remarks>
        /// Can contain either a constant value, a time series file name, or it can be externally controlled.
        /// </remarks>
        public BndExtForceDischargeData Discharge { get; set; }
    }
}