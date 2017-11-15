using System.ComponentModel;
using GeoAPI.Geometries;

namespace DelftTools.Hydro
{
    public class Manhole
    {
        public Manhole(string id)
        {
            Id = id;
        }

        /// <summary>
        /// The unique manhole Id that is defined in the GWSW files of the sewer system.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The shape of the manhole (either square or rectangular).
        /// </summary>
        public ManholeShape Shape { get; set; }

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

        /// <summary>
        /// The coordinates of the manhole.
        /// </summary>
        public Coordinate Coordinates { get; set; }
    }

    public enum ManholeShape
    {
        [Description("")] Unknown,
        [Description("RHK")] Rectangular,
        [Description("RND")] Square
    }
}
