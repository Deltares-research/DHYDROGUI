using System.Collections;
using System.ComponentModel;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.Link1d2d
{
    public interface ILink1D2D : IComparer, IFeature
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

        [DisplayName("Name")]
        [FeatureAttribute(Order = 1, ExportName = "Name")]
        string Name { get; set; }

        [DisplayName("Long name")]
        [FeatureAttribute(Order = 2, ExportName = "Long name")]
        string LongName { get; set; }

        [DisplayName("Type of link")]
        [FeatureAttribute(Order = 3, ExportName = "Type")]
        LinkStorageType TypeOfLink { get; set; }

        [DisplayName("Point index")]
        [FeatureAttribute(Order = 4, ExportName = "Point index")]
        [ReadOnly(true)]
        int DiscretisationPointIndex { get; set; }

        [DisplayName("Cell index")]
        [FeatureAttribute(Order = 5, ExportName = "Cell index")]
        [ReadOnly(true)]
        int FaceIndex { get; set; }

        [DisplayName("Link1D2D index")]
        [FeatureAttribute(Order = 6, ExportName = "Link1D2D index")]
        [ReadOnly(true)]
        int Link1D2DIndex { get; set; }
        IFeatureAttributeCollection Attributes { get; set; }

        Coordinate GetCenter();
    }
}