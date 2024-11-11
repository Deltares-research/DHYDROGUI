using System.Collections.Generic;

namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// Represents a boundary definition in the new style external forcings file (*_bnd.ext).
    /// </summary>
    public sealed class BndExtForceBoundaryData
    {
        /// <summary>
        /// Gets or sets the line number where the boundary data is located.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Gets or sets the quantity name of the boundary.
        /// </summary>
        /// <remarks>
        /// Only for boundary conditions.
        /// </remarks>
        public string Quantity { get; set; }

        /// <summary>
        /// Gets or sets the identifier of a 1D network node to connect to.
        /// </summary>
        /// <remarks>
        /// The boundary will be connected to the grid point on/closest to the network node.
        /// Only used for 1D.
        /// This is an optional value.
        /// </remarks>
        public string NodeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the boundary polyline file (∗.pli).
        /// </summary>
        /// <remarks>
        /// Only used when <see cref="NodeId"/> is not specified.
        /// </remarks>
        public string LocationFile { get; set; }

        /// <summary>
        /// Gets or sets the forcing file names for the boundary.
        /// Can either be a boundary data file (*.bc) or a NetCDF file (*.nc).
        /// </summary>
        public IEnumerable<string> ForcingFiles { get; set; }

        /// <summary>
        /// Gets or sets the Thatcher-Harleman return time (s).
        /// </summary>
        /// <remarks>
        /// The default value of 0s means that the Thatcher-Harleman return time has no impact.
        /// This is an optional value.
        /// </remarks>
        public double ReturnTime { get; set; }

        /// <summary>
        /// Gets or sets the fall velocity for the tracer (m/s).
        /// </summary>
        /// <remarks>
        /// The default value 0m/s means that settling is disabled for this tracer.
        /// This is an optional value.
        /// </remarks>
        public double TracerFallVelocity { get; set; }

        /// <summary>
        /// Gets or sets decay lifetime (τ) for the tracer (s).
        /// </summary>
        /// <remarks>
        /// The default value 0s means that tracer decay is disabled for this tracer.
        /// This is an optional value.
        /// </remarks>
        public double TracerDecayTime { get; set; }

        /// <summary>
        /// Gets or sets the custom width for boundary flow link,
        /// to override default mirrored internal flow link width.
        /// </summary>
        /// <remarks>
        /// Only used for 1D. This is an optional value.
        /// </remarks>
        public double FlowLinkWidth { get; set; }

        /// <summary>
        /// Gets or sets the custom bed level depth below initial water level for boundary point,
        /// to override default mirrored bed level from internal pressure point.
        /// </summary>
        /// <remarks>
        /// Only used for 1D. This is an optional value.
        /// </remarks>
        public double BedLevelDepth { get; set; }
    }
}