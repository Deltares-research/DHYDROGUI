using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.Structures
{
    public interface ICompartment : ISewerFeature, INameable
    {
        /// <summary>
        /// The manhole that contains this compartment.
        /// </summary>
        IManhole ParentManhole { get; set; }
        
        /// <summary>
        /// The Name of the manhole that contains this compartment.
        /// </summary>
        string ParentManholeName { get; }

        /// <summary>
        /// Geometry of the compartment
        /// </summary>
        IGeometry Geometry { get; set; }

        /// <summary>
        /// The surface level of the manhole compared to Dutch NAP (m).
        /// </summary>
        double SurfaceLevel { get; set; }

        /// <summary>
        /// The bottom level of the manhole compared to Dutch NAP (m).
        /// </summary>
        double BottomLevel { get; set; }

        /// <summary>
        /// Width of manhole (m).
        /// </summary>
        double ManholeWidth { get; set; }

        /// <summary>
        /// Length of manhole (m).
        /// </summary>
        double ManholeLength { get; set; }

        /// <summary>
        /// The area at surface level that this manhole can flood (m2).
        /// </summary>
        double FloodableArea { get; set; }

        /// <summary>
        /// The shape of the manhole (either square or rectangular).
        /// </summary>
        CompartmentShape Shape { get; set; }
        
        /// <summary>
        /// Storage type of compartment (like reservoir or closed)
        /// </summary>
        CompartmentStorageType CompartmentStorageType { get; set; }

        /// <summary>
        /// Use storage as function of level
        /// </summary>
        bool UseTable { get; set; }
        
        /// <summary>
        /// Contains the storage table
        /// </summary>
        IFunction Storage { get; set; }

        /// <summary>
        /// Interpolation used for <see cref="Storage"/>
        /// </summary>
        InterpolationType InterpolationType { get; set; }
    }

    public enum CompartmentStorageType
    {
        Reservoir,
        Closed
    }
}
