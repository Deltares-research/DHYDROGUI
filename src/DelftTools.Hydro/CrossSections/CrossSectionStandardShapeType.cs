namespace DelftTools.Hydro.CrossSections
{
    public enum CrossSectionStandardShapeType
    {
        /// <summary>
        /// rectangular cross section standard shape. Has parameters for width and height.
        /// </summary>
        Rectangle,
        
        Arch,

        Cunette,
        
        Elliptical,

        SteelCunette,

        Trapezium,
        
        Egg,// not in use -> only closed branches
        Circle, //not in use -> only closed branches
        InvertedEgg,// not in use -> only closed branches
        UShape,// not in use -> only closed branches

    }
}
