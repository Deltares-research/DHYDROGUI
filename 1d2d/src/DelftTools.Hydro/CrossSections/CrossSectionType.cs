namespace DelftTools.Hydro.CrossSections
{
    public enum CrossSectionType
    {
        ///<summary>
        /// The GIS geometry is leading. The user can edit an Y'Z table but this is derived from the geometry
        ///</summary>
        GeometryBased,
        ///<summary>
        /// The YZ table is leading. The cross section geometry will be shown in maps as a straight line. 
        /// The geometry will be recalculated if the user modifies the y'z table.
        ///</summary>
        YZ,
        ///<summary>
        /// The cross section is defined as tabulated; a table where for each height a total width and flowing width is given.
        /// Delta shell will not support editing of ZW data; it can be stored, send to the modelengine and show the user 
        /// the conveyance table
        ///</summary>
        ZW,
        /// <summary>
        /// A collection of standard cross section-types (arch, cunette, egg etc.), each of which has its own set of
        /// parameters (dimensions) and shape
        /// </summary>
        Standard
    }
}