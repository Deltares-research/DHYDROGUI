using System;
using System.ComponentModel;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;

namespace DelftTools.Hydro
{
    public class Compartment : Node, INameable
    {
        private string description;

        public Compartment(string uniqueId)
        {
            Name = uniqueId;
        }

        /// <summary>
        /// Unique Id for this Compartment.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The manhole that contains this compartment.
        /// </summary>
        public Manhole ParentManhole { get; set; }

        /// <summary>
        /// The shape of the manhole (either square or rectangular).
        /// </summary>
        public CompartmentShape Shape { get; set; }

        /// <summary>
        /// Length of manhole (mm).
        /// </summary>
        public double ManholeLength { get; set; }

        /// <summary>
        /// Width of manhole (mm).
        /// </summary>
        public double ManholeWidth { get; set; }

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

    public enum CompartmentShape
    {
        [Description("")] Unknown,
        [Description("RHK")] Rectangular,
        [Description("RND")] Square
    }
}
