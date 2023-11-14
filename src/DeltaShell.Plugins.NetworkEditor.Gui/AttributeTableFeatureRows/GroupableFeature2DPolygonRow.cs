using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="GroupableFeature2DPolygon"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public class GroupableFeature2DPolygonRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly GroupableFeature2DPolygon feature;

        /// <summary>
        /// Initialize a new instance of the <see cref="GroupableFeature2DPolygonRow"/> class.
        /// </summary>
        /// <param name="feature"> The feature to be presented. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="feature"/> is <c>null</c>.
        /// </exception>
        public GroupableFeature2DPolygonRow(GroupableFeature2DPolygon feature)
            : base(feature)
        {
            Ensure.NotNull(feature, nameof(feature));
            this.feature = feature;
        }

        [DisplayName("Group name")]
        public string GroupName
        {
            get => feature.GroupName;
            set => feature.GroupName = value;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => feature.Name;
            set => feature.SetNameIfValid(value);
        }

        /// <summary>
        /// Gets the underlying <see cref="GroupableFeature2DPolygon"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return feature;
        }
    }
}