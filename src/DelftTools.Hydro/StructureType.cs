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
        /* End Culverts */
        ExtraResistance,
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