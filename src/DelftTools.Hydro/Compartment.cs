using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro
{
    [Entity]
    public class Compartment
    {
        private Manhole parentManhole;
        private bool settingParentManhole;

        public Compartment() : this("compartment")
        {
        }

        public Compartment(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        /// <summary>
        /// The manhole that contains this compartment.
        /// </summary>
        [NoNotifyPropertyChange]
        public Manhole ParentManhole
        {
            get { return parentManhole; }
            set {parentManhole = value; }
        }

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

        /// <summary>
        /// The Compartment is an outlet
        /// </summary>
        public bool IsOutletCompartment()
        {
            return this is OutletCompartment;
        }
    }

    public enum CompartmentShape
    {
        [Description("Unknown")] Unknown,
        [Description("RHK")] Rectangular,
        [Description("RND")] Square
    }
}
