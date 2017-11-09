using System.Collections.Generic;

namespace DelftTools.Hydro
{
    public class ManholeCompartment
    {
        /// <summary>
        /// The unique manhole Id that is defined in the GWSW files of the sewer system.
        /// </summary>
        private long manholeId;

        public ManholeCompartment(long id)
        {
            manholeId = id;
        }

        /// <summary>
        /// The shape of the manhole (either square or rectangular).
        /// </summary>
        public ManholeShape Shape { get; set; }

        /// <summary>
        /// The compartments that this manhole contains.
        /// </summary>
        public ICollection<ICompartment> Compartments { get; set; }

        /// <summary>
        /// Length of manhole (mm).
        /// </summary>
        public int ManholeLength { get; set; }

        /// <summary>
        /// Width of manhole (mm).
        /// </summary>
        public int ManholeWidth { get; set; }

        /// <summary>
        /// The area at surface level that this manhole can flood (m2).
        /// </summary>
        public double FloodableArea { get; set; }

        /// <summary>
        /// The bottom level of the manhole compared to Dutch NAP (m).
        /// </summary>
        public double BottomLevel { get; set; }

        /// <summary>
        /// The surface level of the manhole compared to Dutch NAP (m).
        /// </summary>
        public double SurfaceLevel { get; set; }
    }

    public enum ManholeShape
    {
        Rectangular,
        Square
    }
}
