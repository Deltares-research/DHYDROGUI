namespace DelftTools.Hydro
{
    public enum StructureType
    {
        Unknown,
        /* Bridges */
        Bridge,
        BridgePillar,
        /* End Bridges */
        CompositeBranchStructure,
        /* Culverts */
        Culvert,
        InvertedSiphon,
        Gate,
        Pump,
        /* Weirs */
        Weir,
        UniversalWeir,
        RiverWeir,
        AdvancedWeir,
        Orifice,
        GeneralStructure
        /* End Weirs */,
        ObservationPoint
    }
}