using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="LandBoundary2D"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public class LandBoundary2DRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly LandBoundary2D landBoundary2D;

        /// <summary>
        /// Initialize a new instance of the <see cref="LandBoundary2DRow"/> class.
        /// </summary>
        /// <param name="landBoundary2D"> The land boundary 2D to be presented. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="landBoundary2D"/> is <c>null</c>.
        /// </exception>
        public LandBoundary2DRow(LandBoundary2D landBoundary2D)
            : base(landBoundary2D)
        {
            Ensure.NotNull(landBoundary2D, nameof(landBoundary2D));
            this.landBoundary2D = landBoundary2D;
        }

        [DisplayName("Group name")]
        public string GroupName
        {
            get => landBoundary2D.GroupName;
            set => landBoundary2D.GroupName = value;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => landBoundary2D.Name;
            set => landBoundary2D.SetNameIfValid(value);
        }

        /// <summary>
        /// Gets the underlying <see cref="LandBoundary2D"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return landBoundary2D;
        }
    }
}