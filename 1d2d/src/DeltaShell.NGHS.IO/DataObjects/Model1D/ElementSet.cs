namespace DeltaShell.NGHS.IO.DataObjects.Model1D
{
    public enum ElementSet
    {
        BranchNodes = 1,
        //BranchLinks = 2,
        //UniqueNodesElementSet = 3,
        GridpointsOnBranches = 4,
        ReachSegElmSet = 5,
        //Structures = 6, // (no pumps)
        Pumps = 7,
        //Controllers = 8,
        HBoundaries = 9,
        QBoundaries = 10,
        Laterals = 11,
        Branches = 13,
        Observations = 14,
        Structures = 15,
        Retentions = 16,
        FiniteVolumeGridOnReachSegments = 17,
        FiniteVolumeGridOnGridPoints = 18,
        LateralsOnReachSegments = 19,
        LateralsOnGridPoints = 20,
        ModelWide = 21,
        CrossSection = 22
    }
}