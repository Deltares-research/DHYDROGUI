using System;

namespace DelftTools.Hydro.CrossSections
{
    [Obsolete("D3DFMIQ-1923 remove cross section")]
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

        Egg,   // not in use -> only closed branches
        Round //not in use -> only closed branches
    }
}