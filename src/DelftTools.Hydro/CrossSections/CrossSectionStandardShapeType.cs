namespace DelftTools.Hydro.CrossSections
{
    public enum CrossSectionStandardShapeType
    {
        /// <summary>
        /// rectangular cross section standard shape. Has parameters for width and height.
        /// </summary>
        Rectangle,
        
        Arch,

/*        AsymetricalTrapzium, TODO: Needs validation resolution*/

        Cunette,

        

        Elliptical,

        SteelCunette,

        Trapezium,
        
        Egg,// not in use -> only closed branches
        Round, //not in use -> only closed branches

    }
}
