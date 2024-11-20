namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions
{
    /// <summary>
    /// <see cref="GridSide"/> defines the four sides of a grid clockwise:
    /// * West  : ( 0, 0 ) .. ( 0, N )
    /// * North : ( 0, N ) .. ( M, N )
    /// * East  : ( M, N ) .. ( M, 0 )
    /// * South : ( M, 0 ) .. ( 0, 0 )
    /// </summary>
    public enum GridSide
    {
        West = 1,
        North = 2,
        East = 3,
        South = 4
    }
}