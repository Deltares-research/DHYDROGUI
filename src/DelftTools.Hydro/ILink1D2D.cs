using System.Collections;
using DelftTools.Utils;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DelftTools.Hydro
{
    public interface ILink1D2D : INameable, IComparer, IFeature
    {
        /// <summary>
        /// Geometry
        /// Don't throw this redundant property away: needed for NotifyPropertyChannge event [Entity]
        /// </summary>
        IGeometry Geometry { get; set; }

        /// <summary>
        /// The snap tolerance used during creation on map -> for reproducing
        /// </summary>
        double SnapToleranceUsed { get; set; }

        string LongName { get; set; }
        LinkType TypeOfLink { get; set; }
        int DiscretisationPointIndex { get; set; }
        int FaceIndex { get; set; }
        IFeatureAttributeCollection Attributes { get; set; }
    }
}