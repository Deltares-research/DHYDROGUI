using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries
{
    /// <summary>
    /// <see cref="BoundaryOrientationType"/> defines the possible values for the
    /// Orientation field in the Boundary Category.
    /// </summary>
    public enum BoundaryOrientationType
    {
        [Description(KnownWaveBoundariesFileConstants.EastBoundaryOrientationType)]
        East,

        [Description(KnownWaveBoundariesFileConstants.NorthEastBoundaryOrientationType)]
        NorthEast,

        [Description(KnownWaveBoundariesFileConstants.NorthBoundaryOrientationType)]
        North,

        [Description(KnownWaveBoundariesFileConstants.NorthWestBoundaryOrientationType)]
        NorthWest,

        [Description(KnownWaveBoundariesFileConstants.WestBoundaryOrientationType)]
        West,

        [Description(KnownWaveBoundariesFileConstants.SouthWestBoundaryOrientationType)]
        SouthWest,

        [Description(KnownWaveBoundariesFileConstants.SouthBoundaryOrientationType)]
        South,

        [Description(KnownWaveBoundariesFileConstants.SouthEastBoundaryOrientationType)]
        SouthEast
    }
}