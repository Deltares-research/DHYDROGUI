using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="ThinDam2D"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public class ThinDam2DRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly ThinDam2D thinDam2D;

        /// <summary>
        /// Initialize a new instance of the <see cref="ThinDam2DRow"/> class.
        /// </summary>
        /// <param name="thinDam2D"> The thin dam 2D to be presented. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="thinDam2D"/> is <c>null</c>.
        /// </exception>
        public ThinDam2DRow(ThinDam2D thinDam2D)
            : base(thinDam2D)
        {
            Ensure.NotNull(thinDam2D, nameof(thinDam2D));
            this.thinDam2D = thinDam2D;
        }

        [DisplayName("Group name")]
        public string GroupName
        {
            get => thinDam2D.GroupName;
            set => thinDam2D.GroupName = value;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => thinDam2D.Name;
            set => thinDam2D.SetNameIfValid(value);
        }

        /// <summary>
        /// Gets the underlying <see cref="ThinDam2D"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return thinDam2D;
        }
    }
}