using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="Gully"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public class GullyRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly Gully gully;

        /// <summary>
        /// Initialize a new instance of the <see cref="GullyRow"/> class.
        /// </summary>
        /// <param name="gully"> The gully to be presented. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="gully"/> is <c>null</c>.
        /// </exception>
        public GullyRow(Gully gully)
            : base(gully)
        {
            Ensure.NotNull(gully, nameof(gully));
            this.gully = gully;
        }

        [DisplayName("Group name")]
        public string GroupName
        {
            get => gully.GroupName;
            set => gully.GroupName = value;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => gully.Name;
            set => gully.SetNameIfValid(value);
        }

        [DisplayName("X")]
        public double X => gully.X;

        [DisplayName("Y")]
        public double Y => gully.Y;

        /// <summary>
        /// Gets the underlying <see cref="Gully"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return gully;
        }
    }
}