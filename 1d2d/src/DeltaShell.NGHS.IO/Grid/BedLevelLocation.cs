namespace DeltaShell.NGHS.IO.Grid
{
    /// <summary>
    /// The location of the coverage bed level in the 2d mesh.
    /// </summary>
    public enum BedLevelLocation
    {
        Faces = 1,
        CellEdges = 2,
        NodesMeanLev = 3,
        NodesMinLev = 4,
        NodesMaxLev = 5,
        FacesMeanLevFromNodes = 6
    }
}