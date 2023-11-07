using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="GroupablePointFeature"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public class GroupablePointFeatureRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly GroupablePointFeature feature;

        /// <summary>
        /// Initialize a new instance of the <see cref="GroupablePointFeatureRow"/> class.
        /// </summary>
        /// <param name="feature"> The feature to be presented. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="feature"/> is <c>null</c>.
        /// </exception>
        public GroupablePointFeatureRow(GroupablePointFeature feature)
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

        [DisplayName("X")]
        public double X => feature.X;

        [DisplayName("Y")]
        public double Y => feature.Y;

        /// <summary>
        /// Gets the underlying <see cref="GroupablePointFeature"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature() => feature;
    }
}